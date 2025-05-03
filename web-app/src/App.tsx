import AppRoutes from "./routes/AppRoutes.tsx";
import {ToastContainer} from "react-toastify";

function App() {

  return (
    <div>
        <ToastContainer
            position="top-right"
            autoClose={3000}
            hideProgressBar={false}
            newestOnTop={false}
            closeOnClick
            rtl={false}
            pauseOnFocusLoss
            draggable
            pauseOnHover
            theme="colored"
        />
      <AppRoutes />
    </div>
  )
}

export default App
