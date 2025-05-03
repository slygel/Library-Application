import Layout from "../components/Layout.tsx";
import BorrowingRequests from "./BorrowingRequests.tsx";

const BorrowingPage = () => {
    return (
        <Layout>
            <div className="p-6 min-h-screen">
                <BorrowingRequests/>
            </div>
        </Layout>
    );
};

export default BorrowingPage;