import { useState } from "react";
import { Link } from "react-router-dom";
import api from "../services/api";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    setLoading(true);
    try {
      const { data } = await api.post("/Auth/forgot-password", { email: email.trim() });
      setMessage(data.message || "Đã gửi email đặt lại mật khẩu.");
    } catch {
      setError("Không thể gửi yêu cầu. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md bg-white rounded-2xl p-6 shadow">
        <h1 className="text-2xl font-semibold mb-2">Quên mật khẩu</h1>
        <p className="text-sm text-gray-500 mb-6">
          Nhập email đã đăng ký, chúng tôi sẽ gửi link đặt lại mật khẩu.
        </p>

        <form onSubmit={onSubmit} className="space-y-4">
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="yourname@example.com"
            className="w-full border rounded-xl px-4 py-3 outline-none focus:ring-2 focus:ring-pink-200"
            required
          />
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-pink-500 to-purple-400 text-white rounded-xl py-3 disabled:opacity-60"
          >
            {loading ? "Đang gửi..." : "Gửi link đặt lại"}
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
