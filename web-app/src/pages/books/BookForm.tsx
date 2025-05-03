import { useEffect } from "react"
import { useForm, type SubmitHandler } from "react-hook-form"
import type { Book, BookFormData } from "../../types/Book"
import {Category} from "../../types/Category.ts";

interface BookFormProps {
    onSubmit: (data: Book | Omit<Book, "id">) => void
    initialData: Book | null
    onCancel: () => void
    isEditing: boolean
    categories: Category[]
}

const BookForm = ({ onSubmit, initialData = null, onCancel, isEditing = false, categories }: BookFormProps) => {
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<BookFormData>({
        defaultValues: initialData || {
            title: "",
            author: "",
            categoryId: categories[0].id || "",
            availableQuantity: 1,
            quantity: 1,
            isbn: "",
            publishDate: new Date().toISOString().split('T')[0],
            description: ""
        },
    })

    useEffect(() => {
        if (initialData) {
            reset({
                ...initialData,
                categoryId: initialData.categoryId || "",
            })
        }
    }, [initialData, reset])

    const submitHandler: SubmitHandler<BookFormData> = (data) => {

        const formattedData = {
            ...data,
            categoryId: data.categoryId
        }

        if (isEditing && initialData) {
            onSubmit({ ...formattedData, id: initialData.id })
        } else {
            onSubmit(formattedData)
        }
    }

    return (
        <form onSubmit={handleSubmit(submitHandler)} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-1">
                        Title
                    </label>
                    <input
                        id="title"
                        type="text"
                        className={`w-full px-3 py-2 border ${
                            errors.title ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        placeholder="Enter book title"
                        {...register("title", {required: "Book title is required"})}
                    />
                    {errors.title && <p className="mt-1 text-sm text-red-500">{errors.title.message}</p>}
                </div>

                <div>
                    <label htmlFor="author" className="block text-sm font-medium text-gray-700 mb-1">
                        Author
                    </label>
                    <input
                        id="author"
                        type="text"
                        className={`w-full px-3 py-2 border ${
                            errors.author ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        placeholder="Enter author name"
                        {...register("author", {required: "Author name is required"})}
                    />
                    {errors.author && <p className="mt-1 text-sm text-red-500">{errors.author.message}</p>}
                </div>

                <div>
                    <label htmlFor="categoryId" className="block text-sm font-medium text-gray-700 mb-1">
                        Category
                    </label>
                    <select
                        id="categoryId"
                        className={`w-full px-3 py-2 border ${errors.categoryId ? "border-red-500" : "border-gray-300"} 
                                    rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        {...register("categoryId", {
                            required: "Category is required",
                            validate: (value) => value !== "" || "Please select a category",
                        })}
                        onChange={(e) => console.log("Select changed:", e.target.value)}
                    >
                        {categories.length === 0 ? (
                            <option value="0">No categories available</option>
                        ) : (
                            categories.map((category) => (
                                <option key={category.id} value={category.id}>
                                    {category.name}
                                </option>
                            ))
                        )}
                    </select>
                    {errors.categoryId && <p className="mt-1 text-sm text-red-500">{errors.categoryId.message}</p>}
                </div>

                <div>
                    <label htmlFor="quantity" className="block text-sm font-medium text-gray-700 mb-1">
                        Total Quantity
                    </label>
                    <input
                        id="quantity"
                        type="number"
                        min="0"
                        className={`w-full px-3 py-2 border ${
                            errors.quantity ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        {...register("quantity", {
                            required: "Total quantity is required",
                            valueAsNumber: true,
                            min: {value: 0, message: "Cannot be negative"},
                        })}
                    />
                    {errors.quantity && <p className="mt-1 text-sm text-red-500">{errors.quantity.message}</p>}
                </div>

                <div>
                    <label htmlFor="available" className="block text-sm font-medium text-gray-700 mb-1">
                        Available Copies
                    </label>
                    <input
                        id="available"
                        type="number"
                        min="0"
                        className={`w-full px-3 py-2 border ${
                            errors.availableQuantity ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        {...register("availableQuantity", {
                            required: "Available copies is required",
                            valueAsNumber: true,
                            min: {value: 0, message: "Cannot be negative"},
                        })}
                    />
                    {errors.availableQuantity &&
                        <p className="mt-1 text-sm text-red-500">{errors.availableQuantity.message}</p>}
                </div>

                <div>
                    <label htmlFor="isbn" className="block text-sm font-medium text-gray-700 mb-1">
                        ISBN
                    </label>
                    <input
                        id="isbn"
                        type="text"
                        className={`w-full px-3 py-2 border ${
                            errors.isbn ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        placeholder="Enter ISBN"
                        {...register("isbn", {required: "ISBN is required"})}
                    />
                    {errors.isbn && <p className="mt-1 text-sm text-red-500">{errors.isbn.message}</p>}
                </div>

                <div>
                    <label htmlFor="year" className="block text-sm font-medium text-gray-700 mb-1">
                        Publication Date
                    </label>
                    <input
                        id="year"
                        type="date"
                        className={`w-full px-3 py-2 border ${
                            errors.publishDate ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        {...register("publishDate", {
                            required: "Publication date is required"
                        })}
                    />
                    {errors.publishDate && <p className="mt-1 text-sm text-red-500">{errors.publishDate.message}</p>}
                </div>

                <div>
                    <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                        Description
                    </label>
                    <textarea
                        id="description"
                        rows={4}
                        className={`w-full px-3 py-2 border ${
                            errors.description ? "border-red-500" : "border-gray-300"
                        } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                        placeholder="Enter book description"
                        {...register("description")}
                    />
                    {errors.description && <p className="mt-1 text-sm text-red-500">{errors.description.message}</p>}
                </div>
            </div>

            <div className="flex justify-end gap-2 pt-4">
                <button
                    type="button"
                    onClick={onCancel}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                    Cancel
                </button>
                <button type="submit" className="px-4 py-2 bg-blue-900 text-white rounded-lg hover:bg-blue-800">
                    {isEditing ? "Update Book" : "Add Book"}
                </button>
            </div>
        </form>
    )
}

export default BookForm
