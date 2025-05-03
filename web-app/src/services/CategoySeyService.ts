import axiosInstance from "../utils/axiosInstance";
import { Category } from "../types/Category";

interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export const getCategories = async (pageIndex: number = 1, pageSize: number = 10) => {
    const result = await axiosInstance.get<PaginatedResponse<Category>>(`/categories?pageIndex=${pageIndex}&pageSize=${pageSize}`);
    return result.data;
};

export const createCategory = async (categoryData: { name: string, description: string }) => {
    const result = await axiosInstance.post<Category>("/categories", categoryData);
    return result.data;
};

export const updateCategory = async (id: string, categoryData: { name: string, description: string }) => {
    const result = await axiosInstance.put<Category>(`/categories/${id}`, categoryData);
    return result.data;
};

export const deleteCategory = async (id: string) => {
    const result = await axiosInstance.delete(`/categories/${id}`);
    return result.data;
};