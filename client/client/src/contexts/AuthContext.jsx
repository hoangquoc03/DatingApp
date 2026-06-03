import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";

// ─── Context ─────────────────────────────────────────────────────────────────
const AuthContext = createContext(null);

// ─── Helper: đọc từ cả hai storage ───────────────────────────────────────────
function readStorage(key) {
  return localStorage.getItem(key) || sessionStorage.getItem(key) || null;
}

function getActiveStorage() {
  // Trả về storage nào đang chứa token
  if (localStorage.getItem("token")) return localStorage;
  if (sessionStorage.getItem("token")) return sessionStorage;
  return null;
}

function clearAllAuth() {
  ["token", "refreshToken", "user"].forEach((k) => {
    localStorage.removeItem(k);
    sessionStorage.removeItem(k);
  });
}

function parseUser() {
  try {
    const raw = readStorage("user");
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

// ─── Provider ─────────────────────────────────────────────────────────────────
export function AuthProvider({ children }) {
  const navigate = useNavigate();
  const [user, setUser] = useState(parseUser);
  const [token, setToken] = useState(() => readStorage("token"));
  const [loading, setLoading] = useState(false);

  const isAuthenticated = Boolean(token);

  // ── Lưu tokens vào storage ─────────────────────────────────────────────────
  const saveAuth = useCallback((data, remember = false) => {
    const storage = remember ? localStorage : sessionStorage;
    const accessToken = data.accessToken || data.token;

    storage.setItem("token", accessToken);
    if (data.refreshToken) storage.setItem("refreshToken", data.refreshToken);
    if (data.user) storage.setItem("user", JSON.stringify(data.user));

    setToken(accessToken);
    setUser(data.user || null);
  }, []);

  // ── Login email/password ───────────────────────────────────────────────────
  const login = useCallback(async (email, password, remember = false) => {
    setLoading(true);
    try {
      const { data } = await api.post("/Auth/login", { email, password });
      saveAuth(data, remember);
      navigate(data.user?.role === 1 ? "/admin" : "/dashboard");
      return { success: true };
    } catch (err) {
      const status = err.response?.status;
      const dataMsg = err.response?.data;
      if (status === 401) return { success: false, message: typeof dataMsg === 'string' ? dataMsg : "Email hoặc mật khẩu không chính xác" };
      return { success: false, message: "Không thể kết nối tới máy chủ. Vui lòng thử lại!" };
    } finally {
      setLoading(false);
    }
  }, [saveAuth, navigate]);

  // ── Login Google ───────────────────────────────────────────────────────────
  const loginGoogle = useCallback(async (credential, remember = false) => {
    setLoading(true);
    try {
      const { data } = await api.post("/Auth/google-login", { credential });
      saveAuth(data, remember);
      navigate(data.user?.role === 1 ? "/admin" : "/dashboard");
      return { success: true };
    } catch {
      return { success: false, message: "Xác thực Google thất bại. Vui lòng thử lại!" };
    } finally {
      setLoading(false);
    }
  }, [saveAuth, navigate]);

  // ── Logout ─────────────────────────────────────────────────────────────────
  const logout = useCallback(async () => {
    try {
      const refreshToken = readStorage("refreshToken");
      if (refreshToken) {
        // Gọi API logout để invalidate refresh token phía server
        await api.post("/Auth/logout", { refreshToken }).catch(() => {});
      }
    } finally {
      clearAllAuth();
      setToken(null);
      setUser(null);
      navigate("/login");
    }
  }, [navigate]);

  // ── Cập nhật user info (sau khi edit profile, onboarding) ───────────────
  const updateUser = useCallback((updatedUser) => {
    setUser((prev) => {
      const merged = typeof updatedUser === "function" 
        ? updatedUser(prev) 
        : { ...prev, ...updatedUser };
        
      const storage = getActiveStorage();
      if (storage) storage.setItem("user", JSON.stringify(merged));
      
      return merged;
    });
  }, []);

  // ── Refresh user profile từ server ────────────────────────────────────────
  const refreshProfile = useCallback(async () => {
    try {
      const { data } = await api.get("/User/profile");
      updateUser(data);
      return data;
    } catch {
      return null;
    }
  }, [updateUser]);

  // ── Khi axios interceptor clear tokens (401 không thể refresh) ─────────────
  useEffect(() => {
    const interval = setInterval(() => {
      const currentToken = readStorage("token");
      if (!currentToken && token) {
        // Token đã bị clear (có thể do interceptor), cập nhật state
        setToken(null);
        setUser(null);
      }
    }, 5000);
    return () => clearInterval(interval);
  }, [token]);

  const value = useMemo(
    () => ({
      user,
      token,
      isAuthenticated,
      loading,
      login,
      loginGoogle,
      logout,
      updateUser,
      refreshProfile,
    }),
    [user, token, isAuthenticated, loading, login, loginGoogle, logout, updateUser, refreshProfile],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// ─── Hook ──────────────────────────────────────────────────────────────────────
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth phải được dùng bên trong <AuthProvider>");
  return ctx;
}
