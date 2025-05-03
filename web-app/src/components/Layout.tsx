import Header from "./layout/Header.tsx";
import Footer from "./layout/Footer.tsx";
import LayoutProps from "../types/LayoutProps.ts";

export default function Layout({ children }: LayoutProps) {

    return (
        <div className="flex min-h-screen flex-col bg-white dark:bg-gray-950">
            <Header/>

            {/* Main content */}
            <main>{children}</main>

            <Footer/>
        </div>
    )
}
