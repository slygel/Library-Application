import axiosInstance from "../utils/axiosInstance";
import { Book, BookFormData } from "../types/Book";

interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export const getBooks = async (
    pageIndex: number = 1,
    pageSize: number = 10,
    bookTitle?: string,
    categoryId?: string
) => {
    let url = `/books?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    if (bookTitle) {
        url += `&bookTitle=${encodeURIComponent(bookTitle)}`;
    }
    if (categoryId) {
        url += `&categoryId=${categoryId}`;
    }
    const result = await axiosInstance.get<PaginatedResponse<Book>>(url);
    return result.data;
};

export const getBookById = async (id: string) => {
    const result = await axiosInstance.get<Book>(`/books/${id}`);
    return result.data;
};

export const createBook = async (bookData: BookFormData) => {
    const result = await axiosInstance.post<Book>("/books", bookData);
    return result.data;
};

export const updateBook = async (id: string, bookData: BookFormData) => {
    const result = await axiosInstance.put<Book>(`/books/${id}`, bookData);
    return result.data;
};

export const deleteBook = async (id: string) => {
    const result = await axiosInstance.delete(`/books/${id}`);
    return result.data;
};

export const borrowBook = async (bookId: string) => {
    const result = await axiosInstance.post(`/books/${bookId}/borrow`);
    return result.data;
};