import axiosInstance from "../utils/axiosInstance";
import {RegisterType} from "../types/RegisterType.ts";

export const login = async (username: string, password: string) => {
    const result = await axiosInstance.post("/auth/login", { username, password });
    return result.data;
};

export const register = async (data: RegisterType) => {
    const result = await axiosInstance.post("/auth/register", data);
    return result.data;
}

export const refreshToken = async (refreshToken: string) => {
    const result = await axiosInstance.post("/auth/refresh-token", { refreshToken });
    return result.data;
};

export const logout = async () => {
    try {
        const refreshToken = localStorage.getItem("refreshToken");
        if (refreshToken) {
            await axiosInstance.post("/auth/logout", { refreshToken });
        }
    } catch (error) {
        console.error("Logout error:", error);
    }
};