import Layout from "../components/Layout.tsx";
import {Book, Clock, User} from "lucide-react";
import axios from "axios";
import {toast} from "react-toastify";
import {useEffect, useState} from "react";
import {getStatistic, Statistic} from "../services/Statistic.ts";
const HomePage = () => {

    const [statistics, setStatistics] = useState<Statistic>({
        totalBooks: 0,
        totalCategories: 0,
        totalUsers: 0
    });

    useEffect(() => {
        fetchStatistics();
    }, []);

    const fetchStatistics = async () => {
        try {
            const data = await getStatistic();
            setStatistics(data);
        } catch (error) {
            if (axios.isAxiosError(error) && error.response) {
                toast.error(error.response.data.error);
            } else {
                toast.error("An unexpected error occurred");
            }
        }
    };

    return (
        <Layout>
            <div className="min-h-screen w-screen flex flex-col">
                <main className="flex-grow">
                    <section className="bg-gradient-to-r from-emerald-500 to-teal-600 text-white py-16">
                        <div className="container mx-auto px-4">
                            <div className="flex flex-col md:flex-row items-center justify-between">
                                <div className="md:w-1/2 mb-8 md:mb-0">
                                    <h1 className="text-4xl md:text-5xl font-bold mb-4">Discover Your Next Favorite
                                        Book</h1>
                                    <p className="text-lg mb-6">
                                        Access thousands of books, manage your borrowings, and discover new titles all
                                        in
                                        one place.
                                    </p>
                                </div>
                                <div className="md:w-1/2 flex justify-center transition-opacity duration-500">
                                    <img
                                        src={"/homepage.png"}
                                        alt={""}
                                        width={"400"}
                                        height={"300"}
                                    />
                                </div>
                            </div>
                        </div>
                    </section>

                    <section className="py-12 bg-gray-50">
                        <div className="container mx-auto px-4">
                            <h2 className="text-3xl font-bold mb-12 text-center text-gray-800">Library Services</h2>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                                <div className="bg-white p-6 rounded-lg shadow-md text-center cursor-pointer">
                                    <div
                                        className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 text-emerald-600 rounded-full mb-4">
                                        <Book className="h-8 w-8"/>
                                    </div>
                                    <h3 className="text-xl font-bold mb-2 text-gray-800">Browse Books</h3>
                                    <p className="text-gray-600">
                                        Explore our extensive collection of books, journals, and digital resources.
                                    </p>
                                </div>

                                <div className="bg-white p-6 rounded-lg shadow-md text-center cursor-pointer">
                                    <div
                                        className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 text-emerald-600 rounded-full mb-4">
                                        <Clock className="h-8 w-8"/>
                                    </div>
                                    <h3 className="text-xl font-bold mb-2 text-gray-800">Manage Borrowings</h3>
                                    <p className="text-gray-600">Keep track of your borrowed items and due dates to
                                        avoid late fees.</p>
                                </div>

                                <div className="bg-white p-6 rounded-lg shadow-md text-center cursor-pointer">
                                    <div
                                        className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 text-emerald-600 rounded-full mb-4">
                                        <User className="h-8 w-8"/>
                                    </div>
                                    <h3 className="text-xl font-bold mb-2 text-gray-800">Personal Recommendations</h3>
                                    <p className="text-gray-600">Get personalized book recommendations based on your
                                        reading history.</p>
                                </div>
                            </div>
                        </div>
                    </section>

                    {/* Quick Stats */}
                    <section className="py-12 bg-emerald-600 text-white">
                        <div className="container mx-auto px-4">
                            <div className="grid grid-cols-2 md:grid-cols-3 gap-8 text-center">
                                <div>
                                    <div className="text-4xl font-bold mb-2">{statistics.totalBooks}+</div>
                                    <div className="text-emerald-100">Books</div>
                                </div>
                                <div>
                                    <div className="text-4xl font-bold mb-2">{statistics.totalCategories}+</div>
                                    <div className="text-emerald-100">Categories</div>
                                </div>
                                <div>
                                    <div className="text-4xl font-bold mb-2">{statistics.totalUsers}+</div>
                                    <div className="text-emerald-100">Members</div>
                                </div>
                            </div>
                        </div>
                    </section>
                </main>
            </div>
        </Layout>
    )
};
export default HomePage;