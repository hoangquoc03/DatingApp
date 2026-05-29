import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Heart, Loader2 } from "lucide-react";

/**
 * ProtectedRoute — Bảo vệ các trang cần đăng nhập.
 *
 * Sử dụng:
 *   <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
 *
 * Hoặc wrapping toàn bộ layout:
 *   <Route element={<ProtectedRoute />}>
 *     <Route path="/dashboard" element={<Dashboard />} />
 *     <Route path="/discover"  element={<Discover />}  />
 *   </Route>
 */
export default function ProtectedRoute({ children }) {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();

  // Đang kiểm tra trạng thái auth — hiển thị loading
  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-pink-50 via-purple-50 to-pink-100">
        <div className="flex flex-col items-center gap-4">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center shadow-lg shadow-pink-500/30 animate-pulse">
            <Heart className="w-8 h-8 text-white fill-white" />
          </div>
          <Loader2 className="w-6 h-6 text-[#FF5C9A] animate-spin" />
          <p className="text-[#6B7280] text-sm">Đang xác thực...</p>
        </div>
      </div>
    );
  }

  // Chưa đăng nhập → redirect về /login, lưu lại trang muốn vào
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Đã đăng nhập → render children
  return children;
}
