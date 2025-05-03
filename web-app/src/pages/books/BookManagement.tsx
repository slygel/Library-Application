import React, { useState, useEffect } from "react"
import {Pencil, Trash2, PlusSquare, ChevronDown, BookOpen, SearchIcon} from "lucide-react"
import { Category } from "../../types/Category.ts"
import { Book } from "../../types/Book.ts"
import BookForm from "./BookForm.tsx"
import {toast} from "react-toastify"
import { getBooks, createBook, updateBook, deleteBook } from "../../services/BookService.ts"
import { getCategories } from "../../services/CategoySeyService.ts"
import { useAuth } from "../../contexts/AuthContext.tsx"
import axios from "axios";

const BookManagement = () => {
    const { isAdmin, isAuthenticated } = useAuth()
    const [books, setBooks] = useState<Book[]>([])
    const [categories, setCategories] = useState<Category[]>([])
    const [editingBook, setEditingBook] = useState<Book | null>(null)
    const [isModalOpen, setIsModalOpen] = useState<boolean>(false)
    const [loading, setLoading] = useState<boolean>(true)
    const [error, setError] = useState<string | null>(null)
    const [pageIndex, setPageIndex] = useState<number>(1)
    const [pageSize, setPageSize] = useState<number>(10)
    const [totalPages, setTotalPages] = useState<number>(1)
    const [pageSizeDropdownOpen, setPageSizeDropdownOpen] = useState<boolean>(false)
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState<boolean>(false)
    const [bookToDelete, setBookToDelete] = useState<Book | null>(null)
    const [deleteLoading, setDeleteLoading] = useState<boolean>(false)
    const [searchTerm, setSearchTerm] = useState<string>("")
    const [selectedCategory, setSelectedCategory] = useState<string | null>("")

    const pageSizeOptions = [5, 10, 20, 50]

    useEffect(() => {
        fetchBooks()
        fetchCategories()
    }, [pageIndex, pageSize, selectedCategory])

    const fetchBooks = async () => {
        try {
            setLoading(true)
            const response = await getBooks(
                pageIndex,
                pageSize,
                searchTerm || "",
                selectedCategory || ""
            )
            setBooks(response.items)
            setTotalPages(response.totalPages)
            setError(null)
        } catch (err) {
            setError("Failed to fetch books. Please try again later.")
            console.error("Error fetching books:", err)
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        } finally {
            setLoading(false)
        }
    }

    const fetchCategories = async () => {
        try {
            const response = await getCategories(1, 100) // Get all categories
            setCategories(response.items)
        } catch (err) {
            console.error("Error fetching categories:", err)
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        }
    }

    const handleSubmit = async (data: Book | Omit<Book, "id">): Promise<void> => {
        try {
            if ("id" in data) {
                // Updating an existing book
                await updateBook(data.id, {
                    title: data.title,
                    author: data.author,
                    categoryId: data.categoryId,
                    availableQuantity: data.availableQuantity,
                    quantity: data.quantity,
                    isbn: data.isbn,
                    publishDate: data.publishDate,
                    description: data.description || ""
                })
                toast.success(`Book "${data.title}" has been updated successfully.`)
            } else {
                // Adding a new book
                console.log("Formatted Data:", data.categoryId);
                await createBook({
                    title: data.title,
                    author: data.author,
                    categoryId: data.categoryId,
                    availableQuantity: data.availableQuantity,
                    quantity: data.quantity,
                    isbn: data.isbn,
                    publishDate: data.publishDate,
                    description: data.description || ""
                })
                toast.success(`Book "${data.title}" has been created successfully.`)
            }
            await fetchBooks() // Refresh the list after changes
            setEditingBook(null)
            setIsModalOpen(false)
        } catch (err) {
            console.error("Error saving book:", err)
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        }
    }

    const confirmDelete = (book: Book): void => {
        setBookToDelete(book)
        setDeleteConfirmOpen(true)
    }

    const handleDeleteBook = async (): Promise<void> => {
        if (!bookToDelete) return

        try {
            setDeleteLoading(true)
            const bookTitle = bookToDelete.title

            await deleteBook(bookToDelete.id)

            // Close the modal immediately
            setDeleteConfirmOpen(false)
            setBookToDelete(null)

            // Then show success message with the stored title
            toast.success(`Book "${bookTitle}" has been deleted successfully.`)

            // Finally refresh the data
            await fetchBooks()
        } catch (err) {
            console.error("Error deleting book:", err)
            if (bookToDelete) {
                if (axios.isAxiosError(err) && err.response) {
                    toast.error(err.response.data.error);
                } else {
                    toast.error("An unexpected error occurred");
                }
            } else {
                toast.error("Failed to delete book. Please try again.")
            }
        } finally {
            setDeleteLoading(false)
        }
    }

    const cancelDelete = (): void => {
        setDeleteConfirmOpen(false)
        setBookToDelete(null)
    }

    const openEditModal = (book: Book): void => {
        setEditingBook(book)
        setIsModalOpen(true)
    }

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage)
    }

    const handlePageSizeChange = (size: number) => {
        setPageSize(size)
        setPageIndex(1) // Reset to first page when changing page size
        setPageSizeDropdownOpen(false)
    }

    const getCategoryName = (categoryId: string): string => {
        const category = categories.find((c) => c.id === categoryId)
        return category ? category.name : "Unknown"
    }

    const borrowBook = (book: Book) => {
        // This will be handled in a separate BookDetails component
        toast.info(`This would borrow book "${book.title}" - to be implemented`)
    }

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault()
        setPageIndex(1) // Reset to first page
        fetchBooks() // This will use the current searchTerm
    }

    const handleCategoryChange = (categoryId: string | null) => {
        setSelectedCategory(categoryId)
        setPageIndex(1)
        setSearchTerm("")
        fetchBooks()
    }

    return (
        <div className="max-w-full mx-auto">
            {/* Main Content */}
            <div className="bg-white mb-6">
                <div className="mb-6">
                    <h2 className="text-2xl font-bold text-gray-900">Book Management</h2>
                    <p className="text-gray-600 mt-2">Add, update, and manage the library's book collection.</p>
                </div>

                {/* Filters and Search */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                    <div>
                        <label htmlFor="category" className="block text-sm font-medium text-gray-700 mb-1">
                            Filter by Category
                        </label>
                        <select
                            id="category"
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                            value={selectedCategory || ""}
                            onChange={(e) => handleCategoryChange(e.target.value || null)}
                        >
                            <option value="">All Categories</option>
                            {categories.map((category) => (
                                <option key={category.id} value={category.id}>
                                    {category.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="col-span-2">
                        <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
                            Search Books
                        </label>
                        <form onSubmit={handleSearch} className="flex">
                            <input
                                id="search"
                                type="text"
                                className="flex-1 px-3 py-2 border border-gray-300 rounded-l-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                                placeholder="Search by title or author"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                            />
                            <button
                                type="submit"
                                className="px-4 py-2 bg-blue-900 text-white rounded-r-lg hover:bg-blue-800"
                            >
                                <SearchIcon size={18} />
                            </button>
                        </form>
                    </div>
                </div>

                <div className="flex justify-between items-center mb-4">
                    <div className="flex items-center space-x-4">
                        <h3 className="text-xl font-bold text-gray-900">Books Catalog</h3>
                        <div className="relative">
                            <button
                                onClick={() => setPageSizeDropdownOpen(!pageSizeDropdownOpen)}
                                className="flex items-center space-x-1 text-sm text-gray-600 border border-gray-300 rounded-md px-2 py-1 hover:bg-gray-50 w-25 justify-center"
                            >
                                <span>Show: {pageSize}</span>
                                <ChevronDown size={16}/>
                            </button>
                            {pageSizeDropdownOpen && (
                                <div
                                    className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-md border border-gray-200 z-10 w-25">
                                    <ul className="py-1">
                                        {pageSizeOptions.map(size => (
                                            <li key={size}>
                                                <button
                                                    onClick={() => handlePageSizeChange(size)}
                                                    className={`w-full text-left px-4 py-2 text-sm ${pageSize === size ? 'bg-blue-50 text-blue-700' : 'hover:bg-gray-50'}`}
                                                >
                                                    {size} items
                                                </button>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            )}
                        </div>
                    </div>

                    {isAdmin() && (
                        <button
                            onClick={() => {
                                setEditingBook(null)
                                setIsModalOpen(true)
                            }}
                            className="flex items-center gap-2 py-2 px-4 bg-blue-900 text-white rounded-lg hover:bg-blue-800"
                        >
                            <PlusSquare size={18}/>
                            Add New Book
                        </button>
                    )}
                </div>

                {error && (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                        {error}
                    </div>
                )}

                {loading ? (
                    <div className="text-center py-8">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
                        <p className="mt-2 text-gray-600">Loading books...</p>
                    </div>
                ) : (
                    <>
                        <div className="border rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead>
                                <tr className="bg-gray-50">
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Title</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Author</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Category</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Quantity</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Available</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">ISBN</th>
                                    <th className="text-left py-3 px-3 font-medium text-gray-600">Year</th>
                                    <th className="text-center py-3 px-3 font-medium text-gray-600">Actions</th>
                                </tr>
                                </thead>
                                <tbody>
                                {books.length === 0 ? (
                                    <tr>
                                        <td colSpan={7} className="py-8 text-center text-gray-500">
                                            No books found
                                        </td>
                                    </tr>
                                ) : (
                                    books.map((book, index) => (
                                        <tr key={book.id}
                                            className={`border-t ${index % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                                            <td className="py-3 px-3 font-medium">{book.title}</td>
                                            <td className="py-3 px-3 text-gray-600">{book.author}</td>
                                            <td className="py-3 px-3 text-gray-600">{getCategoryName(book.categoryId)}</td>
                                            <td className="py-3 px-3 text-gray-600">{book.quantity}</td>
                                            <td className="py-3 px-3 text-gray-600">{book.availableQuantity}</td>
                                            <td className="py-3 px-3 text-gray-600">{book.isbn}</td>
                                            <td className="py-3 px-3 text-gray-600">{book.publishDate}</td>
                                            <td className="py-3 px-3">
                                                <div className="flex gap-2 justify-end">
                                                    {isAuthenticated && !isAdmin() && book.availableQuantity > 0 && (
                                                        <button
                                                            onClick={() => borrowBook(book)}
                                                            className="p-2 text-green-600 hover:bg-green-50 rounded"
                                                            aria-label="Borrow book"
                                                        >
                                                            <BookOpen size={18}/>
                                                        </button>
                                                    )}

                                                    {isAdmin() && (
                                                        <>
                                                            <button
                                                                onClick={() => openEditModal(book)}
                                                                className="p-2 text-blue-600 hover:bg-blue-50 rounded"
                                                                aria-label="Edit book"
                                                            >
                                                                <Pencil size={18}/>
                                                            </button>
                                                            <button
                                                                onClick={() => confirmDelete(book)}
                                                                className="p-2 text-red-500 hover:bg-red-50 rounded"
                                                                aria-label="Delete book"
                                                            >
                                                                <Trash2 size={18}/>
                                                            </button>
                                                        </>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    ))
                                )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination controls */}
                        {totalPages > 1 && (
                            <div className="flex justify-between items-center mt-4">
                                <div className="text-sm text-gray-600">
                                    Page {pageIndex} of {totalPages}
                                </div>
                                <div className="flex gap-2">
                                    <button
                                        onClick={() => handlePageChange(pageIndex - 1)}
                                        disabled={pageIndex <= 1}
                                        className={`px-3 py-1 rounded ${
                                            pageIndex <= 1
                                                ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                                                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                                        }`}
                                    >
                                        Previous
                                    </button>
                                    <button
                                        onClick={() => handlePageChange(pageIndex + 1)}
                                        disabled={pageIndex >= totalPages}
                                        className={`px-3 py-1 rounded ${
                                            pageIndex >= totalPages
                                                ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                                                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                                        }`}
                                    >
                                        Next
                                    </button>
                                </div>
                            </div>
                        )}
                    </>
                )}
            </div>

            {/* Add/Edit Modal */}
            {isModalOpen && isAdmin() && (
                <div
                    className="fixed bg-[rgba(0,0,0,0.5)] inset-0 drop-shadow-xl bg-opacity-50 flex items-center justify-center p-4 z-50">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl p-6">
                        <div className="mb-4">
                            <h3 className="text-xl font-bold text-gray-900">{editingBook ? "Edit Book" : "Add New Book"}</h3>
                        </div>

                        <BookForm
                            initialData={editingBook}
                            onSubmit={handleSubmit}
                            onCancel={() => {
                                setIsModalOpen(false)
                                setEditingBook(null)
                            }}
                            isEditing={!!editingBook}
                            categories={categories}
                        />
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {deleteConfirmOpen && bookToDelete && (
                <div className="fixed bg-[rgba(0,0,0,0.5)] inset-0 drop-shadow-xl bg-opacity-50 flex items-center justify-center p-4 z-50">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
                        <div className="mb-4">
                            <h3 className="text-xl font-bold text-gray-900">Confirm Deletion</h3>
                        </div>
                        <div className="mb-6">
                            <p className="text-gray-700">
                                Are you sure you want to delete the book <span className="font-medium">{bookToDelete.title}</span>?
                            </p>
                            <p className="text-gray-500 text-sm mt-2">
                                This action cannot be undone.
                            </p>
                        </div>
                        <div className="flex justify-end gap-2 pt-2">
                            <button
                                type="button"
                                onClick={cancelDelete}
                                disabled={deleteLoading}
                                className={`px-4 py-2 border border-gray-300 rounded-lg ${deleteLoading ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'text-gray-700 hover:bg-gray-50'}`}
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleDeleteBook}
                                disabled={deleteLoading}
                                className={`px-4 py-2 bg-red-600 text-white rounded-lg flex items-center gap-2 ${deleteLoading ? 'opacity-70 cursor-not-allowed' : 'hover:bg-red-700'}`}
                            >
                                {deleteLoading && (
                                    <div className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></div>
                                )}
                                {deleteLoading ? 'Deleting...' : 'Delete'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}

export default BookManagement
