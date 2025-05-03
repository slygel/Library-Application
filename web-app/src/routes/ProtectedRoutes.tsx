import {Navigate, useLocation} from "react-router-dom";
import {useAuth} from "../contexts/AuthContext.tsx";

interface ProtectedRoutesProps {
    children: React.ReactNode;
    permission: string[];
    redirectPath?: string;
}

const ProtectedRoutes: React.FC<ProtectedRoutesProps> = ({
    children,
    permission,
    redirectPath = "/login"
}) => {
    const { isAuthenticated, role, loading } = useAuth();
    const location = useLocation();

    // Show loading state or spinner if authentication is still being checked
    if (loading) {
        return <div className="flex items-center justify-center h-screen">Loading...</div>;
    }

    // If not authenticated, redirect to log in
    if (!isAuthenticated) {
        return <Navigate to={redirectPath} state={{ from: location }} replace />;
    }

    // If user doesn't have required permission, redirect to log in or another page
    if (!role || !permission.includes(role)) {
        return <Navigate to="/unauthorized" state={{ from: location }} replace />;
    }

    // If authenticated and has permission, render the protected content
    return <>{children}</>;
}

export default ProtectedRoutes;