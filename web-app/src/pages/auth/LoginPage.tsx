import React from "react"
import { useState } from "react"
import {Eye, EyeOff, Lock,ArrowRight, User} from "lucide-react"
import {Link} from "react-router-dom";
import {useNavigate} from "react-router-dom";
import {toast} from "react-toastify";
import axios from "axios";
import {login} from "../../services/AuthService.ts";
import {useAuth} from "../../contexts/AuthContext.tsx";

const LoginPage = () => {
    const navigate = useNavigate();
    const { isLogin } = useAuth();
    const [isLoading, setIsLoading] = useState(false)
    const [showPassword, setShowPassword] = useState(false)
    const [formData, setFormData] = useState({
        username: "",
        password: "",
    })
    const [errors, setErrors] = useState({
        username: "",
        password: "",
    })

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value, type, checked } = e.target
        setFormData({
            ...formData,
            [name]: type === "checkbox" ? checked : value,
        })

        // Clear error when user starts typing
        if (errors[name as keyof typeof errors]) {
            setErrors({
                ...errors,
                [name]: "",
            })
        }
    }

    const validateForm = () => {
        let valid = true
        const newErrors = { ...errors }

        if (!formData.username) {
            newErrors.username = "Username is required"
            valid = false
        } else if (formData.username.length < 3) {
            newErrors.username = "Username must be at least 3 characters"
            valid = false
        } else {
            newErrors.username = ""
        }

        if (!formData.password) {
            newErrors.password = "Password is required"
            valid = false
        } else if (formData.password.length < 3) {
            newErrors.password = "Password must be at least 3 characters"
            valid = false
        } else {
            newErrors.password = ""
        }

        setErrors(newErrors)
        return valid
    }

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()

        if (!validateForm()) {
            return
        }

        setIsLoading(true)

        try {
            // API call
            const result = await login(formData.username, formData.password);
            const accessToken = result.accessToken;
            const refreshToken = result.refreshToken;
            
            // Use the authentication context's login function
            isLogin(accessToken, refreshToken);

            toast.success("Login successful!");

            // Redirect to homepage
            navigate("/");
        } catch (error) {
            if (axios.isAxiosError(error) && error.response) {
                toast.error(error.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        } finally {
            setIsLoading(false)
        }
    }

    const togglePasswordVisibility = () => {
        setShowPassword(!showPassword)
    }

    return (
        <div className="flex min-h-screen flex-col items-center justify-center px-4 py-8 dark:bg-gray-900">
            <div className="w-full max-w-md">

                <div className="rounded-lg border bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-800 h-128">
                    {/* Logo and Header */}
                    <div className="mb-8 text-center">
                        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
                            <Lock className="h-6 w-6 text-green-600 dark:text-green-300" />
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Sign in to your account</h1>
                        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">Enter your credentials to access your account</p>
                    </div>
                    {/* Login Form */}
                    <form onSubmit={handleSubmit}>
                        {/* Username Field */}
                        <div className="mb-7">
                            <label htmlFor="username" className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Username
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <User className="h-5 w-5 text-gray-400" />
                                </div>
                                <input
                                    type="text"
                                    id="username"
                                    name="username"
                                    value={formData.username}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.username ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="Enter your username"
                                    disabled={isLoading}
                                />
                            </div>
                            {errors.username && <p className="mt-1 text-xs text-red-500 absolute">{errors.username}</p>}
                        </div>

                        {/* Password Field */}
                        <div className="mb-4">
                            <label htmlFor="password" className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300">
                                Password
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <Lock className="h-5 w-5 text-gray-400" />
                                </div>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="password"
                                    name="password"
                                    value={formData.password}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.password ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="••••••••"
                                    disabled={isLoading}
                                />
                                <button
                                    type="button"
                                    onClick={togglePasswordVisibility}
                                    className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                                >
                                    {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                                </button>
                            </div>
                            {errors.password && <p className="mt-1 text-xs text-red-500 absolute">{errors.password}</p>}
                        </div>

                        {/* Submit Button */}
                        <button
                            type="submit"
                            disabled={isLoading}
                            className="flex w-full items-center justify-center rounded-md bg-emerald-600 px-4 py-2 text-lg font-medium text-white hover:bg-emerald-700 mt-5 h-11 mt-10"
                        >
                            {isLoading ? (
                                <>
                                    <svg
                                        className="mr-2 h-4 w-4 animate-spin"
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                    >
                                        <circle
                                            className="opacity-25"
                                            cx="12"
                                            cy="12"
                                            r="10"
                                            stroke="currentColor"
                                            strokeWidth="4"
                                        ></circle>
                                        <path
                                            className="opacity-75"
                                            fill="currentColor"
                                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                        ></path>
                                    </svg>
                                    Signing in...
                                </>
                            ) : (
                                <>
                                    Sign in
                                    <ArrowRight className="ml-2 h-4 w-4" />
                                </>
                            )}
                        </button>

                        {/* Sign Up Link */}
                        <div className="mt-6 text-center">
                            <p className="text-sm text-gray-600 dark:text-gray-400">
                                Don&apos;t have an account?{" "}
                                <Link
                                    to="/sign-up"
                                    className="font-medium text-green-600 hover:text-green-500"
                                >
                                    Sign Up
                                </Link>
                            </p>
                        </div>
                    </form>
                </div>


            </div>
        </div>
    )
}
export default LoginPage;
