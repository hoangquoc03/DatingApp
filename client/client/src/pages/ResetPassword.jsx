import { useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import api from "../services/api";

function useTokenFromQuery() {
  const { search } = useLocation();
  return useMemo(() => new URLSearchParams(search).get("token") || "", [search]);
}

export default function ResetPassword() {
  const token = useTokenFromQuery();
  const navigate = useNavigate();
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    if (!token) {
      setError("Link không hợp lệ hoặc thiếu token.");
      return;
    }
    if (password.length < 6) {
      setError("Mật khẩu mới phải từ 6 ký tự.");
      return;
    }
    if (password !== confirmPassword) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }

    setLoading(true);
    try {
      const { data } = await api.post("/Auth/reset-password", {
        token,
        newPassword: password,
      });
      setMessage(data.message || "Đổi mật khẩu thành công.");
      setTimeout(() => navigate("/login"), 1200);
    } catch (err) {
      setError(err.response?.data || "Không thể đặt lại mật khẩu.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md bg-white rounded-2xl p-6 shadow">
        <h1 className="text-2xl font-semibold mb-2">Đặt lại mật khẩu</h1>
        <p className="text-sm text-gray-500 mb-6">Nhập mật khẩu mới cho tài khoản của bạn.</p>

        <form onSubmit={onSubmit} className="space-y-4">
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Mật khẩu mới"
            className="w-full border rounded-xl px-4 py-3 outline-none focus:ring-2 focus:ring-pink-200"
            required
          />
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Xác nhận mật khẩu mới"
            className="w-full border rounded-xl px-4 py-3 outline-none focus:ring-2 focus:ring-pink-200"
            required
          />
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-pink-500 to-purple-400 text-white rounded-xl py-3 disabled:opacity-60"
          >
            {loading ? "Đang cập nhật..." : "Cập nhật mật khẩu"}
          </button>
        </form>

        {message && <p className="mt-4 text-sm text-green-600">{message}</p>}
        {error && <p className="mt-4 text-sm text-red-600">{error}</p>}

        <Link to="/login" className="block mt-6 text-sm text-pink-500 hover:underline">
          Quay lại đăng nhập
        </Link>
      </div>
    </div>
  );
}
