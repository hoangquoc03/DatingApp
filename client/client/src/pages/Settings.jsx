import { useState } from "react";
import { useAuth } from "../contexts/AuthContext";
import api from "../services/api";
import { ArrowLeft, Bell, BellOff, Lock, Trash2, ShieldCheck, ShieldAlert } from "lucide-react";
import { Link } from "react-router-dom";

export default function Settings() {
  const { user, logout } = useAuth();
  const [activeTab, setActiveTab] = useState("security");
  const [form, setForm] = useState({ currentPassword: "", newPassword: "" });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  const [notifications, setNotifications] = useState(true);

  const handleChange = (e) => {
    setForm((prev) => ({ ...prev, [e.target.id]: e.target.value }));
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (user.passwordHash === "OAUTH_EXTERNAL_ACCOUNT_NO_PASSWORD") {
      setError("Tài khoản đăng nhập bằng Mạng xã hội không thể đổi mật khẩu.");
      return;
    }
    setLoading(true);
    setMessage(null);
    setError(null);
    try {
      await api.put("/Settings/password", form);
      setMessage("Đổi mật khẩu thành công!");
      setForm({ currentPassword: "", newPassword: "" });
    } catch (err) {
      setError(err.response?.data || "Lỗi khi đổi mật khẩu.");
    } finally {
      setLoading(false);
    }
  };

  const handleDeactivate = async () => {
    if (!window.confirm("Bạn có chắc chắn muốn vô hiệu hóa tài khoản? Mọi người sẽ không thể thấy bạn nữa.")) {
      return;
    }
    setLoading(true);
    try {
      await api.delete("/Settings/account");
      alert("Tài khoản của bạn đã được vô hiệu hóa.");
      logout();
    } catch (err) {
      setError("Lỗi khi vô hiệu hóa tài khoản.");
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#FDFBF7] font-sans pb-20">
      {/* Header */}
      <header className="sticky top-0 z-50 bg-[#FDFBF7]/80 backdrop-blur-md border-b border-gray-100 px-4 py-4 flex items-center gap-3">
        <Link to="/dashboard" className="p-2 -ml-2 rounded-full hover:bg-gray-100 transition-colors">
          <ArrowLeft className="w-6 h-6 text-gray-800" />
        </Link>
        <h1 className="text-xl font-bold tracking-tight text-gray-900">Cài đặt</h1>
      </header>

      <main className="max-w-2xl mx-auto px-4 py-8">
        {/* Tabs */}
        <div className="flex gap-2 p-1 bg-gray-100/80 rounded-2xl mb-8">
          <button
            onClick={() => setActiveTab("security")}
            className={`flex-1 py-3 px-4 rounded-xl text-sm font-medium transition-all duration-300 ${
              activeTab === "security" 
                ? "bg-white text-gray-900 shadow-sm" 
                : "text-gray-500 hover:text-gray-700"
            }`}
          >
            Bảo mật
          </button>
          <button
            onClick={() => setActiveTab("notifications")}
            className={`flex-1 py-3 px-4 rounded-xl text-sm font-medium transition-all duration-300 ${
              activeTab === "notifications" 
                ? "bg-white text-gray-900 shadow-sm" 
                : "text-gray-500 hover:text-gray-700"
            }`}
          >
            Thông báo
          </button>
        </div>

        {/* Content */}
        <div className="space-y-6">
          {activeTab === "security" && (
            <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
              {/* Đổi mật khẩu */}
              <section className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 rounded-full bg-[#FF5C9A]/10 flex items-center justify-center">
                    <Lock className="w-5 h-5 text-[#FF5C9A]" />
                  </div>
                  <div>
                    <h2 className="font-semibold text-gray-900">Đổi mật khẩu</h2>
                    <p className="text-sm text-gray-500">Cập nhật mật khẩu để bảo vệ tài khoản</p>
                  </div>
                </div>

                {message && <div className="mb-6 p-4 bg-green-50 text-green-700 rounded-xl text-sm flex gap-2"><ShieldCheck className="w-5 h-5"/>{message}</div>}
                {error && <div className="mb-6 p-4 bg-red-50 text-red-700 rounded-xl text-sm flex gap-2"><ShieldAlert className="w-5 h-5"/>{error}</div>}

                <form onSubmit={handleChangePassword} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Mật khẩu hiện tại</label>
                    <input
                      type="password"
                      id="currentPassword"
                      value={form.currentPassword}
                      onChange={handleChange}
                      className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#FF5C9A]/20 focus:border-[#FF5C9A] transition-all"
                      placeholder="••••••••"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Mật khẩu mới</label>
                    <input
                      type="password"
                      id="newPassword"
                      value={form.newPassword}
                      onChange={handleChange}
                      className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#FF5C9A]/20 focus:border-[#FF5C9A] transition-all"
                      placeholder="••••••••"
                      required
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={loading}
                    className="w-full py-3 bg-gray-900 text-white rounded-xl font-medium hover:bg-gray-800 transition-colors disabled:opacity-50"
                  >
                    {loading ? "Đang xử lý..." : "Cập nhật mật khẩu"}
                  </button>
                </form>
              </section>

              {/* Vô hiệu hóa tài khoản */}
              <section className="bg-red-50/50 p-6 rounded-3xl border border-red-100">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 rounded-full bg-red-100 flex items-center justify-center">
                    <Trash2 className="w-5 h-5 text-red-600" />
                  </div>
                  <div>
                    <h2 className="font-semibold text-red-900">Vô hiệu hóa tài khoản</h2>
                    <p className="text-sm text-red-700/70">Ẩn hồ sơ của bạn khỏi mọi người</p>
                  </div>
                </div>
                <p className="text-sm text-red-800/80 mb-6">
                  Khi vô hiệu hóa, hồ sơ của bạn sẽ bị ẩn hoàn toàn khỏi hệ thống. Bạn không thể quẹt hay nhắn tin cho đến khi đăng nhập lại.
                </p>
                <button
                  onClick={handleDeactivate}
                  disabled={loading}
                  className="w-full py-3 bg-red-600 text-white rounded-xl font-medium hover:bg-red-700 transition-colors disabled:opacity-50"
                >
                  Vô hiệu hóa ngay
                </button>
              </section>
            </div>
          )}

          {activeTab === "notifications" && (
            <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
              <section className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className={`w-12 h-12 rounded-full flex items-center justify-center transition-colors ${notifications ? "bg-[#C8B6FF]/20" : "bg-gray-100"}`}>
                    {notifications ? <Bell className="w-6 h-6 text-[#C8B6FF]" /> : <BellOff className="w-6 h-6 text-gray-400" />}
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900">Thông báo ứng dụng</h3>
                    <p className="text-sm text-gray-500">Nhận thông báo khi có Match mới</p>
                  </div>
                </div>
                <button 
                  onClick={() => setNotifications(!notifications)}
                  className={`relative inline-flex h-7 w-12 items-center rounded-full transition-colors ${notifications ? "bg-[#FF5C9A]" : "bg-gray-200"}`}
                >
                  <span className={`inline-block h-5 w-5 transform rounded-full bg-white transition-transform ${notifications ? "translate-x-6" : "translate-x-1"}`} />
                </button>
              </section>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
