export interface AuthContextType {
    token?: string | null;
    refreshToken?: string | null;
    isAuthenticated: boolean;
    role?: string | null;
    username?: string | null;
    loading: boolean;
    isAdmin: () => boolean;
    isUser: () => boolean;
    isLogin: (token: string, refreshToken: string) => void;
    isLogout: () => void;
}