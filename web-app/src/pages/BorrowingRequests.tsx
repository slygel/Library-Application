import { useState, useEffect } from "react"
import {CheckCircle, ChevronDown, XCircle} from "lucide-react"
import {BorrowingResponse} from "../types/Book.ts"
import { useAuth } from "../contexts/AuthContext.tsx"
import { toast } from "react-toastify"
import { getAllBorrowingRequests, getMyBorrowingRequests, approveBorrowingRequest, rejectBorrowingRequest } from "../services/BookBorrowingService.ts"

const BorrowingRequests = () => {
    const { isAdmin } = useAuth()
    const [activeTab, setActiveTab] = useState<"pending" | "processed">("pending")
    const [borrowingResponse, setBorrowingResponse] = useState<BorrowingResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [pageIndex, setPageIndex] = useState(1)
    const [pageSize,setPageSize] = useState(5)
    const [totalPages, setTotalPages] = useState(1)
    const [pageSizeDropdownOpen, setPageSizeDropdownOpen] = useState<boolean>(false)

    const pageSizeOptions = [5, 10, 20, 50];

    useEffect(() => {
        fetchBorrowingResponse()
    }, [pageIndex, pageSize])

    const fetchBorrowingResponse = async () => {
        try {
            setLoading(true)
            const response = isAdmin()
                ? await getAllBorrowingRequests(pageIndex, pageSize)
                : await getMyBorrowingRequests(pageIndex, pageSize)

            setBorrowingResponse(response.items)
            setTotalPages(response.totalPages)
        } catch (error) {
            console.error("Error fetching borrowing response:", error)
            toast.error("Failed to fetch borrowing response")
        } finally {
            setLoading(false)
        }
    }

    const handleApproveRequest = async (requestId: string) => {
        try {
            await approveBorrowingRequest(requestId)
            toast.success("Request approved successfully")
            fetchBorrowingResponse()
        } catch (error) {
            console.error("Error approving request:", error)
            toast.error("Failed to approve request")
        }
    }

    const handleRejectRequest = async (requestId: string) => {
        try {
            await rejectBorrowingRequest(requestId)
            toast.success("Request rejected successfully")
            fetchBorrowingResponse()
        } catch (error) {
            console.error("Error rejecting request:", error)
            toast.error("Failed to reject request")
        }
    }

    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    const getStatusDisplay = (status: number) => {
        switch (status) {
            case 1:
                return "Approved";
            case 2:
                return "Rejected";
            case 3:
                return "Waiting";
            case 4:
                return "Returned";
            default:
                return status;
        }
    }

    const getStatusClass = (status: number) => {
        switch (status) {
            case 1:
                return "bg-green-100 text-green-800";
            case 2:
                return "bg-red-100 text-red-800";
            case 3:
                return "bg-yellow-100 text-yellow-800";
            case 4:
                return "bg-blue-100 text-blue-800";
            default:
                return "bg-gray-100 text-gray-800";
        }
    }

    const handlePageSizeChange = (size: number) => {
        setPageSize(size);
        setPageIndex(1);
        setPageSizeDropdownOpen(false);
    };

    const handlePageChange = (newPage: number) => {
        setPageIndex(newPage)
    }

    const pendingRequests = borrowingResponse.filter((request) => request.status === 3)
    const processedRequests = borrowingResponse.filter((request) => request.status !== 3)

    if (loading) {
        return <div className="text-center py-12">Loading...</div>
    }

    return (
        <div className="max-w-full space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-gray-900">Borrowing Requests</h2>
                <div className="relative">
                    <button
                        onClick={() => setPageSizeDropdownOpen(!pageSizeDropdownOpen)}
                        className="flex items-center space-x-1 text-sm text-gray-600 border border-gray-300 rounded-md px-2 py-1 hover:bg-gray-50 w-25 justify-center"
                    >
                        <span className={"text-center"}>Show: {pageSize}</span>
                        <ChevronDown size={16}/>
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
                <div className="flex space-x-2">
                    <button
                        onClick={() => setActiveTab("pending")}
                        className={`cursor-pointer px-4 py-2 rounded-md ${
                            activeTab === "pending"
                                ? "bg-blue-100 text-blue-700"
                                : "bg-gray-100 text-gray-700"
                        }`}
                    >
                        Pending Requests
                    </button>
                    <button
                        onClick={() => setActiveTab("processed")}
                        className={`cursor-pointer px-4 py-2 rounded-md ${
                            activeTab === "processed"
                                ? "bg-blue-100 text-blue-700"
                                : "bg-gray-100 text-gray-700"
                        }`}
                    >
                        Processed Requests
                    </button>
                </div>
            </div>

            {activeTab === "pending" ? (
                <div>
                    {pendingRequests.length === 0 ? (
                        <div className="text-center py-12 text-gray-500">No pending borrowing requests to display.</div>
                    ) : (
                        <div className="space-y-4">
                            {pendingRequests.map((request) => (
                                <div key={request.id} className="border rounded-lg p-4 bg-white">
                                    <div className="flex justify-between items-start mb-4">
                                        <div>
                                            <p className="text-md text-gray-600">
                                                <span className="font-semibold mr-2">Request Date:</span>
                                                {formatDate(request.requestDate)}
                                            </p>
                                            <p className="text-md text-gray-600 ">
                                                <span className="font-semibold mr-2">Due Date:</span>
                                                {formatDate(request.expirationDate)}
                                            </p>
                                            <p className="text-md text-gray-600">
                                                <span className="font-semibold mr-2">Name:</span>
                                                {request.requestor.name}
                                            </p>
                                        </div>
                                        {isAdmin() && (
                                            <div className="flex space-x-2">
                                                <button
                                                    onClick={() => handleApproveRequest(request.id)}
                                                    className="flex items-center gap-1 px-3 py-1 bg-green-100 text-green-700 rounded-md hover:bg-green-200"
                                                >
                                                    <CheckCircle size={16} />
                                                    Approve
                                                </button>
                                                <button
                                                    onClick={() => handleRejectRequest(request.id)}
                                                    className="flex items-center gap-1 px-3 py-1 bg-red-100 text-red-700 rounded-md hover:bg-red-200"
                                                >
                                                    <XCircle size={16} />
                                                    Reject
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                    <div className="border-t pt-4">
                                        <table className="w-full border-collapse">
                                            <thead>
                                            <tr className="border-b">
                                                <th className="py-2 px-4 text-left font-medium text-gray-600">Borrow Id</th>
                                                <th className="py-2 px-4 text-left font-medium text-gray-600">Books</th>
                                                <th className="py-2 px-4 text-left font-medium text-gray-600">Status</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                            <tr>
                                                <td className="py-2 px-4">
                                                    {request.id.split("-")[0].toUpperCase()}
                                                </td>
                                                <td className="py-2 px-4">
                                                    {request.books.map((book) => (
                                                        <div key={book.id} className="text-sm text-gray-600">
                                                            {book.bookTitle}
                                                        </div>
                                                    ))}
                                                </td>
                                                <td className="py-2 px-4">
                                                    <span
                                                        className={`px-3 py-1 rounded-full text-xs font-medium uppercase ${getStatusClass(request.status)}`}>
                                                            {getStatusDisplay(request.status)}
                                                    </span>
                                                </td>
                                            </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            ) : (
                <div>
                    {processedRequests.length === 0 ? (
                        <div className="text-center py-12 text-gray-500">No processed borrowing requests to display.</div>
                    ) : (
                        <div className="border rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead>
                                <tr className="bg-gray-50">
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Borrow Id</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Books</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Status</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Requester</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Request Date</th>
                                    <th className="text-left py-3 px-4 font-medium text-gray-600">Due Date</th>
                                </tr>
                                </thead>
                                <tbody>
                                {processedRequests.map((request, index) => (
                                    <tr key={request.id}
                                        className={`border-t ${index % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                                        <td className="py-3 px-4">
                                            {request.id.split("-")[0].toUpperCase()}
                                        </td>
                                        <td className="py-2 px-4">
                                            {request.books.map((book) => (
                                                <div key={book.id} className="text-sm text-gray-600">
                                                    {book.bookTitle}
                                                </div>
                                            ))}
                                        </td>
                                        <td className="py-3 px-4">
                                            <span
                                                className={`px-3 py-1 rounded-full text-xs font-medium uppercase ${getStatusClass(request.status)}`}>
                                                {getStatusDisplay(request.status)}
                                            </span>
                                        </td>
                                        <td className="py-3 px-4">
                                            {request.requestor.name}
                                        </td>
                                        <td className="py-3 px-4">{formatDate(request.requestDate)}</td>
                                        <td className="py-3 px-4">
                                            {formatDate(request.expirationDate)}
                                        </td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            )}

            {/* Pagination */}
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
        </div>
    )
}

export default BorrowingRequests