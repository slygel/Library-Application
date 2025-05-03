import Layout from "../components/Layout.js";
import CategoryManagement from "./categories/CategoryManagement.tsx";

const CategoryPage = () => {
    return (
        <Layout>
            <div className="p-6">
                <CategoryManagement/>
            </div>
        </Layout>
    );
};

export default CategoryPage;