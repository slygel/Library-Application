
import {BookOpen} from "lucide-react"
import {Link} from "react-router-dom";

const Footer = () => {
    return (
        <footer className="bg-gray-800 text-gray-300 py-8">
            <div className="container mx-auto px-4">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
                    <div>
                        <h3 className="text-white text-lg font-bold mb-4">LibraryApp</h3>
                        <p className="mb-4">Your gateway to knowledge and discovery.</p>
                        <div className="flex items-center space-x-2">
                            <BookOpen className="h-6 w-6 text-emerald-400"/>
                            <span className="text-white font-bold">LibraryApp</span>
                        </div>
                    </div>

                    <div>
                        <h4 className="text-white text-md font-bold mb-4">Quick Links</h4>
                        <ul className="space-y-2">
                            <li>
                                <Link to="/" className="hover:text-emerald-400 transition-colors">
                                    Home
                                </Link>
                            </li>
                            <li>
                                <Link to="#" className="hover:text-emerald-400 transition-colors">
                                    My Books
                                </Link>
                            </li>
                        </ul>
                    </div>

                    <div>
                        <h4 className="text-white text-md font-bold mb-4">Services</h4>
                        <ul className="space-y-2">
                            <li>
                                <Link to="#" className="hover:text-emerald-400 transition-colors">
                                    Book Borrowing
                                </Link>
                            </li>
                            <li>
                                <Link to="#" className="hover:text-emerald-400 transition-colors">
                                    Research Help
                                </Link>
                            </li>
                            <li>
                                <Link to="#" className="hover:text-emerald-400 transition-colors">
                                    Study Rooms
                                </Link>
                            </li>
                        </ul>
                    </div>

                    <div>
                        <h4 className="text-white text-md font-bold mb-4">Contact</h4>
                        <address className="not-italic">
                            <p>Ba Vi, Ha Noi</p>
                            <p>Email: nttue03@gmail.com</p>
                            <p>Phone: 0383291503</p>
                        </address>
                    </div>
                </div>

                <div className="border-t border-gray-700 mt-8 pt-6 text-center text-sm">
                    <p>&copy; {new Date().getFullYear()} LibraryApp. All rights reserved.</p>
                </div>
            </div>
        </footer>
    );
};

export default Footer;