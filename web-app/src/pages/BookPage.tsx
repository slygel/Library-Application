import Layout from "../components/Layout.tsx";
import BookManagement from "./books/BookManagement.tsx";
import {useAuth} from "../contexts/AuthContext.tsx";
import BookList from "./books/BookList.tsx";

const BookPage = () => {
    const { isAdmin } = useAuth();
    return (
        <Layout>
            <div className="p-6">
                {isAdmin() ? <BookManagement /> : <BookList />}
            </div>
        </Layout>
    );
};

export default BookPage;