import { GoogleLogin } from "@react-oauth/google";
import { Eye, EyeOff, Heart, Lock, Mail, Sparkles } from "lucide-react";
import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

export default function Login() {
  const { login, loginGoogle, loading } = useAuth();
  const location = useLocation();
  // Nếu user bị redirect từ trang khác, sau login quay lại trang đó
  const from = location.state?.from?.pathname || "/dashboard";

  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [form, setForm] = useState({ email: "", password: "" });
  const [errors, setErrors] = useState({});

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
    setErrors({});
    const result = await login(
      form.email.trim().toLowerCase(),
      form.password,
      rememberMe,
    );
    if (!result.success) {
      setErrors({ general: result.message });
    }
    // Nếu thành công: AuthContext tự navigate đến trang phù hợp
  };

  return (
    <div className="min-h-[100dvh] w-full flex relative overflow-hidden bg-white">
      {/* Cột trái - Splash Screen nghệ thuật (Ẩn trên Mobile) */}
      <div className="hidden lg:flex w-1/2 relative flex-col justify-between p-16 bg-aura-dark overflow-hidden">
        {/* Lớp ánh sáng Aura Gradient bùng nổ */}
        <div className="absolute inset-0 opacity-70">
          <div className="absolute top-[-10%] left-[-10%] w-[600px] h-[600px] rounded-full bg-aura-pink blur-[120px] mix-blend-screen opacity-50 animate-pulse" style={{ animationDuration: '8s' }}></div>
          <div className="absolute bottom-[-10%] right-[-10%] w-[700px] h-[700px] rounded-full bg-aura-blue blur-[140px] mix-blend-screen opacity-50 animate-pulse" style={{ animationDuration: '10s', animationDelay: '2s' }}></div>
        </div>
        
        {/* Header trái */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-12 h-12 rounded-full bg-white flex items-center justify-center shadow-lg">
            <Heart className="w-6 h-6 text-aura-dark fill-aura-dark" />
          </div>
          <span className="text-3xl font-semibold text-white tracking-tight">
            Aura
          </span>
        </div>

        {/* Nội dung trái */}
        <div className="relative z-10 max-w-lg mt-auto mb-8">
          <h1 className="text-6xl font-medium text-white tracking-tighter leading-[1.05] mb-6">
            Nơi những kết nối đẹp bắt đầu.
          </h1>
          <p className="text-xl text-white/70 font-light leading-relaxed">
            Gặp đúng người phù hợp trong một không gian an toàn, tinh tế và chân thành. Dành riêng cho những trái tim hiện đại.
          </p>
        </div>
      </div>

      {/* Cột phải - Form Đăng nhập */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-6 sm:p-12 relative bg-aura-bg">
        {/* Nền gradient trên mobile */}
        <div className="absolute inset-0 lg:hidden opacity-30 pointer-events-none">
            <div className="absolute top-[-10%] left-[-20%] w-[400px] h-[400px] rounded-full bg-aura-pink blur-[90px]"></div>
            <div className="absolute bottom-[-10%] right-[-20%] w-[400px] h-[400px] rounded-full bg-aura-blue blur-[90px]"></div>
        </div>

        <div className="w-full max-w-md relative z-10">
          {/* Mobile Header */}
          <div className="flex justify-center lg:hidden items-center gap-3 mb-10">
              <div className="w-12 h-12 rounded-full bg-aura-dark flex items-center justify-center">
                  <Heart className="w-6 h-6 text-white fill-white" />
              </div>
              <span className="text-3xl font-semibold tracking-tight text-aura-dark">Aura</span>
          </div>

          <div className="mb-10 text-center lg:text-left">
            <h2 className="text-4xl tracking-tight font-medium text-gray-900 mb-3">Đăng nhập</h2>
            <p className="text-gray-500 text-lg">Chào mừng bạn quay trở lại với Aura.</p>
          </div>

          {/* Form */}
          <div className="glass p-8 sm:p-10 rounded-[32px]">
            <form className="space-y-6" onSubmit={handleSubmit}>
              <div className="space-y-2">
                <label htmlFor="email" className="text-sm text-gray-900 font-medium ml-1">
                  Email
                </label>
                <div className="relative group">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="email"
                    type="email"
                    autoComplete="email"
                    value={form.email}
                    onChange={handleChange}
                    placeholder="yourname@example.com"
                    className={`w-full pl-12 pr-4 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 placeholder:text-gray-400
                      ${
                        errors.email
                          ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                          : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                      }`}
                  />
                </div>
                {errors.email && <p className="text-sm text-red-500 pl-1">{errors.email}</p>}
              </div>

              <div className="space-y-2">
                <label htmlFor="password" className="text-sm text-gray-900 font-medium ml-1">
                  Mật khẩu
                </label>
                <div className="relative group">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    autoComplete="current-password"
                    value={form.password}
                    onChange={handleChange}
                    placeholder="••••••••"
                    className={`w-full pl-12 pr-12 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 placeholder:text-gray-400
                    ${
                      errors.password
                        ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                        : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                    }`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-900 transition-colors p-1"
                  >
                    {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                  </button>
                </div>
                {errors.password && <p className="text-sm text-red-500 pl-1">{errors.password}</p>}
              </div>

              <div className="flex items-center justify-between px-1">
                <label className="flex items-center gap-3 cursor-pointer group">
                  <input
                    type="checkbox"
                    checked={rememberMe}
                    onChange={(e) => setRememberMe(e.target.checked)}
                    className="w-5 h-5 rounded-lg border-2 border-gray-300 text-aura-dark focus:ring-2 focus:ring-gray-900/20 cursor-pointer accent-aura-dark"
                  />
                  <span className="text-sm text-gray-500 group-hover:text-gray-900 transition-colors">
                    Ghi nhớ tôi
                  </span>
                </label>
                <Link
                  to="/forgot-password"
                  className="text-sm font-medium text-gray-500 hover:text-aura-dark transition-colors"
                >
                  Quên mật khẩu?
                </Link>
              </div>

              {errors.general && (
                <div className="text-sm text-red-600 bg-red-50 border border-red-100 px-4 py-3 rounded-xl">
                  {errors.general}
                </div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="btn-magnetic w-full py-4 bg-aura-dark text-white font-medium rounded-2xl hover:bg-black transition-colors flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed shadow-md shadow-gray-900/10"
              >
                {loading ? (
                  <>
                    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Đang xử lý...
                  </>
                ) : (
                  <>Đăng nhập</>
                )}
              </button>
            </form>

            <div className="relative my-8">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-200" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-4 bg-transparent text-gray-400">Hoặc</span>
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex justify-center w-full btn-magnetic">
                <GoogleLogin
                  onSuccess={async (credentialResponse) => {
                    setErrors({});
                    const result = await loginGoogle(credentialResponse.credential, rememberMe);
                    if (!result.success) setErrors({ general: result.message });
                  }}
                  onError={() => setErrors({ general: "Đăng nhập Google thất bại." })}
                  useOneTap
                  theme="outline"
                  size="large"
                  shape="pill"
                  width="100%"
                  locale="vi"
                />
              </div>

              <button
                type="button"
                className="btn-magnetic w-full py-[10px] px-4 bg-white border border-gray-200 rounded-full hover:bg-gray-50 transition-colors flex items-center justify-center gap-3 font-medium text-gray-700 shadow-sm"
              >
                <svg className="w-5 h-5" viewBox="0 0 24 24" fill="#1877F2">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
                </svg>
                <span className="text-sm">Tiếp tục với Facebook</span>
              </button>
            </div>

            <div className="mt-10 text-center">
              <p className="text-sm text-gray-500">
                Chưa có tài khoản?{" "}
                <Link to="/register" className="font-semibold text-aura-dark hover:underline underline-offset-4">
                  Đăng ký ngay
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}