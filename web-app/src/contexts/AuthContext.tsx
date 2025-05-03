import React, {createContext, useContext, useEffect, useState} from "react";
import {AuthContextType} from "../types/AuthContextType.ts";
import {jwtDecode} from "jwt-decode";
import {logout} from "../services/AuthService.ts";

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface JwtPayload {
    userId?: string;
    unique_name?: string;
    role: string;
}

// AuthProvider component
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [token, setToken] = useState<string | null>(null);
    const [refreshToken, setRefreshToken] = useState<string | null>(null);
    const [role, setRole] = useState<string | null>(null);
    const [username, setUsername] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    const isLogin = (newToken: string, newRefreshToken: string) => {
        localStorage.setItem("accessToken", newToken);
        localStorage.setItem("refreshToken", newRefreshToken);
        setToken(newToken);
        setRefreshToken(newRefreshToken);
        try {
            const decoded: JwtPayload = jwtDecode(newToken);
            setRole(decoded.role);
            if (decoded.unique_name) setUsername(decoded.unique_name);
        } catch (error) {
            console.error("Invalid token", error);
        }
    };

    const isLogout = async () => {
        try {
            await logout();
        } catch (error) {
            console.error("Logout error:", error);
        } finally {
            localStorage.removeItem("accessToken");
            localStorage.removeItem("refreshToken");
            setToken(null);
            setRefreshToken(null);
            setRole(null);
            setUsername(null);
        }
    };

    const isAdmin = () => {
        return role === "Admin";
    };

    const isUser = () => {
        return role === "User";
    };

    useEffect(() => {
        const storedToken = localStorage.getItem("accessToken");
        const storedRefreshToken = localStorage.getItem("refreshToken");
        if (storedToken && storedRefreshToken) {
            isLogin(storedToken, storedRefreshToken);
        }
        setLoading(false);
    }, []);

    const value: AuthContextType = {
        token,
        refreshToken,
        isAuthenticated: !!token,
        role,
        username,
        loading,
        isAdmin,
        isUser,
        isLogin,
        isLogout,
    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

// Custom hook to use AuthContext
export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};