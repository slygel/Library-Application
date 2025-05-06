import {Link, useLocation} from "react-router-dom";
import { useEffect } from "react";
import {CircleAlert} from "lucide-react";

const NotFound = () => {
    const location = useLocation();

    useEffect(() => {
        console.error(
            "404 Error: User attempted to access non-existent route:",
            location.pathname
        );
    }, [location.pathname]);

    return (
        <div className="flex flex-col items-center justify-center h-screen bg-gray-50 p-4">
            <div className="w-full max-w-md bg-white rounded-lg shadow-md p-8 text-center">
                <div className="flex justify-center mb-6">
                    <CircleAlert className="h-16 w-16 text-red-500"/>
                </div>
                <h1 className="text-2xl font-bold text-gray-900 mb-4">Page not found</h1>
                <p className="text-gray-600 mb-6">
                    The page you are looking for does not exist or has been moved.
                </p>
                <div className="flex flex-col space-y-2">
                    <Link
                        to="/"
                        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                    >
                        Return to Home
                    </Link>
                    <button
                        onClick={() => window.history.back()}
                        className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
                    >
                        Go Back
                    </button>
                </div>
            </div>
        </div>
    );
};

export default NotFound;
