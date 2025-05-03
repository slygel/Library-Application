import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import {
    Bell,
    BookOpen,
    BookPlus,
    Eye,
    FolderPlus,
    Home, LogIn,
    LogOut,
    Menu,
    Search,
    Settings,
    User,
    X,
} from "lucide-react"
import NavItem from "../NavItem.tsx"
import { useAuth } from "../../contexts/AuthContext.tsx"

const Header = () => {
    const { isAuthenticated, isAdmin, isUser, username, isLogout } = useAuth()
    const navigate = useNavigate()
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)
    const [isUserMenuOpen, setIsUserMenuOpen] = useState(false)

    const toggleMobileMenu = () => {
        setIsMobileMenuOpen(!isMobileMenuOpen)
    }

    const toggleUserMenu = () => {
        setIsUserMenuOpen(!isUserMenuOpen)
    }

    const handleLogout = () => {
        isLogout()
        navigate('/')
    }

    // Admin specific navigation items
    const adminNavItems = () => (
        <>
            <NavItem icon={<Home className="h-6 w-6" />} title="Dashboard" href="/" />
            <NavItem icon={<FolderPlus className="h-6 w-6" />} title="Manage Categories" href="/categories" />
            <NavItem icon={<BookPlus className="h-6 w-6" />} title="Manage Books" href="/books" />
            <NavItem icon={<Eye className="h-6 w-6" />} title="Review Requests" href="/requests" />
        </>
    )

    // User specific navigation items
    const userNavItems = () => (
        <>
            <NavItem icon={<Home className="h-6 w-6" />} title="Home" href="/" />
            <NavItem icon={<BookOpen className="h-6 w-6" />} title="Books" href="/books" />
            <NavItem icon={<Eye className="h-6 w-6" />} title="My Borrowings" href="/my-borrowings" />
        </>
    )

    // Render different navigation based on role
    const renderNavigation = () => {
        if (isAdmin()) {
            return adminNavItems()
        } else if (isUser()) {
            return userNavItems()
        }
        return null
    }

    // Guest header for non-authenticated users
    if (!isAuthenticated) {
        return (
            <header className="sticky top-0 z-40 border-b-1 border-b-gray-100 shadow-sm bg-white">
                <div className="container mx-auto flex h-16 items-center justify-between px-4">
                    <div className="flex items-center">
                        <Link to="/" className="flex items-center gap-2">
                            <BookOpen className="h-8 w-8 text-emerald-600"/>
                            <span className="text-xl font-bold">Library App</span>
                        </Link>
                    </div>

                    <div className="hidden md:flex items-center space-x-4">
                        <Link to="/sign-up"
                          className="flex items-center text-gray-600 hover:text-emerald-600 transition-colors">
                            <User className="h-5 w-5 mr-1"/>
                            <span>Sign Up</span>
                        </Link>
                        <Link
                            to="/login"
                            className="bg-emerald-600 flex items-center text-white px-4 py-2 rounded-md hover:bg-emerald-700 transition-colors"
                        >
                            <LogIn className="h-4 w-4 inline mr-2"/>
                            <span>Sign In</span>
                        </Link>
                    </div>
                </div>
            </header>
        )
    }

    return (
        <header className="sticky top-0 z-40 border-b-1 border-b-gray-100 shadow-sm bg-white">
            <div className="container mx-auto flex h-16 items-center justify-between px-4">
                {/* Logo */}
                <div className="flex items-center">
                    <Link to="/" className="flex items-center gap-2">
                        <BookOpen className="h-8 w-8 text-emerald-600"/>
                        <span className="text-xl font-bold">
                            {isAdmin() ? "Admin Dashboard" : "Library App"}
                        </span>
                    </Link>
                </div>

                {/* Desktop Navigation */}
                <nav className="hidden md:flex items-center space-x-1">
                    {renderNavigation()}
                </nav>

                {/* Right side actions */}
                <div className="flex items-center gap-2">
                    <button className="cursor-pointer rounded-full p-2 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800">
                        <Search className="h-5 w-5" />
                    </button>
                    <button className="cursor-pointer rounded-full p-2 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800">
                        <Bell className="h-5 w-5" />
                    </button>

                    {/* User menu */}
                    <div className="relative">
                        <button
                            onClick={toggleUserMenu}
                            className="cursor-pointer flex h-8 w-8 items-center justify-center rounded-full bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600"
                        >
                            <User className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                        </button>

                        {/* User dropdown menu */}
                        {isUserMenuOpen && (
                            <div className="absolute right-0 mt-2 w-48 rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 dark:bg-gray-800">
                                <div className="px-4 py-2 border-b dark:border-gray-700">
                                    <p className="text-sm font-medium">{username || "User"}</p>
                                    <p className="text-xs text-gray-500 dark:text-gray-400">
                                        {isAdmin() ? "Administrator" : "Library Member"}
                                    </p>
                                </div>
                                <Link
                                    to="/"
                                    className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
                                >
                                    <User className="mr-2 h-4 w-4" />
                                    Profile
                                </Link>
                                <Link
                                    to="/"
                                    className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
                                >
                                    <Settings className="mr-2 h-4 w-4" />
                                    Settings
                                </Link>
                                <button
                                    onClick={handleLogout}
                                    className="flex w-full items-center px-4 py-2 text-sm text-red-600 hover:bg-gray-100 dark:text-red-400 dark:hover:bg-gray-700"
                                >
                                    <LogOut className="mr-2 h-4 w-4" />
                                    Logout
                                </button>
                            </div>
                        )}
                    </div>

                    {/* Mobile menu button */}
                    <button
                        onClick={toggleMobileMenu}
                        className="ml-2 rounded-md p-2 text-gray-600 hover:bg-gray-100 md:hidden dark:text-gray-400 dark:hover:bg-gray-800"
                    >
                        {isMobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
                    </button>
                </div>
            </div>

            {/* Mobile Navigation */}
            {isMobileMenuOpen && (
                <nav className="border-t p-4 md:hidden">
                    <div className="flex flex-col space-y-2">
                        {renderNavigation()}
                    </div>
                </nav>
            )}
        </header>
    )
}

export default Header
