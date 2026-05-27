import { GoogleLogin } from "@react-oauth/google";
import api from "../services/api";
import { Eye, EyeOff, Heart, Lock, Mail, Sparkles } from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

export default function Login() {
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: "",
    password: "",
  });

  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // Xử lý thay đổi dữ liệu ô nhập
  const handleChange = (e) => {
    const { id, value } = e.target;

    setForm((prev) => ({
      ...prev,
      [id]: value,
    }));

    setErrors((prev) => ({
      ...prev,
      [id]: "",
      general: "",
    }));
  };

  // Xác thực dữ liệu form client-side
  const validateForm = () => {
    const newErrors = {};

    if (!form.email.trim()) {
      newErrors.email = "Vui lòng nhập email";
    }

    if (!form.password.trim()) {
      newErrors.password = "Vui lòng nhập mật khẩu";
    }

    return newErrors;
  };

  // Xử lý gửi form đăng nhập thông thường
  const handleSubmit = async (e) => {
    e.preventDefault();

    const validationErrors = validateForm();
    if (Object.keys(validationErrors).length) {
      setErrors(validationErrors);
      return;
    }

    try {
      setLoading(true);
      setErrors({});

      const payload = {
        email: form.email.trim().toLowerCase(),
        password: form.password,
      };

      const { data } = await api.post("/Auth/login", payload);
      const storage = rememberMe ? localStorage : sessionStorage;

      storage.setItem("token", data.token);
      storage.setItem("user", JSON.stringify(data.user));

      navigate("/dashboard");
    } catch (err) {
      console.error("Lỗi Đăng Nhập:", err);
      if (err.response?.status === 401) {
        setErrors({
          general: "Email hoặc mật khẩu không chính xác",
        });
      } else {
        setErrors({
          general: "Không thể kết nối tới máy chủ. Vui lòng kiểm tra lại Backend!",
        });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen w-full relative overflow-hidden">
      {/* Khối nền Gradient chuyển màu chuyển động */}
      <div className="absolute inset-0 bg-gradient-to-br from-pink-50 via-purple-50 to-pink-100">
        <div className="absolute top-0 left-1/4 w-96 h-96 bg-[#FF5C9A]/20 rounded-full blur-[120px] animate-pulse" />
        <div
          className="absolute bottom-0 right-1/4 w-96 h-96 bg-[#C8B6FF]/30 rounded-full blur-[120px] animate-pulse"
          style={{ animationDelay: "1s" }}
        />
      </div>

      {/* Nội dung giao diện chính */}
      <div className="relative min-h-screen flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-7xl mx-auto">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            {/* Cột trái - Thương hiệu & Thống kê ứng dụng */}
            <div className="hidden lg:flex flex-col justify-center space-y-8">
              <div className="space-y-6">
                <div className="flex items-center gap-3">
                  <div className="relative">
                    <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center shadow-lg shadow-pink-500/30">
                      <Heart className="w-7 h-7 text-white fill-white" />
                    </div>
                    <div className="absolute -top-1 -right-1 w-4 h-4 bg-gradient-to-br from-yellow-400 to-pink-400 rounded-full animate-pulse" />
                  </div>
                  <span className="text-3xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                    Aura Dating
                  </span>
                </div>

                <div className="space-y-4">
                  <h1 className="text-5xl font-bold text-[#1F2937] leading-tight">
                    Nơi những kết nối đẹp bắt đầu
                  </h1>
                  <p className="text-xl text-[#6B7280] leading-relaxed">
                    Gặp đúng người phù hợp trong không gian an toàn, tinh tế và chân thành.
                  </p>
                </div>

                <div className="space-y-4 pt-8">
                  <div className="bg-white/60 backdrop-blur-lg rounded-2xl p-6 shadow-xl border border-white/50 transform hover:scale-105 transition-transform duration-300">
                    <div className="flex items-center gap-4">
                      <div className="w-14 h-14 rounded-xl bg-gradient-to-br from-pink-400 to-pink-500 flex items-center justify-center">
                        <Sparkles className="w-7 h-7 text-white" />
                      </div>
                      <div>
                        <p className="text-sm text-[#6B7280]">Kết nối mới hôm nay</p>
                        <p className="text-2xl font-bold text-[#1F2937]">2,547</p>
                      </div>
                    </div>
                  </div>

                  <div className="bg-white/60 backdrop-blur-lg rounded-2xl p-6 shadow-xl border border-white/50 transform hover:scale-105 transition-transform duration-300">
                    <div className="flex items-center gap-4">
                      <div className="w-14 h-14 rounded-xl bg-gradient-to-br from-purple-400 to-purple-500 flex items-center justify-center">
                        <Heart className="w-7 h-7 text-white fill-white" />
                      </div>
                      <div>
                        <p className="text-sm text-[#6B7280]">Cặp đôi thành công</p>
                        <p className="text-2xl font-bold text-[#1F2937]">15,234</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Cột phải - Thẻ Đăng nhập */}
            <div className="flex items-center justify-center">
              <div className="w-full max-w-md">
                <div className="bg-white/70 backdrop-blur-xl rounded-[32px] shadow-2xl border border-white/60 p-10 space-y-8">
                  {/* Logo hiển thị trên giao diện Mobile */}
                  <div className="lg:hidden flex items-center justify-center gap-3 mb-6">
                    <div className="relative">
                      <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center shadow-lg shadow-pink-500/30">
                        <Heart className="w-6 h-6 text-white fill-white" />
                      </div>
                    </div>
                    <span className="text-2xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                      Aura Dating
                    </span>
                  </div>

                  <div className="text-center space-y-2">
                    <h2 className="text-3xl font-bold text-[#1F2937]">Chào mừng trở lại</h2>
                    <p className="text-[#6B7280]">Đăng nhập để tiếp tục hành trình kết nối của bạn</p>
                  </div>

                  {/* Form thu thập dữ liệu đăng nhập thường */}
                  <form className="space-y-6" onSubmit={handleSubmit}>
                    <div className="space-y-2">
                      <label htmlFor="email" className="text-sm text-[#1F2937] font-medium">
                        Email
                      </label>
                      <div className="relative group">
                        <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280]" />
                        <input
                          id="email"
                          type="email"
                          autoComplete="email"
                          value={form.email}
                          onChange={handleChange}
                          placeholder="yourname@example.com"
                          className={`w-full pl-12 pr-4 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300
                            ${
                              errors.email
                                ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
                                : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
                            }`}
                        />
                      </div>
                      {errors.email && <p className="text-sm text-red-500 pl-1">{errors.email}</p>}
                    </div>

                    <div className="space-y-2">
                      <label htmlFor="password" className="text-sm text-[#1F2937] font-medium">
                        Mật khẩu
                      </label>
                      <div className="relative group">
                        <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] group-focus-within:text-[#FF5C9A] transition-colors" />
                        <input
                          id="password"
                          type={showPassword ? "text" : "password"}
                          autoComplete="current-password"
                          value={form.password}
                          onChange={handleChange}
                          placeholder="••••••••"
                          className={`w-full pl-12 pr-12 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300
                          ${
                            errors.password
                              ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
                              : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
                          }`}
                        />
                        <button
                          type="button"
                          onClick={() => setShowPassword(!showPassword)}
                          className="absolute right-4 top-1/2 -translate-y-1/2 text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
                        >
                          {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                        </button>
                      </div>
                      {errors.password && <p className="text-sm text-red-500 pl-1">{errors.password}</p>}
                    </div>

                    <div className="flex items-center justify-between">
                      <label className="flex items-center gap-2 cursor-pointer group">
                        <input
                          type="checkbox"
                          checked={rememberMe}
                          onChange={(e) => setRememberMe(e.target.checked)}
                          className="w-5 h-5 rounded-lg border-2 border-gray-300 text-[#FF5C9A] focus:ring-2 focus:ring-[#FF5C9A]/20 cursor-pointer"
                        />
                        <span className="text-sm text-[#6B7280] group-hover:text-[#1F2937] transition-colors">
                          Ghi nhớ đăng nhập
                        </span>
                      </label>
                      <button type="button" className="text-sm font-medium text-[#FF5C9A] hover:text-[#C8B6FF] transition-colors">
                        Quên mật khẩu?
                      </button>
                    </div>

                    {errors.general && (
                      <div className="text-sm text-red-500 bg-red-50 border border-red-200 px-4 py-3 rounded-xl animate-pulse">
                        {errors.general}
                      </div>
                    )}

                    <button
                      type="submit"
                      disabled={loading}
                      className="w-full py-4 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white font-medium rounded-2xl shadow-lg hover:scale-[1.02] active:scale-[0.98] transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
                    >
                      {loading ? (
                        <>
                          <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                          Đang đăng nhập...
                        </>
                      ) : (
                        <>
                          <Heart className="w-5 h-5 fill-white" />
                          Đăng nhập
                        </>
                      )}
                    </button>
                  </form>

                  {/* Thanh ngăn cách giữa các hình thức đăng nhập */}
                  <div className="relative">
                    <div className="absolute inset-0 flex items-center">
                      <div className="w-full border-t border-gray-300" />
                    </div>
                    <div className="relative flex justify-center text-sm">
                      <span className="px-4 bg-white/70 text-[#6B7280]">Hoặc tiếp tục với</span>
                    </div>
                  </div>

                  {/* Khối xử lý Đăng nhập mạng xã hội (Tích hợp luồng Google Credential mới) */}
                  <div className="flex flex-col items-center justify-center gap-4 w-full pt-2">
                    <div className="flex justify-center w-full transform hover:scale-[1.01] transition-transform">
                      <GoogleLogin
                        onSuccess={async (credentialResponse) => {
                          try {
                            setLoading(true);
                            setErrors({});

                            const response = await api.post("/Auth/google-login", {
                              credential: credentialResponse.credential,
                            });

                            const { token, user } = response.data;
                            const storage = rememberMe ? localStorage : sessionStorage;

                            storage.setItem("token", token);
                            storage.setItem("user", JSON.stringify(user));

                            navigate("/dashboard");
                          } catch (err) {
                            console.error("Đăng nhập Google thất bại:", err);
                            setErrors({
                              general: "Xác thực Google thành công nhưng Server từ chối kết nối hoặc tạo User thất bại.",
                            });
                          } finally {
                            setLoading(false);
                          }
                        }}
                        onError={() => {
                          console.log("Google Sign-In Failed");
                          setErrors({ general: "Yêu cầu đăng nhập qua tài khoản Google bị hủy bỏ." });
                        }}
                        useOneTap
                        theme="filled_blue"
                        shape="circle"
                        locale="vi"
                      />
                    </div>

                    <button
                      type="button"
                      className="w-full py-3 px-4 bg-white border-2 border-gray-200 rounded-2xl hover:border-[#C8B6FF] hover:shadow-sm transition-all duration-300 flex items-center justify-center gap-2 font-medium text-[#1F2937]"
                    >
                      <svg className="w-5 h-5" viewBox="0 0 24 24" fill="#1877F2">
                        <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
                      </svg>
                      <span className="text-sm">Tiếp tục với Facebook</span>
                    </button>
                  </div>

                  {/* Chân trang điều hướng biểu mẫu */}
                  <div className="text-center space-y-4 pt-4">
                    <p className="text-sm text-[#6B7280]">
                      Chưa có tài khoản?{" "}
                      <Link to="/register" className="font-medium text-[#FF5C9A] hover:text-[#C8B6FF] transition-colors">
                        Đăng ký miễn phí
                      </Link>
                    </p>

                    <div className="pt-4 space-y-2 border-t border-gray-200">
                      <div className="flex items-center justify-center gap-2 text-xs text-[#6B7280]">
                        <Lock className="w-4 h-4" />
                        <span>Thông tin của bạn luôn được bảo mật tuyệt đối</span>
                      </div>
                      <div className="flex items-center justify-center gap-2 text-xs text-[#6B7280]">
                        <Heart className="w-4 h-4 fill-current" />
                        <span>Hơn 1.000.000 người đã bắt đầu kết nối tại Aura Dating</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}