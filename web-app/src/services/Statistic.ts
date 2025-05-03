import axiosInstance from "../utils/axiosInstance.ts";

export interface Statistic {
    totalBooks: number;
    totalCategories: number;
    totalUsers: number;
}
export const getStatistic = async () => {
    const result = await axiosInstance.get<Statistic>("/statistics");
    return result.data;
}