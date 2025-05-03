import React, { useState, useEffect } from "react"
import { BookOpen, ChevronDown, SearchIcon } from "lucide-react"
import { Category } from "../../types/Category.ts"
import { Book } from "../../types/Book.ts"
import {toast} from "react-toastify"
import { getBooks} from "../../services/BookService.ts"
import { getCategories } from "../../services/CategoySeyService.ts"
import { useAuth } from "../../contexts/AuthContext.tsx"
import { createBorrowingRequest,getMonthlyBorrowingCount } from "../../services/BookBorrowingService.ts"
import axios from "axios";

const BookList = () => {
    const { isAuthenticated, isAdmin } = useAuth()
    const [books, setBooks] = useState<Book[]>([])
    const [categories, setCategories] = useState<Category[]>([])
    const [loading, setLoading] = useState<boolean>(true)
    const [pageIndex, setPageIndex] = useState<number>(1)
    const [pageSize, setPageSize] = useState<number>(12)
    const [totalPages, setTotalPages] = useState<number>(1)
    const [pageSizeDropdownOpen, setPageSizeDropdownOpen] = useState<boolean>(false)
    const [selectedCategory, setSelectedCategory] = useState<string | null>("")
    const [searchTerm, setSearchTerm] = useState<string>("")
    const [borrowLoading, setBorrowLoading] = useState<boolean>(false)
    const [selectedBooks, setSelectedBooks] = useState<Book[]>([])
    const [showBorrowModal, setShowBorrowModal] = useState(false)
    const [monthlyBorrowingCount, setMonthlyBorrowingCount] = useState<number>(0)

    const pageSizeOptions = [5, 10, 20, 50]

    useEffect(() => {
        fetchBooks()
        fetchCategories()
        if(isAuthenticated && !isAdmin()) {
            fetchMonthlyBorrowingCount()
        }
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
        } catch (err) {
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            }else{
                toast.error("An unexpected error occurred");
            }
        } finally {
            setLoading(false)
        }
    }

    const fetchCategories = async () => {
        try {
            const response = await getCategories(1, 10)
            setCategories(response.items)
        } catch (err) {
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            }else{
                toast.error("An unexpected error occurred");
            }
        }
    }

    const fetchMonthlyBorrowingCount = async () => {
        try {
            const count = await getMonthlyBorrowingCount()
            setMonthlyBorrowingCount(Number(count))
        } catch (err) {
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            }else{
                toast.error("An unexpected error occurred");
            }
        }
    }

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage)
    }

    const handlePageSizeChange = (size: number) => {
        setPageSize(size)
        setPageIndex(1) // Reset to first page when changing page size
        setPageSizeDropdownOpen(false)
    }

    const handleCategoryChange = (categoryId: string | null) => {
        setSelectedCategory(categoryId)
        setPageIndex(1)
        setSearchTerm("")
        fetchBooks()
    }

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault()
        setPageIndex(1)
        fetchBooks()
    }

    const handleBookSelect = (book: Book) => {
        if (selectedBooks.find(b => b.id === book.id)) {
            setSelectedBooks(selectedBooks.filter(b => b.id !== book.id))
        } else {
            if (selectedBooks.length >= 5) {
                toast.error("You can only borrow up to 5 books at a time")
                return
            }
            setSelectedBooks([...selectedBooks, book])
        }
    }

    const handleBorrowSubmit = async () => {
        if (selectedBooks.length === 0) {
            toast.error("Please select at least one book to borrow")
            return
        }

        if (monthlyBorrowingCount >= 3) {
            toast.error("You have reached the limit of 3 borrowing requests per month")
            return
        }

        try {
            setBorrowLoading(true)
            await createBorrowingRequest(selectedBooks.map(book => book.id))
            toast.success("Borrowing request submitted successfully")
            setShowBorrowModal(false)
            setSelectedBooks([])
            await fetchBooks()
            await fetchMonthlyBorrowingCount()
        } catch (error) {
            if (axios.isAxiosError(error) && error.response) {
                toast.error(error.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        } finally {
            setBorrowLoading(false)
        }
    }

    return (
        <div className="max-w-full mx-auto">
            <div className="bg-white mb-6">
                <div className="mb-6">
                    <h2 className="text-2xl font-bold text-gray-900">Book Catalog</h2>
                    <p className="text-gray-600 mt-2">Browse our collection and borrow books.</p>
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
                            onChange={(e) => {
                                handleCategoryChange(e.target.value || null)
                            }}
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
                        <h3 className="text-xl font-bold text-gray-900">Available Books</h3>
                        <div className="relative">
                            <button
                                onClick={() => setPageSizeDropdownOpen(!pageSizeDropdownOpen)}
                                className="flex items-center space-x-1 text-sm text-gray-600 border border-gray-300 rounded-md px-2 py-1 hover:bg-gray-50 w-25 justify-center"
                            >
                                <span>Show: {pageSize}</span>
                                <ChevronDown size={16} />
                            </button>
                            {pageSizeDropdownOpen && (
                                <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-md border border-gray-200 z-10 w-25">
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

                    {searchTerm && (
                        <button
                            onClick={() => {
                                setSearchTerm("")
                                setPageIndex(1)
                                fetchBooks()
                            }}
                            className="text-sm text-blue-600 hover:text-blue-800"
                        >
                            Clear Search
                        </button>
                    )}
                </div>

                {/* Monthly Borrowing Limit Info */}
                {isAuthenticated && !isAdmin() && (
                    <div className="mb-4 p-4 bg-blue-50 rounded-lg">
                        <p className="text-sm text-blue-700">
                            Monthly borrowing requests: {monthlyBorrowingCount}/3
                        </p>
                        <p className="text-xs text-blue-600 mt-1">
                            You can borrow up to 5 books in one request, with a maximum of 3 requests per month.
                        </p>
                    </div>
                )}

                {loading ? (
                    <div className="text-center py-8">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
                        <p className="mt-2 text-gray-600">Loading books...</p>
                    </div>
                ) : (
                    <>
                    {books.length === 0 ? (
                        <div className="text-center py-8">
                            <p className="text-gray-600">
                                {selectedCategory && categories.find(c => c.id === selectedCategory)?.name
                                    ? `No books found`
                                    : "No books found matching your search criteria."}
                            </p>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                            {books.map((book) => (
                                <div key={book.id} className="border rounded-lg overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow">
                                    <div className="p-4">
                                        <h3 className="text-lg font-semibold text-gray-900 mb-2">{book.title}</h3>
                                        <p className="text-gray-600 mb-2">By {book.author}</p>
                                        <div className="flex justify-between items-center">
                                            <span className={`text-sm ${book.availableQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
                                                {book.availableQuantity > 0 ? `${book.availableQuantity}/${book.quantity || 0} available` : 'Not available'}
                                            </span>
                                            {isAuthenticated && !isAdmin() && book.availableQuantity > 0 && (
                                                <button
                                                    onClick={() => handleBookSelect(book)}
                                                    disabled={!selectedBooks.find(b => b.id === book.id) && selectedBooks.length >= 5 || monthlyBorrowingCount >= 3}
                                                    className={`cursor-pointer flex items-center gap-1 px-3 py-1 rounded-md text-sm
                                                        ${selectedBooks.find(b => b.id === book.id)
                                                        ? 'bg-blue-100 text-blue-700'
                                                        : selectedBooks.length >= 5 || monthlyBorrowingCount >= 3 
                                                            ? 'bg-gray-100 text-gray-400 cursor-not-allowed' 
                                                            : 'bg-green-100 text-green-700 hover:bg-green-200'
                                                    }`}
                                                >
                                                    <BookOpen size={16} />
                                                    {selectedBooks.find(b => b.id === book.id) ? 'Selected' : 'Select'}
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                        {/* Borrow Modal */}
                        {showBorrowModal && (
                            <div className="fixed inset-0 bg-[rgba(0,0,0,0.5)] bg-opacity-50 flex items-center justify-center p-4">
                                <div className="bg-white rounded-lg p-6 max-w-md w-full">
                                    <h3 className="text-lg font-semibold mb-4">Confirm Book Selection</h3>
                                    <div className="mb-4">
                                        <p className="text-gray-600 mb-2 font-semibold">Selected Books:</p>
                                        <ul className="space-y-2">
                                            {selectedBooks.map(book => (
                                                <li key={book.id} className="flex justify-between items-center">
                                                    <span>{book.title}</span>
                                                    <button
                                                        onClick={() => handleBookSelect(book)}
                                                        className="text-red-600 hover:text-red-800"
                                                    >
                                                        Remove
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                    <div className="flex justify-end space-x-2">
                                        <button
                                            onClick={() => setShowBorrowModal(false)}
                                            className="cursor-pointer px-4 py-2 text-gray-600 hover:text-gray-800"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            onClick={handleBorrowSubmit}
                                            disabled={borrowLoading || selectedBooks.length === 0}
                                            className="cursor-pointer px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
                                        >
                                            {borrowLoading ? 'Submitting...' : 'Submit Request'}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Submit Button */}
                        {isAuthenticated && !isAdmin() && selectedBooks.length > 0 && (
                            <div className="fixed bottom-6 right-6">
                                <button
                                    onClick={() => setShowBorrowModal(true)}
                                    className="cursor-pointer bg-blue-600 text-white px-6 py-3 rounded-full shadow-lg hover:bg-blue-700 flex items-center gap-2"
                                >
                                    <BookOpen size={20} />
                                    Submit Borrow Request ({selectedBooks.length})
                                </button>
                            </div>
                        )}

                        {/* Pagination controls */}
                        {totalPages > 1 && (
                            <div className="flex justify-between items-center mt-6">
                                <div className="text-sm text-gray-600">
                                    Page {pageIndex} of {totalPages}
                                </div>
                                <div className="flex gap-2">
                                    <button
                                        type="button"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            handlePageChange(pageIndex - 1);
                                        }}
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
                                        type="button"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            handlePageChange(pageIndex + 1);
                                        }}
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
        </div>
    )
}

export default BookList