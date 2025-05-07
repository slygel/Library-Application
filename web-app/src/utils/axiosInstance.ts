import axios, { AxiosRequestConfig } from "axios";
const excludedUrls = ["/auth/login", "/auth/register", "/auth/refresh-token"];

// Create instance Axios
const axiosInstance = axios.create({
    baseURL: "http://localhost:5280/api/v1",
    headers: { "Content-Type": "application/json" },
});

// Store token refresh logic
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

// Function to add request to waiting queue while refreshing token
const onRefreshed = (token: string) => {
    refreshSubscribers.map((callback) => callback(token));
    refreshSubscribers = [];
};

// Add an interceptor to automatically refresh the token when receiving a 401 error
axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

        // If you get error 401 (Unauthorized)
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            if (!isRefreshing) {
                isRefreshing = true;

                try {
                    // Send token refresh request
                    const refreshToken = localStorage.getItem("refreshToken");

                    if (!refreshToken) {
                        throw new Error("No refresh token available");
                    }

                    const res = await axios.post(
                        "http://localhost:5280/api/v1/auth/refresh-token",
                        { refreshToken }
                    );

                    // Store new access token and refresh token
                    const { accessToken, refreshToken: newRefreshToken } = res.data;
                    localStorage.setItem("accessToken", accessToken);
                    localStorage.setItem("refreshToken", newRefreshToken);

                    axiosInstance.defaults.headers["Authorization"] = `Bearer ${accessToken}`;

                    // Resend all delayed requests
                    onRefreshed(accessToken);

                    isRefreshing = false;

                    // Retry the original request with new token
                    originalRequest.headers = originalRequest.headers || {};
                    originalRequest.headers["Authorization"] = `Bearer ${accessToken}`;
                    return axios(originalRequest);

                } catch (err) {
                    localStorage.removeItem("accessToken");
                    localStorage.removeItem("refreshToken");
                    isRefreshing = false;

                    // Redirect to login page
                    window.location.href = "/login";
                    return Promise.reject(err);
                }
            }

            return new Promise((resolve) => {
                refreshSubscribers.push((token: string) => {
                    originalRequest.headers = originalRequest.headers || {};
                    originalRequest.headers["Authorization"] = `Bearer ${token}`;
                    resolve(axios(originalRequest));
                });
            });
        }

        return Promise.reject(error);
    }
);

axiosInstance.interceptors.request.use((config) => {
    const token = localStorage.getItem("accessToken");

    // Check if the URL is in the exclusion list
    const isExcluded = config.url ? excludedUrls.some((url) => config.url?.includes(url)) : false;

    if (token && !isExcluded) {
        config.headers["Authorization"] = `Bearer ${token}`;
    }

    return config;
});

export default axiosInstance;
