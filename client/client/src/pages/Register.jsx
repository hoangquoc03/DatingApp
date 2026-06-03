import { GoogleLogin } from "@react-oauth/google";
import api from "../services/api";
import {
  CalendarDays,
  Eye,
  EyeOff,
  Heart,
  Lock,
  Mail,
  Sparkles,
  User,
} from "lucide-react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

export default function App() {
  const navigate = useNavigate();
  const { login, loginGoogle } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const calculateAge = (birthDateString) => {
    const [year, month, day] = birthDateString.split("-").map(Number);

    const today = new Date();
    const birthDate = new Date(year, month - 1, day);

    let age = today.getFullYear() - birthDate.getFullYear();

    const hasHadBirthdayThisYear =
      today.getMonth() > birthDate.getMonth() ||
      (today.getMonth() === birthDate.getMonth() &&
        today.getDate() >= birthDate.getDate());

    if (!hasHadBirthdayThisYear) {
      age--;
    }

    return age;
  };
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
    gender: "",
    dateOfBirth: "",
    agreeTerms: false,
  });
  const handleChange = (e) => {
    const { id, value } = e.target;

    setForm((prev) => ({
      ...prev,
      [id]: value,
    }));

    // clear lỗi field đang nhập
    setErrors((prev) => ({
      ...prev,
      [id]: "",
    }));
  };

  const validateForm = () => {
    const newErrors = {};

    // Full name
    if (!form.fullName.trim()) {
      newErrors.fullName = "Vui lòng nhập họ và tên";
    }

    // Email
    if (!form.email.trim()) {
      newErrors.email = "Vui lòng nhập email";
    } else if (!/\S+@\S+\.\S+/.test(form.email)) {
      newErrors.email = "Email không hợp lệ";
    }

    // Password
    if (!form.password) {
      newErrors.password = "Vui lòng nhập mật khẩu";
    } else if (form.password.length < 6) {
      newErrors.password = "Mật khẩu phải từ 6 ký tự";
    }

    // Confirm password
    if (!form.confirmPassword) {
      newErrors.confirmPassword = "Vui lòng xác nhận mật khẩu";
    } else if (form.password !== form.confirmPassword) {
      newErrors.confirmPassword = "Mật khẩu xác nhận không khớp";
    }

    // Gender
    if (!form.gender) {
      newErrors.gender = "Vui lòng chọn giới tính";
    }

    // Date of birth
    if (!form.dateOfBirth) {
      newErrors.dateOfBirth = "Vui lòng chọn ngày sinh";
    } else {
      const age = calculateAge(form.dateOfBirth);

      if (age < 18) {
        newErrors.dateOfBirth = "Bạn phải từ 18 tuổi trở lên";
      }
    }

    // Agree Terms
    if (!form.agreeTerms) {
      newErrors.agreeTerms = "Bạn cần đồng ý điều khoản sử dụng";
    }

    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validationErrors = validateForm();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      console.log("Validate lỗi:", validationErrors);
      return;
    }

    try {
      setLoading(true);
      setErrors({});

      const payload = {
        fullName: form.fullName,
        email: form.email,
        password: form.password,
        gender: Number(form.gender), // enum backend
        dateOfBirth: form.dateOfBirth,
      };

      console.log("📤 Dữ liệu gửi lên API:", payload);

      const response = await api.post("/Auth/register", payload);

      console.log("✅ Response thành công:", response.data);

      if (response.status === 200) {
        // Tự động đăng nhập sau khi đăng ký thành công
        const loginResult = await login(form.email, form.password);
        if (loginResult.success) {
          navigate("/onboarding");
        } else {
          navigate("/login");
        }
      }
    } catch (err) {
      console.log("❌ Full Error:", err);

      if (err.response) {
        console.log("📌 Status:", err.response.status);
        console.log("📌 Data:", JSON.stringify(err.response.data, null, 2));
        console.log("📌 Headers:", err.response.headers);
      } else if (err.request) {
        console.log("📌 Không nhận được response:", err.request);
      } else {
        console.log("📌 Error message:", err.message);
      }

      if (err.response?.status === 400) {
        const data = err.response.data;

        // lỗi validation asp.net
        if (data.errors) {
          console.log(
            "📌 Validation Errors:",
            JSON.stringify(data.errors, null, 2),
          );

          const firstKey = Object.keys(data.errors)[0];
          const firstMessage = data.errors[firstKey][0];

          setErrors({
            general: firstMessage,
          });
        } else if (
          typeof data === "string" &&
          data.toLowerCase().includes("email")
        ) {
          setErrors({
            email: "Email đã tồn tại",
          });
        } else {
          setErrors({
            general: "Dữ liệu không hợp lệ",
          });
        }
      } else {
        setErrors({
          general: "Lỗi kết nối máy chủ",
        });
      }
    } finally {
      setLoading(false);
    }
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
            Mỗi kết nối đẹp đều bắt đầu từ một bước đầu tiên.
          </h1>
          <p className="text-xl text-white/70 font-light leading-relaxed">
            Tham gia Aura Dating để gặp đúng người phù hợp trong không gian an toàn, tinh tế và chân thành. Dành riêng cho những trái tim hiện đại.
          </p>
        </div>
      </div>

      {/* Cột phải - Form Đăng ký */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-6 sm:p-12 relative bg-aura-bg overflow-y-auto max-h-[100dvh]">
        {/* Nền gradient trên mobile */}
        <div className="absolute inset-0 lg:hidden opacity-30 pointer-events-none fixed">
            <div className="absolute top-[-10%] left-[-20%] w-[400px] h-[400px] rounded-full bg-aura-pink blur-[90px]"></div>
            <div className="absolute bottom-[-10%] right-[-20%] w-[400px] h-[400px] rounded-full bg-aura-blue blur-[90px]"></div>
        </div>

        <div className="w-full max-w-md relative z-10 py-10">
          {/* Mobile Header */}
          <div className="flex justify-center lg:hidden items-center gap-3 mb-10">
              <div className="w-12 h-12 rounded-full bg-aura-dark flex items-center justify-center">
                  <Heart className="w-6 h-6 text-white fill-white" />
              </div>
              <span className="text-3xl font-semibold tracking-tight text-aura-dark">Aura</span>
          </div>

          <div className="mb-10 text-center lg:text-left">
            <h2 className="text-4xl tracking-tight font-medium text-gray-900 mb-3">Tạo tài khoản mới</h2>
            <p className="text-gray-500 text-lg">Bắt đầu hành trình kết nối của bạn chỉ trong vài bước.</p>
          </div>

          {/* Form */}
          <div className="glass p-8 sm:p-10 rounded-[32px]">
            <form className="space-y-6" onSubmit={handleSubmit}>
              
              {/* Họ và tên */}
              <div className="space-y-2">
                <label htmlFor="fullName" className="text-sm text-gray-900 font-medium ml-1">
                  Họ và tên
                </label>
                <div className="relative group">
                  <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="fullName"
                    type="text"
                    onChange={handleChange}
                    value={form.fullName}
                    placeholder="Nhập họ và tên"
                    className={`w-full pl-12 pr-4 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 placeholder:text-gray-400
                      ${
                        errors.fullName
                          ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                          : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                      }`}
                  />
                </div>
                {errors.fullName && <p className="text-sm text-red-500 pl-1">{errors.fullName}</p>}
              </div>

              {/* Email / SĐT */}
              <div className="space-y-2">
                <label htmlFor="email" className="text-sm text-gray-900 font-medium ml-1">
                  Email hoặc số điện thoại
                </label>
                <div className="relative group">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="email"
                    type="text"
                    onChange={handleChange}
                    value={form.email}
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

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Ngày sinh */}
                <div className="space-y-2">
                  <label htmlFor="dateOfBirth" className="text-sm text-gray-900 font-medium ml-1">
                    Ngày sinh
                  </label>
                  <div className="relative group">
                    <CalendarDays className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                    <input
                      id="dateOfBirth"
                      type="date"
                      onChange={handleChange}
                      value={form.dateOfBirth}
                      className={`w-full pl-12 pr-4 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 placeholder:text-gray-400 [&::-webkit-calendar-picker-indicator]:opacity-0
                        ${
                          errors.dateOfBirth
                            ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                            : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                        }`}
                    />
                  </div>
                  {errors.dateOfBirth && <p className="text-sm text-red-500 pl-1">{errors.dateOfBirth}</p>}
                </div>

                {/* Giới tính */}
                <div className="space-y-2">
                  <label htmlFor="gender" className="text-sm text-gray-900 font-medium ml-1">
                    Giới tính
                  </label>
                  <div className="relative group">
                    <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 pointer-events-none z-10" />
                    <select
                      id="gender"
                      onChange={handleChange}
                      value={form.gender}
                      className={`w-full pl-12 pr-12 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 appearance-none cursor-pointer
                        ${
                          errors.gender
                            ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                            : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                        }
                        ${form.gender === "" ? "text-gray-400" : "text-gray-900"}`}
                    >
                      <option value="">Chọn giới tính</option>
                      <option value="0">Nam</option>
                      <option value="1">Nữ</option>
                      <option value="2">Khác</option>
                    </select>
                    <svg className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 pointer-events-none" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                  {errors.gender && <p className="text-sm text-red-500 pl-1">{errors.gender}</p>}
                </div>
              </div>

              {/* Mật khẩu */}
              <div className="space-y-2">
                <label htmlFor="password" className="text-sm text-gray-900 font-medium ml-1">
                  Mật khẩu
                </label>
                <div className="relative group">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="••••••••"
                    onChange={handleChange}
                    value={form.password}
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

              {/* Xác nhận mật khẩu */}
              <div className="space-y-2">
                <label htmlFor="confirmPassword" className="text-sm text-gray-900 font-medium ml-1">
                  Xác nhận mật khẩu
                </label>
                <div className="relative group">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-aura-dark transition-colors" />
                  <input
                    id="confirmPassword"
                    type={showConfirmPassword ? "text" : "password"}
                    placeholder="••••••••"
                    onChange={handleChange}
                    value={form.confirmPassword}
                    className={`w-full pl-12 pr-12 py-4 bg-white/50 border rounded-2xl outline-none transition-all duration-300 placeholder:text-gray-400
                      ${
                        errors.confirmPassword
                          ? "border-red-500 focus:ring-4 focus:ring-red-500/10 bg-white"
                          : "border-gray-200 focus:border-aura-dark focus:ring-4 focus:ring-gray-900/5 hover:bg-white focus:bg-white"
                      }`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-900 transition-colors p-1"
                  >
                    {showConfirmPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                  </button>
                </div>
                {errors.confirmPassword && <p className="text-sm text-red-500 pl-1">{errors.confirmPassword}</p>}
              </div>

              {/* Đồng ý điều khoản */}
              <div className="space-y-2 pt-2">
                <label className="flex items-start gap-3 cursor-pointer group">
                  <input
                    id="agreeTerms"
                    type="checkbox"
                    onChange={handleChange}
                    checked={form.agreeTerms}
                    className="mt-1 w-5 h-5 rounded-lg border-2 border-gray-300 text-aura-dark focus:ring-2 focus:ring-gray-900/20 cursor-pointer accent-aura-dark"
                  />
                  <span className="text-sm text-gray-500 leading-6 group-hover:text-gray-900 transition-colors">
                    Tôi đồng ý với{" "}
                    <span className="text-aura-dark font-medium hover:underline underline-offset-4">điều khoản sử dụng</span>{" "}
                    và{" "}
                    <span className="text-aura-dark font-medium hover:underline underline-offset-4">chính sách bảo mật</span>
                  </span>
                </label>
                {errors.agreeTerms && <p className="text-sm text-red-500 pl-1">{errors.agreeTerms}</p>}
              </div>

              {errors.general && (
                <div className="text-sm text-red-600 bg-red-50 border border-red-100 px-4 py-3 rounded-xl">
                  {errors.general}
                </div>
              )}

              {/* Button đăng ký */}
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
                  <>Đăng ký tài khoản</>
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

            {/* Social Login */}
            <div className="space-y-4">
              <div className="flex justify-center w-full btn-magnetic">
                <GoogleLogin
                  onSuccess={async (credentialResponse) => {
                    setErrors({});
                    const result = await loginGoogle(credentialResponse.credential, false);
                    if (!result.success) setErrors({ general: result.message });
                  }}
                  onError={() => setErrors({ general: "Đăng nhập Google thất bại." })}
                  useOneTap
                  theme="outline"
                  size="large"
                  shape="pill"
                  width="100%"
                  locale="vi"
                  text="signup_with"
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
                Đã có tài khoản?{" "}
                <Link to="/login" className="font-semibold text-aura-dark hover:underline underline-offset-4">
                  Đăng nhập ngay
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
