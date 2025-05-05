import axiosInstance from "../utils/axiosInstance";
import {BorrowingRequest, BorrowingResponse} from "../types/Book";

interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
}

export enum Status {
    Approved = 1,
    Rejected = 2,
    Waiting = 3
}

// export const getAllBorrowingRequests = async (pageIndex: number = 1, pageSize: number = 10) => {
//     const result = await axiosInstance.get<PaginatedResponse<BorrowingResponse>>(`/book-borrowing?pageIndex=${pageIndex}&pageSize=${pageSize}`);
//     return result.data;
// };

export const getAllBorrowingRequests = async (pageIndex: number = 1, pageSize: number = 10, status?: Status) => {
    const url = `/book-borrowing?pageIndex=${pageIndex}&pageSize=${pageSize}${status !== undefined ? `&status=${status}` : ''}`;
    const result = await axiosInstance.get<PaginatedResponse<BorrowingResponse>>(url);
    return result.data;
};


// export const getMyBorrowingRequests = async (pageIndex: number = 1, pageSize: number = 10) => {
//     const result = await axiosInstance.get<PaginatedResponse<BorrowingResponse>>(`/book-borrowing/my-requests?pageIndex=${pageIndex}&pageSize=${pageSize}`);
//     return result.data;
// };

export const getMyBorrowingRequests = async (pageIndex: number = 1, pageSize: number = 10, status?: Status) => {
    const url = `/book-borrowing/my-requests?pageIndex=${pageIndex}&pageSize=${pageSize}${status !== undefined ? `&status=${status}` : ''}`;
    const result = await axiosInstance.get<PaginatedResponse<BorrowingResponse>>(url);
    return result.data;
};

export const createBorrowingRequest = async (bookIds: string[]) => {
    const result = await axiosInstance.post<BorrowingRequest>("/book-borrowing", { bookIds });
    return result.data;
};

export const approveBorrowingRequest = async (id: string) => {
    const result = await axiosInstance.put<BorrowingRequest>(`/book-borrowing/${id}/approve`);
    return result.data;
};

export const rejectBorrowingRequest = async (id: string) => {
    const result = await axiosInstance.put<BorrowingRequest>(`/book-borrowing/${id}/reject`);
    return result.data;
};

export const getMonthlyBorrowingCount = async () => {
    const result = await axiosInstance.get<{ count: number }>("/book-borrowing/monthly-count");
    console.log(result);
    return result.data;
};