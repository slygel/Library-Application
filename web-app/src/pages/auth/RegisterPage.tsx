import {ArrowRight, Eye, EyeOff, Lock, Mail, MapPinHouse, Phone, User, UserRoundCheck} from "lucide-react";
import {Link, useNavigate} from "react-router-dom";
import React, {useState} from "react";
import {register} from "../../services/AuthService.ts";
import axios from "axios";
import {toast} from "react-toastify";

const RegisterPage = () => {

    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false)
    const [showPassword, setShowPassword] = useState(false)
    const [formData, setFormData] = useState({
        name: "",
        username: "",
        email: "",
        phoneNumber: "",
        address: "",
        password: "",
        confirmPassword: "",
    });

    const [errors, setErrors] = useState({
        name: "",
        username: "",
        email: "",
        phoneNumber: "",
        address: "",
        password: "",
        confirmPassword: "",
    });

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
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        const phoneRegex = /^0\d{9}$/;

        if(!formData.name || formData.name.trim() === ''){
            newErrors.name = "Full name is required"
            valid = false
        }

        if (!formData.username || formData.username.trim() === '') {
            newErrors.username = "Username is required"
            valid = false
        } else if (formData.username.length < 3) {
            newErrors.username = "Username must be at least 3 characters"
            valid = false
        } else {
            newErrors.username = ""
        }

        if(!formData.email || formData.email.trim() === ''){
            newErrors.email = "Email is required"
            valid = false
        }else if(!emailRegex.test(formData.email)){
            newErrors.email = "Email invalid"
            valid = false
        }

        if(!formData.phoneNumber || formData.phoneNumber.trim() === ''){
            newErrors.phoneNumber = "Phone number is required"
            valid = false
        }else if(!phoneRegex.test(formData.phoneNumber)){
            newErrors.phoneNumber = "Phone number invalid"
            valid = false
        }

        if(!formData.address || formData.address.trim() === ''){
            newErrors.address = "Address is required"
            valid = false
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

        if(formData.confirmPassword !== formData.password){
            newErrors.confirmPassword = "Confirmation password does not match"
            valid = false
        }

        setErrors(newErrors)
        return valid;
    }

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()

        if (!validateForm()) {
            return
        }

        setIsLoading(true)

        try {
            // API call
            await register(formData);
            toast.success("Registration successful !");
            setTimeout(() => navigate("/login"), 1000);
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
        <div className="flex flex-col items-center justify-center px-4 py-6 dark:bg-gray-900">
            <div className="w-full max-w-lg">
                <div className="rounded-lg border bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-800 h-240">
                    {/* Logo and Header */}
                    <div className="mb-2 text-center">
                        <div
                            className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
                            <Lock className="h-6 w-6 text-green-600 dark:text-green-300"/>
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Sign up a account</h1>
                        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">Enter your registration information</p>
                    </div>
                    {/* Register Form */}
                    <form onSubmit={handleSubmit}>

                        {/* Full name Field */}
                        <div className="mb-5">
                            <label htmlFor="fullName"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Full name
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <UserRoundCheck className="h-5 w-5 text-gray-400"/>
                                </div>
                                <input
                                    type="text"
                                    id="name"
                                    name="name"
                                    value={formData.name}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.name ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="Enter your fullname"
                                    disabled={isLoading}
                                />
                            </div>
                            {errors.name && <p className="mt-1 text-xs text-red-500 absolute">{errors.name}</p>}
                        </div>

                        {/*Username*/}
                        <div className="mb-5">
                            <label htmlFor="username"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Username
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <User className="h-5 w-5 text-gray-400"/>
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

                        {/*Email*/}
                        <div className="mb-5">
                            <label htmlFor="email"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Email
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <Mail className="h-5 w-5 text-gray-400"/>
                                </div>
                                <input
                                    type="text"
                                    id="email"
                                    name="email"
                                    value={formData.email}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.email ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="Enter your email"
                                    disabled={isLoading}
                                />
                            </div>
                            {errors.email && <p className="mt-1 text-xs text-red-500 absolute">{errors.email}</p>}
                        </div>

                        {/*Phone number*/}
                        <div className="mb-5">
                            <label htmlFor="phoneNumber"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Phone number
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <Phone className="h-5 w-5 text-gray-400"/>
                                </div>
                                <input
                                    type="text"
                                    id="phoneNumber"
                                    name="phoneNumber"
                                    value={formData.phoneNumber}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.phoneNumber ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="Enter your phone number"
                                    disabled={isLoading}
                                />
                            </div>
                            {errors.phoneNumber && <p className="mt-1 text-xs text-red-500 absolute">{errors.phoneNumber}</p>}
                        </div>

                        {/*Address*/}
                        <div className="mb-5">
                            <label htmlFor="address"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300 h-6">
                                Address
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <MapPinHouse className="h-5 w-5 text-gray-400"/>
                                </div>
                                <input
                                    type="text"
                                    id="address"
                                    name="address"
                                    value={formData.address}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.address ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="Enter your username"
                                    disabled={isLoading}
                                />
                            </div>
                            {errors.address && <p className="mt-1 text-xs text-red-500 absolute">{errors.address}</p>}
                        </div>

                        {/* Password Field */}
                        <div className="mb-5">
                            <label htmlFor="password"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300">
                                Password
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <Lock className="h-5 w-5 text-gray-400"/>
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
                                    {showPassword ? <EyeOff className="h-5 w-5"/> : <Eye className="h-5 w-5"/>}
                                </button>
                            </div>
                            {errors.password && <p className="mt-1 text-xs text-red-500 absolute">{errors.password}</p>}
                        </div>

                        {/*Password confirm*/}
                        <div className="mb-5">
                            <label htmlFor="password"
                                   className="mb-2 block text-md font-medium text-gray-700 dark:text-gray-300">
                                Confirm password
                            </label>
                            <div className="relative">
                                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                                    <Lock className="h-5 w-5 text-gray-400"/>
                                </div>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="confirmPassword"
                                    name="confirmPassword"
                                    value={formData.confirmPassword}
                                    onChange={handleChange}
                                    className={`
                                        block w-full rounded-md border px-3 py-2 pl-10 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 h-12
                                        ${errors.confirmPassword ? "border-red-300 focus:border-red-500 focus:ring-red-500" : "border-gray-300 dark:border-gray-600"},
                                    `}
                                    placeholder="••••••••"
                                    disabled={isLoading}
                                />
                                <button
                                    type="button"
                                    onClick={togglePasswordVisibility}
                                    className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                                >
                                    {showPassword ? <EyeOff className="h-5 w-5"/> : <Eye className="h-5 w-5"/>}
                                </button>
                            </div>
                            {errors.confirmPassword && <p className="mt-1 text-xs text-red-500 absolute">{errors.confirmPassword}</p>}
                        </div>

                        {/* Submit Button */}
                        <button
                            type="submit"
                            disabled={isLoading}
                            className="flex w-full items-center justify-center rounded-md bg-emerald-600 px-4 py-2 text-lg font-medium text-white hover:bg-emerald-700 mt-7 h-11">
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
                                    Sign Up
                                    <ArrowRight className="ml-2 h-4 w-4"/>
                                </>
                            )}
                        </button>

                        {/* Sign Up Link */}
                        <div className="mt-4 text-center">
                            <p className="text-sm text-gray-600 dark:text-gray-400">
                                Do you have an account?{" "}
                                <Link
                                    to="/login"
                                    className="font-medium text-green-600 hover:text-green-500"
                                >
                                    Sign In
                                </Link>
                            </p>
                        </div>
                    </form>
                </div>


            </div>
        </div>
    );
};

export default RegisterPage;