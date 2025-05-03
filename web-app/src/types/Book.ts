export interface Book {
    id: string
    title: string
    author: string
    categoryId: string
    availableQuantity: number,
    quantity: number
    isbn: string
    publishDate: string
    description?: string
}

export interface BookFormData {
    title: string
    author: string
    categoryId: string
    availableQuantity: number
    quantity: number
    isbn: string
    publishDate: string
    description?: string
}

export interface BorrowingRequest {
    id: string
    userId: string
    bookId: string
    requestDate: string
    status: number
    processedDate?: string
    dueDate?: string
    processedBy?: string
}

export interface User {
    id: string
    name: string
    email: string
    phoneNumber: string
}
export interface BookBorrowing {
    id: string;
    bookId: string;
    bookTitle: string;
}
export interface BorrowingResponse {
    id: string
    requestDate: string
    status: number
    expirationDate: string
    requestor: User
    books: BookBorrowing[];
}