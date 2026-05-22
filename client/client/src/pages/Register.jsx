import axios from "axios";
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
export default function App() {
  const navigate = useNavigate();
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

      const response = await axios.post(
        "http://localhost:5267/api/Auth/register",
        payload,
      );

      console.log("✅ Response thành công:", response.data);

      if (response.status === 200) {
        navigate("/onboarding");
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
    <div className="min-h-screen w-full flex">
      {/* Left Visual Section */}
      <div className="hidden lg:flex lg:w-[45%] relative overflow-hidden bg-gradient-to-br from-pink-50 via-purple-50 to-pink-100">
        {/* Background Glow Effects */}
        <div className="absolute top-20 left-20 w-96 h-96 bg-pink-300/30 rounded-full blur-3xl"></div>
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-purple-300/30 rounded-full blur-3xl"></div>

        {/* Main Image */}
        <div className="relative z-10 flex flex-col items-center justify-center w-full px-16">
          <div className="relative mb-12">
            <img
              src="couple.jpg"
              alt="Happy couple"
              className="w-full h-auto rounded-3xl shadow-2xl"
            />
            {/* Floating Match Cards */}
            <div className="absolute -top-6 -right-6 bg-white p-4 rounded-2xl shadow-xl animate-pulse">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-pink-400 to-purple-400"></div>
                <div>
                  <div className="w-16 h-3 bg-gray-200 rounded mb-1"></div>
                  <div className="w-12 h-2 bg-gray-100 rounded"></div>
                </div>
                <Heart className="w-6 h-6 text-pink-500 fill-pink-500" />
              </div>
            </div>

            <div
              className="absolute -bottom-6 -left-6 bg-white p-4 rounded-2xl shadow-xl animate-pulse"
              style={{ animationDelay: "0.5s" }}
            >
              <div className="flex items-center gap-3">
                <Sparkles className="w-6 h-6 text-purple-500" />
                <div className="w-20 h-3 bg-gray-200 rounded"></div>
              </div>
            </div>
          </div>

          {/* Branding Text */}
          <div className="text-center max-w-md">
            <h1 className="text-4xl mb-4 bg-gradient-to-r from-pink-600 to-purple-600 bg-clip-text text-transparent">
              Mỗi kết nối đẹp đều bắt đầu từ một bước đầu tiên
            </h1>
            <p className="text-lg text-gray-600">
              Tham gia Aura Dating để gặp đúng người phù hợp trong không gian an
              toàn và tinh tế.
            </p>
          </div>
        </div>
      </div>

      {/* Right Column - Login Card */}
      <div className="w-full lg:w-[55%] min-h-screen flex items-center justify-center px-6 py-10 bg-white">
        <div className="w-full max-w-md">
          <div className="bg-white/70 backdrop-blur-xl rounded-[32px] shadow-2xl border border-white/60 p-10 space-y-8">
            {/* Mobile Logo */}
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

            {/* Header */}
            <div className="text-center space-y-2">
              <h2 className="text-3xl font-bold text-[#1F2937]">
                Tạo tài khoản mới
              </h2>
              <p className="text-[#6B7280]">
                Bắt đầu hành trình kết nối của bạn chỉ trong vài bước đơn giản.
              </p>
            </div>

            {/* Login Form */}
            <form className="space-y-6" onSubmit={handleSubmit}>
              {/* Họ và tên */}
              <div className="space-y-2">
                <label
                  htmlFor="fullName"
                  className="text-sm text-[#1F2937] font-medium"
                >
                  Họ và tên
                </label>

                <div className="relative group">
                  <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] group-focus-within:text-[#FF5C9A] transition-colors" />

                  <input
                    id="fullName"
                    type="text"
                    onChange={handleChange}
                    value={form.fullName}
                    placeholder="Nhập họ và tên"
                    className={`w-full pl-12 pr-4 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300
      ${
        errors.fullName
          ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
          : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
      }`}
                  />
                </div>

                {errors.fullName && (
                  <p className="text-sm text-red-500 pl-1 animate-pulse">
                    {errors.fullName}
                  </p>
                )}
              </div>

              {/* Email / SĐT */}
              <div className="space-y-2">
                <label
                  htmlFor="email"
                  className="text-sm text-[#1F2937] font-medium"
                >
                  Email hoặc số điện thoại
                </label>

                <div className="relative group">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] group-focus-within:text-[#FF5C9A] transition-colors" />

                  <input
                    id="email"
                    type="text"
                    onChange={handleChange}
                    value={form.email}
                    placeholder="yourname@example.com"
                    className={`w-full pl-12 pr-4 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300
      ${
        errors.email
          ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
          : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
      }`}
                  />
                </div>

                {errors.email && (
                  <p className="text-sm text-red-500 pl-1 animate-pulse">
                    {errors.email}
                  </p>
                )}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Ngày sinh */}
                <div className="space-y-2">
                  <label
                    htmlFor="dateOfBirth"
                    className="text-sm text-[#1F2937] font-medium"
                  >
                    Ngày sinh
                  </label>

                  <div className="relative group">
                    <CalendarDays className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280]" />

                    <input
                      id="dateOfBirth"
                      type="date"
                      onChange={handleChange}
                      value={form.dateOfBirth}
                      className={`w-full pl-12 pr-4 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300 [&::-webkit-calendar-picker-indicator]:opacity-0
        ${
          errors.dateOfBirth
            ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
            : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
        }`}
                    />
                  </div>

                  {errors.dateOfBirth && (
                    <p className="text-sm text-red-500 pl-1 animate-pulse">
                      {errors.dateOfBirth}
                    </p>
                  )}
                </div>

                {/* Giới tính */}
                <div className="space-y-2">
                  <label
                    htmlFor="gender"
                    className="text-sm text-[#1F2937] font-medium"
                  >
                    Giới tính
                  </label>

                  <div className="relative group">
                    <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] pointer-events-none z-10" />

                    <select
                      id="gender"
                      onChange={handleChange}
                      value={form.gender}
                      className={`w-full pl-12 pr-12 py-4 bg-white/90 border-2 rounded-2xl outline-none transition-all duration-300 appearance-none text-[#1F2937] cursor-pointer
    ${
      errors.gender
        ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
        : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10 hover:border-[#FF5C9A]/40"
    }
    ${form.gender === "" ? "text-[#9CA3AF]" : "text-[#1F2937]"}`}
                    >
                      <option value="">Chọn giới tính</option>
                      <option value="0">Nam</option>
                      <option value="1">Nữ</option>
                      <option value="2">Khác</option>
                    </select>

                    {/* Custom Arrow */}
                    <svg
                      className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] pointer-events-none"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M19 9l-7 7-7-7"
                      />
                    </svg>
                  </div>

                  {errors.gender && (
                    <p className="text-sm text-red-500 pl-1 animate-pulse">
                      {errors.gender}
                    </p>
                  )}
                </div>
              </div>

              {/* Mật khẩu */}
              <div className="space-y-2">
                <label
                  htmlFor="password"
                  className="text-sm text-[#1F2937] font-medium"
                >
                  Mật khẩu
                </label>

                <div className="relative group">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] group-focus-within:text-[#FF5C9A] transition-colors" />

                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="••••••••"
                    onChange={handleChange}
                    value={form.password}
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
                    {showPassword ? (
                      <EyeOff className="w-5 h-5" />
                    ) : (
                      <Eye className="w-5 h-5" />
                    )}
                  </button>
                </div>

                {errors.password && (
                  <p className="text-sm text-red-500 pl-1 animate-pulse">
                    {errors.password}
                  </p>
                )}
              </div>

              {/* Xác nhận mật khẩu */}
              <div className="space-y-2">
                <label
                  htmlFor="confirmPassword"
                  className="text-sm text-[#1F2937] font-medium"
                >
                  Xác nhận mật khẩu
                </label>

                <div className="relative group">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#6B7280] group-focus-within:text-[#FF5C9A] transition-colors" />

                  <input
                    id="confirmPassword"
                    type={showConfirmPassword ? "text" : "password"}
                    placeholder="••••••••"
                    onChange={handleChange}
                    value={form.confirmPassword}
                    className={`w-full pl-12 pr-12 py-4 bg-white/80 border-2 rounded-2xl outline-none transition-all duration-300
      ${
        errors.confirmPassword
          ? "border-red-500 focus:ring-4 focus:ring-red-500/10"
          : "border-gray-200 focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10"
      }`}
                  />

                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
                  >
                    {showConfirmPassword ? (
                      <EyeOff className="w-5 h-5" />
                    ) : (
                      <Eye className="w-5 h-5" />
                    )}
                  </button>
                </div>

                {errors.confirmPassword && (
                  <p className="text-sm text-red-500 pl-1 animate-pulse">
                    {errors.confirmPassword}
                  </p>
                )}
              </div>
              {/* Đồng ý điều khoản */}
              <div className="space-y-2">
                <label className="flex items-start gap-3 cursor-pointer select-none">
                  <input
                    id="agreeTerms"
                    type="checkbox"
                    onChange={handleChange}
                    checked={form.agreeTerms}
                    className={`mt-1 w-5 h-5 rounded border-2 transition-all duration-300
      ${
        errors.agreeTerms
          ? "border-red-500 text-red-500"
          : "border-gray-300 text-[#FF5C9A]"
      }`}
                  />

                  <span className="text-sm text-[#6B7280] leading-6">
                    Tôi đồng ý với{" "}
                    <span className="text-[#FF5C9A] font-medium hover:underline">
                      điều khoản sử dụng
                    </span>{" "}
                    và{" "}
                    <span className="text-[#FF5C9A] font-medium hover:underline">
                      chính sách bảo mật
                    </span>
                  </span>
                </label>

                {errors.agreeTerms && (
                  <p className="text-sm text-red-500 pl-1 animate-pulse">
                    {errors.agreeTerms}
                  </p>
                )}
              </div>

              {/* Button đăng ký */}
              <button
                type="submit"
                disabled={loading}
                className={`w-full py-4 rounded-2xl font-medium text-white flex items-center justify-center gap-2 transition-all duration-300
  ${
    loading
      ? "bg-gray-400 cursor-not-allowed"
      : "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] shadow-lg shadow-pink-500/30 hover:shadow-xl hover:shadow-pink-500/40 hover:scale-[1.02] active:scale-[0.98]"
  }`}
              >
                {loading ? (
                  <>
                    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Đang đăng ký...
                  </>
                ) : (
                  <>
                    <Heart className="w-5 h-5 fill-white" />
                    Đăng ký tài khoản
                  </>
                )}
              </button>
            </form>

            {/* Divider */}
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-4 bg-white/70 text-[#6B7280]">
                  Hoặc tiếp tục với
                </span>
              </div>
            </div>

            {/* Social Login */}
            <div className="grid grid-cols-2 gap-4">
              <button
                type="button"
                className="py-3 px-4 bg-white border-2 border-gray-200 rounded-xl hover:border-[#FF5C9A] hover:shadow-md transition-all duration-300 flex items-center justify-center gap-2 font-medium text-[#1F2937]"
              >
                <svg className="w-5 h-5" viewBox="0 0 24 24">
                  <path
                    fill="#4285F4"
                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                  />
                  <path
                    fill="#34A853"
                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  />
                  <path
                    fill="#FBBC05"
                    d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                  />
                  <path
                    fill="#EA4335"
                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                  />
                </svg>
                <span className="text-sm">Google</span>
              </button>
              <button
                type="button"
                className="py-3 px-4 bg-white border-2 border-gray-200 rounded-xl hover:border-[#C8B6FF] hover:shadow-md transition-all duration-300 flex items-center justify-center gap-2 font-medium text-[#1F2937]"
              >
                <svg className="w-5 h-5" viewBox="0 0 24 24" fill="#1877F2">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
                </svg>
                <span className="text-sm">Facebook</span>
              </button>
            </div>

            {/* Footer */}
            <div className="text-center space-y-4 pt-4">
              <p className="text-sm text-[#6B7280]">
                Chưa có tài khoản?{" "}
                <Link
                  to="/login"
                  className="font-medium text-[#FF5C9A] hover:text-[#C8B6FF] transition-colors"
                >
                  Đăng nhập ngay
                </Link>{" "}
              </p>

              {/* Trust Badges */}
              <div className="pt-4 space-y-2 border-t border-gray-200">
                <div className="flex items-center justify-center gap-2 text-xs text-[#6B7280]">
                  <Lock className="w-4 h-4" />
                  <span>Thông tin của bạn luôn được bảo mật tuyệt đối</span>
                </div>
                <div className="flex items-center justify-center gap-2 text-xs text-[#6B7280]">
                  <Heart className="w-4 h-4 fill-current" />
                  <span>
                    Hơn 1.000.000 người đã bắt đầu kết nối tại Aura Dating
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
