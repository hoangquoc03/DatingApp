import axios from "axios";

const api = axios.create({
  // Sử dụng cổng HTTPS đồng nhất cho toàn hệ thống
  baseURL: "https://localhost:7150/api", 
});

api.interceptors.request.use((config) => {
  // 🔹 Sửa mục 8: Đọc linh hoạt từ cả hai bộ nhớ để tránh lỗi 401 khi user dùng Session
  const token = localStorage.getItem("token") || sessionStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export default api;