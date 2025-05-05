import { useState, useEffect } from "react"
import {CheckCircle, ChevronDown, Clock, LayoutGrid, Loader2, XCircle} from "lucide-react"
import {BorrowingResponse} from "../types/Book.ts"
import { useAuth } from "../contexts/AuthContext.tsx"
import { toast } from "react-toastify"
import { getAllBorrowingRequests, getMyBorrowingRequests, approveBorrowingRequest, rejectBorrowingRequest, Status } from "../services/BookBorrowingService.ts"

const BorrowingRequests = () => {
    const { isAdmin } = useAuth()
    const [activeStatus, setActiveStatus] = useState<Status | 'all'>(Status.Waiting)
    const [borrowingResponse, setBorrowingResponse] = useState<BorrowingResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [pageIndex, setPageIndex] = useState(1)
    const [pageSize,setPageSize] = useState(5)
    const [totalPages, setTotalPages] = useState(1)
    const [pageSizeDropdownOpen, setPageSizeDropdownOpen] = useState<boolean>(false)
    const [loadingRequestIds, setLoadingRequestIds] = useState<Record<string, {approving?: boolean, rejecting?: boolean}>>({})

    const pageSizeOptions = [5, 10, 20, 50];

    useEffect(() => {
        fetchBorrowingResponse()
    }, [pageIndex, pageSize, activeStatus])

    const fetchBorrowingResponse = async () => {
        try {
            setLoading(true)
            const status = activeStatus === 'all' ? undefined : activeStatus;
            const response = isAdmin()
                ? await getAllBorrowingRequests(pageIndex, pageSize, status)
                : await getMyBorrowingRequests(pageIndex, pageSize, status)

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
            setLoadingRequestIds(prev => ({
                ...prev,
                [requestId]: { ...prev[requestId], approving: true }
            }))
            await approveBorrowingRequest(requestId)
            toast.success("Request approved successfully")
            fetchBorrowingResponse()
        } catch (error) {
            console.error("Error approving request:", error)
            toast.error("Failed to approve request")
        } finally {
            setLoadingRequestIds(prev => ({
                ...prev,
                [requestId]: { ...prev[requestId], approving: false }
            }))
        }
    }

    const handleRejectRequest = async (requestId: string) => {
        try {
            setLoadingRequestIds(prev => ({
                ...prev,
                [requestId]: { ...prev[requestId], rejecting: true }
            }))
            await rejectBorrowingRequest(requestId)
            toast.success("Request rejected successfully")
            fetchBorrowingResponse()
        } catch (error) {
            console.error("Error rejecting request:", error)
            toast.error("Failed to reject request")
        } finally {
            setLoadingRequestIds(prev => ({
                ...prev,
                [requestId]: { ...prev[requestId], rejecting: false }
            }))
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
            case Status.Approved:
                return "Approved";
            case Status.Rejected:
                return "Rejected";
            case Status.Waiting:
                return "Waiting";
            default:
                return status;
        }
    }

    const getStatusClass = (status: number) => {
        switch (status) {
            case Status.Approved:
                return "bg-green-100 text-green-800";
            case Status.Rejected:
                return "bg-red-100 text-red-800";
            case Status.Waiting:
                return "bg-yellow-100 text-yellow-800";
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

    const getButtonClass = (status: Status | 'all') => {
        // return `cursor-pointer px-4 py-2 rounded-md ${
        //     activeStatus === status
        //         ? "bg-blue-100 text-blue-700"
        //         : "bg-gray-100 text-gray-700 hover:bg-gray-200"
        // }`
        if (activeStatus === status) {
            // Active state styling
            switch (status) {
                case 'all':
                    return "cursor-pointer px-4 py-2 rounded-md bg-blue-200 text-blue-800";
                case Status.Waiting:
                    return "cursor-pointer px-4 py-2 rounded-md bg-yellow-200 text-yellow-800";
                case Status.Approved:
                    return "cursor-pointer px-4 py-2 rounded-md bg-green-200 text-green-800";
                case Status.Rejected:
                    return "cursor-pointer px-4 py-2 rounded-md bg-red-200 text-red-800";
                default:
                    return "cursor-pointer px-4 py-2 rounded-md bg-blue-100 text-blue-700";
            }
        } else {
            // Inactive state styling
            switch (status) {
                case 'all':
                    return "cursor-pointer px-4 py-2 rounded-md bg-blue-50 text-blue-600 hover:bg-blue-100";
                case Status.Waiting:
                    return "cursor-pointer px-4 py-2 rounded-md bg-yellow-50 text-yellow-600 hover:bg-yellow-100";
                case Status.Approved:
                    return "cursor-pointer px-4 py-2 rounded-md bg-green-50 text-green-600 hover:bg-green-100";
                case Status.Rejected:
                    return "cursor-pointer px-4 py-2 rounded-md bg-red-50 text-red-600 hover:bg-red-100";
                default:
                    return "cursor-pointer px-4 py-2 rounded-md bg-gray-100 text-gray-700 hover:bg-gray-200";
            }
        }
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
                        onClick={() => setActiveStatus(Status.Waiting)}
                        className={getButtonClass(Status.Waiting)}
                    >
                        <span className="font-medium flex items-center gap-1">
                            <Clock size={16}/>
                            Waiting
                        </span>
                    </button>
                    <button
                        onClick={() => setActiveStatus(Status.Approved)}
                        className={getButtonClass(Status.Approved)}
                    >
                        <span className="font-medium flex items-center gap-1">
                            <CheckCircle size={16} />
                            Approved
                        </span>
                    </button>
                    <button
                        onClick={() => setActiveStatus(Status.Rejected)}
                        className={getButtonClass(Status.Rejected)}
                    >
                        <span className="font-medium flex items-center gap-1">
                            <XCircle size={16}/>
                            Rejected
                        </span>
                    </button>
                    <button
                        onClick={() => setActiveStatus('all')}
                        className={getButtonClass('all')}
                    >
                        <span className="font-medium flex items-center gap-1">
                            <LayoutGrid size={16} />
                            Show All
                        </span>
                    </button>
                </div>
            </div>

            {loading ? (
                <div className="text-center py-8">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
                    <p className="mt-2 text-gray-600">Loading requests...</p>
                </div>
            ) : (
                <div>
                    {borrowingResponse.length === 0 ? (
                        <div className="text-center py-12 text-gray-500">No borrowing requests to display.</div>
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
                                    {isAdmin() && activeStatus === Status.Waiting && (
                                        <th className="text-left py-3 px-4 font-medium text-gray-600">Actions</th>
                                    )}
                                </tr>
                                </thead>
                                <tbody>
                                {borrowingResponse.map((request, index) => (
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
                                            <span className={`px-3 py-1 rounded-full text-xs font-medium uppercase ${getStatusClass(request.status)}`}>
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
                                        {isAdmin() && activeStatus === Status.Waiting && (
                                            <td className="py-3 px-4">
                                                <div className="flex space-x-2">
                                                    <button
                                                        onClick={() => handleApproveRequest(request.id)}
                                                        disabled={loadingRequestIds[request.id]?.approving || loadingRequestIds[request.id]?.rejecting}
                                                        className={`cursor-pointer flex items-center gap-1 px-3 py-1 rounded-md ${
                                                            loadingRequestIds[request.id]?.approving || loadingRequestIds[request.id]?.rejecting
                                                                ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                                                                : "bg-green-100 text-green-700 hover:bg-green-200"
                                                        }`}
                                                    >
                                                        {loadingRequestIds[request.id]?.approving ? (
                                                            <Loader2 size={16} className="animate-spin" />
                                                        ) : (
                                                            <CheckCircle size={16} />
                                                        )}
                                                        {loadingRequestIds[request.id]?.approving ? "Approving..." : "Approve"}
                                                    </button>
                                                    <button
                                                        onClick={() => handleRejectRequest(request.id)}
                                                        disabled={loadingRequestIds[request.id]?.approving || loadingRequestIds[request.id]?.rejecting}
                                                        className={`cursor-pointer flex items-center gap-1 px-3 py-1 rounded-md ${
                                                            loadingRequestIds[request.id]?.approving || loadingRequestIds[request.id]?.rejecting
                                                                ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                                                                : "bg-red-100 text-red-700 hover:bg-red-200"
                                                        }`}
                                                    >
                                                        {loadingRequestIds[request.id]?.rejecting ? (
                                                            <Loader2 size={16} className="animate-spin" />
                                                        ) : (
                                                            <XCircle size={16} />
                                                        )}
                                                        {loadingRequestIds[request.id]?.rejecting ? "Rejecting..." : "Reject"}
                                                    </button>
                                                </div>
                                            </td>
                                        )}
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