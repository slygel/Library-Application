import { useState, useEffect } from "react"
import {FolderPlus, Pencil, Trash2, ChevronDown } from "lucide-react"
import {Category} from "../../types/Category.ts";
import CategoryForm from "./CategoryForm.tsx";
import { getCategories, createCategory, updateCategory, deleteCategory } from "../../services/CategoySeyService.ts";
import {toast} from "react-toastify";
import axios from "axios";

const CategoryManagement = () => {
    const [categories, setCategories] = useState<Category[]>([])
    const [editingCategory, setEditingCategory] = useState<Category | null>(null)
    const [isModalOpen, setIsModalOpen] = useState<boolean>(false)
    const [loading, setLoading] = useState<boolean>(true)
    const [pageIndex, setPageIndex] = useState<number>(1)
    const [pageSize, setPageSize] = useState<number>(5)
    const [totalPages, setTotalPages] = useState<number>(1)
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState<boolean>(false)
    const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null)
    const [pageSizeDropdownOpen, setPageSizeDropdownOpen] = useState<boolean>(false)
    const [deleteLoading, setDeleteLoading] = useState<boolean>(false)

    const pageSizeOptions = [5, 10, 20, 50];

    useEffect(() => {
        fetchCategories();
    }, [pageIndex, pageSize]);

    const fetchCategories = async () => {
        try {
            setLoading(true);
            const response = await getCategories(pageIndex, pageSize);
            setCategories(response.items);
            setTotalPages(response.totalPages);
        } catch (err) {
            console.error("Error fetching categories:", err);
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        } finally {
            setLoading(false);
        }
    };


    const handleSubmit = async (data: Category | Omit<Category, "id">): Promise<void> => {
        try {
            if ("id" in data) {
                // Updating an existing category
                await updateCategory(data.id, {
                    name: data.name,
                    description: data.description
                });
                toast.success(`Category "${data.name}" has been updated successfully.`);
            } else {
                // Adding a new category
                await createCategory({
                    name: data.name,
                    description: data.description
                });
                toast.success(`Category "${data.name}" has been created successfully.`);
            }
            await fetchCategories(); // Refresh the list after changes
            setEditingCategory(null);
            setIsModalOpen(false);
        } catch (err) {
            console.error("Error saving category:", err);
            if (axios.isAxiosError(err) && err.response) {
                toast.error(err.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        }
    };

    const confirmDelete = (category: Category): void => {
        setCategoryToDelete(category);
        setDeleteConfirmOpen(true);
    };

    const handleDeleteCategory = async (): Promise<void> => {
        if (!categoryToDelete) return;

        try {
            setDeleteLoading(true);
            const categoryName = categoryToDelete.name;

            await deleteCategory(categoryToDelete.id);

            setDeleteConfirmOpen(false);
            setCategoryToDelete(null);

            // Show success message
            toast.success(`Category "${categoryName}" has been deleted successfully.`);

            // Refresh the data
            await fetchCategories();
        } catch (err) {
            console.error("Error deleting category:", err);
            if (categoryToDelete) {
                if (axios.isAxiosError(err) && err.response) {
                    toast.error(err.response.data.error);
                } else {
                    toast.error("An unexpected error occurred");
                }
            } else {
                toast.error("Failed to delete category. Please try again.");
            }
        } finally {
            setDeleteLoading(false);
        }
    };

    const cancelDelete = (): void => {
        setDeleteConfirmOpen(false);
        setCategoryToDelete(null);
    };

    const openEditModal = (category: Category): void => {
        setEditingCategory(category)
        setIsModalOpen(true)
    }

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage);
    };

    const handlePageSizeChange = (size: number) => {
        setPageSize(size);
        setPageIndex(1);
        setPageSizeDropdownOpen(false);
    };

    return (
        <div className="max-w-full mx-auto">
            {/* Main Content */}
            <div className="bg-white mb-6">
                <div className="mb-6">
                    <h2 className="text-2xl font-bold text-gray-900">Category Management</h2>
                    <p className="text-gray-600 mt-2">Manage book categories and classifications.</p>
                </div>

                <div className="flex justify-between items-center mb-4">
                    <div className="flex items-center space-x-4">
                        <h3 className="text-xl font-bold text-gray-900">Book Categories</h3>

                    </div>
                    <button
                        onClick={() => {
                            setEditingCategory(null)
                            setIsModalOpen(true)
                        }}
                        className="flex items-center gap-2 py-2 px-4 bg-green-800 text-white rounded-lg hover:bg-green-900"
                    >
                        <FolderPlus size={18}/>
                        Add Category
                    </button>
                </div>

                {loading ? (
                    <div className="text-center py-8">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
                        <p className="mt-2 text-gray-600">Loading categories...</p>
                    </div>
                ) : (
                    <>
                        <div className="border rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead>
                                <tr className="bg-gray-50">
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Name</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Description</th>
                                    <th className="text-right py-3 px-4 font-medium text-gray-600">Actions</th>
                                </tr>
                                </thead>
                                <tbody>
                                {categories.length === 0 ? (
                                    <tr>
                                        <td colSpan={3} className="py-8 text-center text-gray-500">
                                            No categories found
                                        </td>
                                    </tr>
                                ) : (
                                    categories.map((category, index) => (
                                        <tr key={category.id}
                                            className={`border-t ${index % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                                            <td className="py-3 px-4 font-medium">{category.name}</td>
                                            <td className="py-3 px-4 text-gray-600">{category.description}</td>
                                            <td className="py-3 px-4">
                                                <div className="flex gap-2 justify-end">
                                                    <button
                                                        onClick={() => openEditModal(category)}
                                                        className="p-2 text-blue-600 hover:bg-blue-100 rounded"
                                                        aria-label="Edit category"
                                                    >
                                                        <Pencil size={18}/>
                                                    </button>
                                                    <button
                                                        onClick={() => confirmDelete(category)}
                                                        className="p-2 text-red-500 hover:bg-red-100 rounded"
                                                        aria-label="Delete category"
                                                    >
                                                        <Trash2 size={18}/>
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))
                                )}
                                </tbody>
                            </table>
                        </div>


                        <div className="relative mt-4 flex items-center justify-between">
                            <button
                                onClick={() => setPageSizeDropdownOpen(!pageSizeDropdownOpen)}
                                className="flex items-center space-x-1 text-sm text-gray-600 border border-gray-300 rounded-md px-2 py-1 hover:bg-gray-50 w-25 justify-center"
                            >
                                <span className={"text-center"}>Show: {pageSize}</span>
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

                        {/* Pagination */}
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

            {/* Modal */}
            {isModalOpen && (
                <div
                    className="fixed bg-[rgba(0,0,0,0.5)] inset-0 drop-shadow-xl bg-opacity-50 flex items-center justify-center p-4 z-50">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
                        <div className="mb-4">
                            <h3 className="text-xl font-bold text-gray-900">
                                {editingCategory ? "Edit Category" : "Add New Category"}
                            </h3>
                        </div>

                        <CategoryForm
                            initialData={editingCategory}
                            onSubmit={handleSubmit}
                            onCancel={() => {
                                setIsModalOpen(false)
                                setEditingCategory(null)
                            }}
                            isEditing={!!editingCategory}
                        />
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {deleteConfirmOpen && categoryToDelete && (
                <div
                    className="fixed bg-[rgba(0,0,0,0.5)] inset-0 drop-shadow-xl bg-opacity-50 flex items-center justify-center p-4 z-50">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
                        <div className="mb-4">
                            <h3 className="text-xl font-bold text-gray-900">Confirm Deletion</h3>
                        </div>
                        <div className="mb-6">
                            <p className="text-gray-700">
                            Are you sure you want to delete the category <span className="font-medium">{categoryToDelete.name}</span>?
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
                                className={`px-4 py-2 border border-gray-300 rounded-lg text-gray-700 ${deleteLoading ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-50'}`}
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleDeleteCategory}
                                disabled={deleteLoading}
                                className={`px-4 py-2 bg-red-600 text-white rounded-lg ${deleteLoading ? 'opacity-50 cursor-not-allowed' : 'hover:bg-red-700'} flex items-center gap-2`}
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

export default CategoryManagement
