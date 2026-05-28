import axios from "axios";

const api = axios.create({
  // Sử dụng cổng HTTPS đồng nhất cho toàn hệ thống
  baseURL: "https://localhost:7150/api", 
});

const REFRESH_URL = "/Auth/refresh";
let isRefreshing = false;
let pendingQueue = [];

const saveAuthTokens = (accessToken, refreshToken) => {
  const storages = [localStorage, sessionStorage];
  for (const storage of storages) {
    if (storage.getItem("token") || storage.getItem("refreshToken")) {
      storage.setItem("token", accessToken);
      if (refreshToken) storage.setItem("refreshToken", refreshToken);
    }
  }
};

const clearAuthTokens = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  sessionStorage.removeItem("token");
  sessionStorage.removeItem("refreshToken");
};

api.interceptors.request.use((config) => {
  // 🔹 Sửa mục 8: Đọc linh hoạt từ cả hai bộ nhớ để tránh lỗi 401 khi user dùng Session
  const token = localStorage.getItem("token") || sessionStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (!error.response || error.response.status !== 401 || originalRequest?._retry) {
      return Promise.reject(error);
    }

    const refreshToken =
      localStorage.getItem("refreshToken") || sessionStorage.getItem("refreshToken");
    if (!refreshToken) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const { data } = await axios.post(
        `${api.defaults.baseURL}${REFRESH_URL}`,
        { refreshToken },
      );
      const newToken = data.accessToken || data.token;
      saveAuthTokens(newToken, data.refreshToken);
      pendingQueue.forEach(({ resolve }) => resolve(newToken));
      pendingQueue = [];
      originalRequest.headers.Authorization = `Bearer ${newToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      pendingQueue.forEach(({ reject }) => reject(refreshError));
      pendingQueue = [];
      clearAuthTokens();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

export default api;