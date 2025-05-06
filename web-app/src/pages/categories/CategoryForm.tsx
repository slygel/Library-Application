import { useEffect } from "react"
import { useForm, type SubmitHandler } from "react-hook-form"
import { Category, CategoryFormData} from "../../types/Category.ts"

interface CategoryFormProps {
    onSubmit: (data: Category | Omit<Category, "id">) => void
    initialData: Category | null
    onCancel: () => void
    isEditing: boolean
}

const CategoryForm = ({onSubmit, initialData = null, onCancel, isEditing = false,}: CategoryFormProps) =>{
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<CategoryFormData>({
        defaultValues: initialData || { name: "", description: "" },
    })

    useEffect(() => {
        if (initialData) {
            reset(initialData)
        }
    }, [initialData, reset])

    const submitHandler: SubmitHandler<CategoryFormData> = (data) => {
        if (isEditing && initialData) {
            onSubmit({ ...data, id: initialData.id })
        } else {
            onSubmit(data)
        }
    }

    return (
        <form onSubmit={handleSubmit(submitHandler)} className="space-y-4">
            <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                    Category Name
                </label>
                <input
                    id="name"
                    type="text"
                    className={`w-full px-3 py-2 border ${
                        errors.name ? "border-red-500" : "border-gray-300"
                    } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                    placeholder="Enter category name"
                    {...register("name", { required: "Category name is required" })}
                />
                {errors.name && <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>}
            </div>

            <div>
                <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                    Description
                </label>
                <textarea
                    id="description"
                    rows={3}
                    className={`w-full px-3 py-2 border ${
                        errors.description ? "border-red-500" : "border-gray-300"
                    } rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none`}
                    placeholder="Enter category description"
                    {...register("description", { required: "Category description is required" })}
                />
            </div>

            <div className="flex justify-end gap-2 pt-2">
                <button
                    type="button"
                    onClick={onCancel}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                >
                    Cancel
                </button>
                <button type="submit" className="px-4 py-2 bg-green-800 text-white rounded-lg hover:bg-green-900">
                    {isEditing ? "Update Category" : "Add Category"}
                </button>
            </div>
        </form>
    )
}

export default CategoryForm