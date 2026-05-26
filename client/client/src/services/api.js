import axios from "axios";

const api = axios.create({
  // Sửa từ 5267 thành 7150 (Cổng HTTPS của Backend)
  baseURL: "https://localhost:7150/api", 
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");

  if (token) config.headers.Authorization = `Bearer ${token}`;

  return config;
});

export default api;