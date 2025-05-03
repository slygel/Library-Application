import {BrowserRouter, Route, Routes} from "react-router-dom";
import HomePage from "../pages/HomePage.tsx";
import LoginPage from "../pages/auth/LoginPage.tsx";
import RegisterPage from "../pages/auth/RegisterPage.tsx";
import CategoryPage from "../pages/CategoryPage.tsx";
import ProtectedRoutes from "./ProtectedRoutes.tsx";
import NotFound from "../pages/NotFound.tsx";
import {AuthProvider} from "../contexts/AuthContext.tsx";
import BookPage from "../pages/BookPage.tsx";
import BorrowingPage from "../pages/BorrowingPage.tsx";
import Unauthorized from "../pages/Unauthorized.tsx";

const AppRoutes = () => {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    {/* Public routes */}
                    <Route path="/login" element={<LoginPage/>} />
                    <Route path="/sign-up" element={<RegisterPage/>} />
                    <Route path="/unauthorized" element={<Unauthorized/>} />
                    
                    <Route path="/" element={
                        <HomePage/>
                    }/>
                    
                    <Route path="/books" element={
                        <ProtectedRoutes permission={["Admin", "User"]}>
                            <BookPage/>
                        </ProtectedRoutes>
                    }/>
                    
                    {/* Admin only routes */}
                    <Route path="/categories" element={
                        <ProtectedRoutes permission={["Admin"]}>
                            <CategoryPage/>
                        </ProtectedRoutes>
                    }/>
                    
                    <Route path="/requests" element={
                        <ProtectedRoutes permission={["Admin"]}>
                            <BorrowingPage/>
                        </ProtectedRoutes>
                    }/>

                    {/* User only routes */}
                    <Route path="/my-borrowings" element={
                        <ProtectedRoutes permission={["User"]}>
                            <BorrowingPage/>
                        </ProtectedRoutes>
                    }/>
                    
                    {/* Catch-all route */}
                    <Route path="*" element={<NotFound/>} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
};

export default AppRoutes;