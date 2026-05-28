import {
  Award,
  CheckCircle,
  Heart,
  MessageCircle,
  Shield,
  Sparkles,
  Star,
  Users,
} from "lucide-react";
import { Link } from "react-router-dom";
export default function App() {
  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="sticky top-0 z-50 bg-white/80 backdrop-blur-lg border-b border-gray-100">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className="w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-2xl flex items-center justify-center">
                <Heart className="w-6 h-6 text-white fill-white" />
              </div>
              <span className="text-2xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                Aura Dating
              </span>
            </div>

            <nav className="hidden md:flex items-center gap-8">
              <a
                href="#"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Trang chủ
              </a>
              <a
                href="#features"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Tính năng
              </a>
              <a
                href="#how-it-works"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Cách hoạt động
              </a>
              <a
                href="#testimonials"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Câu chuyện
              </a>
              <a
                href="#pricing"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Bảng giá
              </a>
              <a
                href="#contact"
                className="text-gray-700 hover:text-[#FF5C9A] transition-colors"
              >
                Liên hệ
              </a>
            </nav>

            <div className="flex items-center gap-3">
              <Link to="/login">
                <button className="cursor-pointer bg-white text-[#FF5C9A] px-6 py-3 rounded-full border border-[#FF5C9A]/30 hover:border-[#FF5C9A] transition-colors">
                  Đăng nhập
                </button>
              </Link>
              <Link to="/register">
                <button className="cursor-pointer bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white px-6 py-3 rounded-full">
                  Đăng ký miễn phí
                </button>
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-pink-50 via-purple-50 to-white opacity-60" />
        <div className="max-w-7xl mx-auto px-6 py-24 relative">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
            {/* Left Content */}
            <div className="space-y-8">
              <div className="inline-flex items-center gap-2 bg-white/60 backdrop-blur-sm px-4 py-2 rounded-full border border-pink-200">
                <Sparkles className="w-4 h-4 text-[#FF5C9A]" />
                <span className="text-sm text-gray-700">
                  Nền tảng hẹn hò cao cấp
                </span>
              </div>

              <h1 className="text-6xl font-bold leading-tight">
                Tìm người phù hợp
                <br />
                <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                  dành riêng cho bạn
                </span>
              </h1>

              <p className="text-xl text-gray-600 leading-relaxed">
                Kết nối chân thành trong không gian hiện đại, an toàn và đầy cảm
                hứng. Gặp đúng người dựa trên sở thích, tính cách và năng lượng
                phù hợp.
              </p>

              <div className="flex gap-4">
                <button className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white px-8 py-4 rounded-full hover:shadow-xl hover:scale-105 transition-all text-lg font-semibold">
                  Bắt đầu miễn phí
                </button>
                <button className="bg-white text-gray-800 px-8 py-4 rounded-full border-2 border-gray-200 hover:border-[#FF5C9A] hover:shadow-lg transition-all text-lg font-semibold">
                  Khám phá ngay
                </button>
              </div>

              <div className="flex gap-8 pt-4">
                <div className="flex items-center gap-2">
                  <Users className="w-5 h-5 text-[#FF5C9A]" />
                  <span className="text-gray-700">1.000.000+ lượt kết nối</span>
                </div>
                <div className="flex items-center gap-2">
                  <Star className="w-5 h-5 text-[#FF5C9A] fill-[#FF5C9A]" />
                  <span className="text-gray-700">4.9/5 đánh giá</span>
                </div>
                <div className="flex items-center gap-2">
                  <CheckCircle className="w-5 h-5 text-[#FF5C9A]" />
                  <span className="text-gray-700">100% hồ sơ xác minh</span>
                </div>
              </div>
            </div>

            {/* Right Visual */}
            <div className="relative">
              <div className="relative z-10 space-y-6">
                {/* Main Profile Card */}
                <div className="bg-white rounded-3xl p-6 shadow-2xl border border-gray-100 hover:scale-105 transition-transform">
                  <div className="aspect-[4/5] bg-gradient-to-br from-pink-200 to-purple-200 rounded-2xl mb-4 flex items-center justify-center">
                    <Users className="w-24 h-24 text-white/50" />
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <h3 className="text-2xl font-bold">Minh Anh, 24</h3>
                      <div className="w-3 h-3 bg-green-500 rounded-full" />
                    </div>
                    <p className="text-gray-600">Hà Nội</p>
                    <div className="flex gap-2 flex-wrap pt-2">
                      <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                        Du lịch
                      </span>
                      <span className="px-3 py-1 bg-purple-50 text-[#C8B6FF] rounded-full text-sm">
                        Âm nhạc
                      </span>
                      <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                        Đọc sách
                      </span>
                    </div>
                  </div>
                </div>

                {/* Floating Cards */}
                <div className="absolute -top-12 -right-12 bg-white rounded-2xl p-4 shadow-xl border border-gray-100 animate-float">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full" />
                    <div>
                      <p className="font-semibold">Khôi Nguyên</p>
                      <p className="text-sm text-gray-600">98% phù hợp</p>
                    </div>
                    <Heart className="w-6 h-6 text-[#FF5C9A]" />
                  </div>
                </div>

                <div className="absolute -bottom-8 -left-8 bg-white rounded-2xl p-4 shadow-xl border border-gray-100 animate-float-delayed">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-gradient-to-br from-[#C8B6FF] to-[#FF5C9A] rounded-full" />
                    <div>
                      <p className="font-semibold">Thu Hà</p>
                      <p className="text-sm text-gray-600">95% phù hợp</p>
                    </div>
                    <Heart className="w-6 h-6 text-[#C8B6FF]" />
                  </div>
                </div>
              </div>

              {/* Background Glow */}
              <div className="absolute inset-0 bg-gradient-to-br from-[#FF5C9A]/20 to-[#C8B6FF]/20 blur-3xl -z-10" />
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-24 bg-gray-50">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold mb-4">
              Tính năng{" "}
              <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                nổi bật
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Trải nghiệm hẹn hò thông minh và an toàn
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Feature 1 */}
            <div className="bg-white rounded-3xl p-8 hover:shadow-2xl transition-all group border border-gray-100">
              <div className="w-16 h-16 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-2xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <Sparkles className="w-8 h-8 text-white" />
              </div>
              <h3 className="text-2xl font-bold mb-4">
                Ghép đôi AI thông minh
              </h3>
              <p className="text-gray-600 leading-relaxed">
                Thuật toán AI tiên tiến phân tích tính cách, sở thích và giá trị
                sống để gợi ý những người thực sự phù hợp với bạn.
              </p>
            </div>

            {/* Feature 2 */}
            <div className="bg-white rounded-3xl p-8 hover:shadow-2xl transition-all group border border-gray-100">
              <div className="w-16 h-16 bg-gradient-to-br from-[#C8B6FF] to-[#FF5C9A] rounded-2xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <Shield className="w-8 h-8 text-white" />
              </div>
              <h3 className="text-2xl font-bold mb-4">Xác minh an toàn</h3>
              <p className="text-gray-600 leading-relaxed">
                Mọi hồ sơ đều được xác minh qua nhiều bước kiểm tra để đảm bảo
                môi trường hẹn hò an toàn và chân thực.
              </p>
            </div>

            {/* Feature 3 */}
            <div className="bg-white rounded-3xl p-8 hover:shadow-2xl transition-all group border border-gray-100">
              <div className="w-16 h-16 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-2xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <MessageCircle className="w-8 h-8 text-white" />
              </div>
              <h3 className="text-2xl font-bold mb-4">Trò chuyện tự nhiên</h3>
              <p className="text-gray-600 leading-relaxed">
                Giao diện chat hiện đại với nhiều tính năng thú vị giúp bạn dễ
                dàng bắt đầu và duy trì cuộc trò chuyện.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Members Grid */}
      <section className="py-24">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold mb-4">
              Những người phù hợp{" "}
              <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                đang chờ bạn
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Kết nối ngay với hàng nghìn thành viên tích cực
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {/* Profile Card 1 */}
            <div className="bg-white rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all border border-gray-100 group">
              <div className="aspect-[4/5] bg-gradient-to-br from-pink-300 to-purple-300 relative flex items-center justify-center">
                <Users className="w-20 h-20 text-white/50" />
                <div className="absolute top-4 right-4 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
              </div>
              <div className="p-6 space-y-3">
                <h3 className="text-xl font-bold">Hải Yến, 23</h3>
                <p className="text-gray-600 flex items-center gap-2">
                  <span>📍</span> TP. Hồ Chí Minh
                </p>
                <div className="flex gap-2 flex-wrap">
                  <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                    Yoga
                  </span>
                  <span className="px-3 py-1 bg-purple-50 text-[#C8B6FF] rounded-full text-sm">
                    Cafe
                  </span>
                </div>
              </div>
            </div>

            {/* Profile Card 2 */}
            <div className="bg-white rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all border border-gray-100 group">
              <div className="aspect-[4/5] bg-gradient-to-br from-purple-300 to-pink-300 relative flex items-center justify-center">
                <Users className="w-20 h-20 text-white/50" />
                <div className="absolute top-4 right-4 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
              </div>
              <div className="p-6 space-y-3">
                <h3 className="text-xl font-bold">Tuấn Anh, 26</h3>
                <p className="text-gray-600 flex items-center gap-2">
                  <span>📍</span> Đà Nẵng
                </p>
                <div className="flex gap-2 flex-wrap">
                  <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                    Thể thao
                  </span>
                  <span className="px-3 py-1 bg-purple-50 text-[#C8B6FF] rounded-full text-sm">
                    Nấu ăn
                  </span>
                </div>
              </div>
            </div>

            {/* Profile Card 3 */}
            <div className="bg-white rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all border border-gray-100 group">
              <div className="aspect-[4/5] bg-gradient-to-br from-pink-200 to-purple-400 relative flex items-center justify-center">
                <Users className="w-20 h-20 text-white/50" />
                <div className="absolute top-4 right-4 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
              </div>
              <div className="p-6 space-y-3">
                <h3 className="text-xl font-bold">Lan Hương, 25</h3>
                <p className="text-gray-600 flex items-center gap-2">
                  <span>📍</span> Hà Nội
                </p>
                <div className="flex gap-2 flex-wrap">
                  <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                    Nghệ thuật
                  </span>
                  <span className="px-3 py-1 bg-purple-50 text-[#C8B6FF] rounded-full text-sm">
                    Phim ảnh
                  </span>
                </div>
              </div>
            </div>

            {/* Profile Card 4 */}
            <div className="bg-white rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all border border-gray-100 group">
              <div className="aspect-[4/5] bg-gradient-to-br from-purple-400 to-pink-200 relative flex items-center justify-center">
                <Users className="w-20 h-20 text-white/50" />
                <div className="absolute top-4 right-4 w-3 h-3 bg-green-500 rounded-full border-2 border-white" />
              </div>
              <div className="p-6 space-y-3">
                <h3 className="text-xl font-bold">Minh Khoa, 27</h3>
                <p className="text-gray-600 flex items-center gap-2">
                  <span>📍</span> Cần Thơ
                </p>
                <div className="flex gap-2 flex-wrap">
                  <span className="px-3 py-1 bg-pink-50 text-[#FF5C9A] rounded-full text-sm">
                    Công nghệ
                  </span>
                  <span className="px-3 py-1 bg-purple-50 text-[#C8B6FF] rounded-full text-sm">
                    Game
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section
        id="how-it-works"
        className="py-24 bg-gradient-to-br from-pink-50 to-purple-50"
      >
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold mb-4">
              Cách{" "}
              <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                hoạt động
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Chỉ 3 bước đơn giản để tìm được người phù hợp
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-12 relative">
            {/* Step 1 */}
            <div className="relative">
              <div className="bg-white rounded-3xl p-8 text-center shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
                <div className="w-20 h-20 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full flex items-center justify-center mx-auto mb-6 text-white text-3xl font-bold">
                  1
                </div>
                <h3 className="text-2xl font-bold mb-4">Tạo hồ sơ</h3>
                <p className="text-gray-600 leading-relaxed">
                  Hoàn thiện hồ sơ của bạn với ảnh đẹp, sở thích và điều bạn
                  đang tìm kiếm. Chỉ mất 5 phút!
                </p>
              </div>
            </div>

            {/* Step 2 */}
            <div className="relative">
              <div className="bg-white rounded-3xl p-8 text-center shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
                <div className="w-20 h-20 bg-gradient-to-br from-[#C8B6FF] to-[#FF5C9A] rounded-full flex items-center justify-center mx-auto mb-6 text-white text-3xl font-bold">
                  2
                </div>
                <h3 className="text-2xl font-bold mb-4">Nhận gợi ý phù hợp</h3>
                <p className="text-gray-600 leading-relaxed">
                  AI của chúng tôi sẽ phân tích và gợi ý những người có chung sở
                  thích, tính cách với bạn.
                </p>
              </div>
            </div>

            {/* Step 3 */}
            <div className="relative">
              <div className="bg-white rounded-3xl p-8 text-center shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
                <div className="w-20 h-20 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full flex items-center justify-center mx-auto mb-6 text-white text-3xl font-bold">
                  3
                </div>
                <h3 className="text-2xl font-bold mb-4">Bắt đầu trò chuyện</h3>
                <p className="text-gray-600 leading-relaxed">
                  Khi cả hai đều quan tâm, hãy bắt đầu trò chuyện và tìm hiểu
                  nhau thêm!
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section id="testimonials" className="py-24">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold mb-4">
              Câu chuyện{" "}
              <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                thành công
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Những kết nối đẹp từ cộng đồng Aura Dating
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Testimonial 1 */}
            <div className="bg-white rounded-3xl p-8 shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
              <div className="flex gap-1 mb-4">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className="w-5 h-5 text-[#FF5C9A] fill-[#FF5C9A]"
                  />
                ))}
              </div>
              <p className="text-gray-700 mb-6 leading-relaxed italic">
                "Hẹn hò lần đầu tiên mình thấy tinh tế và nghiêm túc đến vậy. Đã
                gặp được người bạn trai tuyệt vời sau 2 tuần sử dụng!"
              </p>
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full" />
                <div>
                  <p className="font-bold">Phương Anh</p>
                  <p className="text-sm text-gray-600">24 tuổi, Hà Nội</p>
                </div>
              </div>
            </div>

            {/* Testimonial 2 */}
            <div className="bg-white rounded-3xl p-8 shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
              <div className="flex gap-1 mb-4">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className="w-5 h-5 text-[#FF5C9A] fill-[#FF5C9A]"
                  />
                ))}
              </div>
              <p className="text-gray-700 mb-6 leading-relaxed italic">
                "Thuật toán ghép đôi rất chính xác, những người được gợi ý đều
                phù hợp với tính cách và sở thích của mình."
              </p>
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 bg-gradient-to-br from-[#C8B6FF] to-[#FF5C9A] rounded-full" />
                <div>
                  <p className="font-bold">Hoàng Minh</p>
                  <p className="text-sm text-gray-600">27 tuổi, TP.HCM</p>
                </div>
              </div>
            </div>

            {/* Testimonial 3 */}
            <div className="bg-white rounded-3xl p-8 shadow-lg border border-gray-100 hover:shadow-2xl transition-all">
              <div className="flex gap-1 mb-4">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className="w-5 h-5 text-[#FF5C9A] fill-[#FF5C9A]"
                  />
                ))}
              </div>
              <p className="text-gray-700 mb-6 leading-relaxed italic">
                "Giao diện đẹp, dễ sử dụng, và quan trọng nhất là cộng đồng
                thành viên chất lượng cao. Rất đáng để thử!"
              </p>
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full" />
                <div>
                  <p className="font-bold">Thu Hằng</p>
                  <p className="text-sm text-gray-600">25 tuổi, Đà Nẵng</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Pricing */}
      <section id="pricing" className="py-24 bg-gray-50">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-bold mb-4">
              Bảng giá{" "}
              <span className="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                đơn giản
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Chọn gói phù hợp với nhu cầu của bạn
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Free Plan */}
            <div className="bg-white rounded-3xl p-8 shadow-lg border-2 border-gray-200 hover:shadow-2xl transition-all">
              <h3 className="text-2xl font-bold mb-2">Miễn phí</h3>
              <div className="mb-6">
                <span className="text-5xl font-bold">0đ</span>
                <span className="text-gray-600">/tháng</span>
              </div>
              <ul className="space-y-4 mb-8">
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5 text-[#FF5C9A]" />
                  <span>10 lượt thích mỗi ngày</span>
                </li>
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Ghép đôi cơ bản</span>
                </li>
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Trò chuyện không giới hạn</span>
                </li>
              </ul>
              <button className="w-full bg-gray-100 text-gray-800 py-4 rounded-full font-semibold hover:bg-gray-200 transition-colors">
                Dùng thử miễn phí
              </button>
            </div>

            {/* Premium Plan */}
            <div className="bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-3xl p-8 shadow-2xl border-2 border-transparent transform scale-105 relative">
              <div className="absolute -top-4 left-1/2 -translate-x-1/2 bg-white px-4 py-1 rounded-full">
                <span className="text-sm font-bold text-[#FF5C9A]">
                  Phổ biến nhất
                </span>
              </div>
              <h3 className="text-2xl font-bold mb-2 text-white">Premium</h3>
              <div className="mb-6 text-white">
                <span className="text-5xl font-bold">199.000đ</span>
                <span className="text-white/80">/tháng</span>
              </div>
              <ul className="space-y-4 mb-8 text-white">
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>Thích không giới hạn</span>
                </li>
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>Xem ai đã thích bạn</span>
                </li>
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>Ưu tiên hiển thị hồ sơ</span>
                </li>
                <li className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>AI ghép đôi nâng cao</span>
                </li>
              </ul>
              <button className="w-full bg-white text-[#FF5C9A] py-4 rounded-full font-semibold hover:shadow-lg transition-all">
                Nâng cấp ngay
              </button>
            </div>

            {/* VIP Plan */}
            <div className="bg-white rounded-3xl p-8 shadow-lg border-2 border-gray-200 hover:shadow-2xl transition-all">
              <h3 className="text-2xl font-bold mb-2">VIP</h3>
              <div className="mb-6">
                <span className="text-5xl font-bold">399.000đ</span>
                <span className="text-gray-600">/tháng</span>
              </div>
              <ul className="space-y-4 mb-8">
                <li className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Tất cả tính năng Premium</span>
                </li>
                <li className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Huy hiệu VIP nổi bật</span>
                </li>
                <li className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Hỗ trợ ưu tiên 24/7</span>
                </li>
                <li className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-[#FF5C9A]" />
                  <span>Ưu đãi độc quyền</span>
                </li>
              </ul>
              <button className="w-full bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white py-4 rounded-full font-semibold hover:shadow-lg transition-all">
                Trở thành VIP
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* Final CTA */}
      <section className="py-32 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF]" />
        <div className="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48ZyBmaWxsPSJub25lIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiPjxwYXRoIGQ9Ik0zNiAxOGMzLjMxNCAwIDYgMi42ODYgNiA2cy0yLjY4NiA2LTYgNi02LTIuNjg2LTYtNiAyLjY4Ni02IDYtNnoiIGZpbGw9IiNmZmYiIGZpbGwtb3BhY2l0eT0iLjA1Ii8+PC9nPjwvc3ZnPg==')] opacity-20" />

        <div className="max-w-4xl mx-auto px-6 text-center relative z-10">
          <h2 className="text-6xl font-bold text-white mb-6">
            Bạn chỉ cách một kết nối đẹp
            <br />
            vài cú nhấp chuột
          </h2>
          <p className="text-xl text-white/90 mb-12 max-w-2xl mx-auto">
            Hàng nghìn người đang chờ kết nối với bạn. Bắt đầu hành trình tìm
            kiếm tình yêu ngay hôm nay!
          </p>
          <button className="bg-white text-[#FF5C9A] px-12 py-6 rounded-full text-xl font-bold hover:shadow-2xl hover:scale-105 transition-all inline-flex items-center gap-3">
            <Heart className="w-6 h-6" />
            Tạo tài khoản miễn phí
          </button>

          <div className="mt-12 flex gap-12 justify-center text-white">
            <div>
              <div className="text-4xl font-bold">1M+</div>
              <div className="text-white/80">Thành viên</div>
            </div>
            <div>
              <div className="text-4xl font-bold">500K+</div>
              <div className="text-white/80">Kết nối mỗi ngày</div>
            </div>
            <div>
              <div className="text-4xl font-bold">4.9/5</div>
              <div className="text-white/80">Đánh giá</div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer id="contact" className="bg-gray-900 text-white py-16">
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12">
            {/* Brand */}
            <div>
              <div className="flex items-center gap-2 mb-4">
                <div className="w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-2xl flex items-center justify-center">
                  <Heart className="w-6 h-6 text-white fill-white" />
                </div>
                <span className="text-2xl font-bold">Aura Dating</span>
              </div>
              <p className="text-gray-400">
                Nền tảng hẹn hò cao cấp dành cho người trẻ hiện đại
              </p>
            </div>

            {/* Giới thiệu */}
            <div>
              <h4 className="font-bold mb-4">Giới thiệu</h4>
              <ul className="space-y-2 text-gray-400">
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Về chúng tôi
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Tuyển dụng
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Blog
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Báo chí
                  </a>
                </li>
              </ul>
            </div>

            {/* Hỗ trợ */}
            <div>
              <h4 className="font-bold mb-4">Hỗ trợ</h4>
              <ul className="space-y-2 text-gray-400">
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Trung tâm trợ giúp
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    An toàn
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Hướng dẫn
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Liên hệ
                  </a>
                </li>
              </ul>
            </div>

            {/* Pháp lý */}
            <div>
              <h4 className="font-bold mb-4">Pháp lý</h4>
              <ul className="space-y-2 text-gray-400">
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Điều khoản sử dụng
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Chính sách bảo mật
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Cookie
                  </a>
                </li>
                <li>
                  <a
                    href="#"
                    className="hover:text-[#FF5C9A] transition-colors"
                  >
                    Bản quyền
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div className="border-t border-gray-800 pt-8">
            <div className="flex flex-col md:flex-row justify-between items-center gap-4">
              <p className="text-gray-400">
                © 2026 Aura Dating. All rights reserved.
              </p>
              <div className="flex gap-6">
                <a
                  href="#"
                  className="text-gray-400 hover:text-[#FF5C9A] transition-colors"
                >
                  Facebook
                </a>
                <a
                  href="#"
                  className="text-gray-400 hover:text-[#FF5C9A] transition-colors"
                >
                  Instagram
                </a>
                <a
                  href="#"
                  className="text-gray-400 hover:text-[#FF5C9A] transition-colors"
                >
                  Twitter
                </a>
                <a
                  href="#"
                  className="text-gray-400 hover:text-[#FF5C9A] transition-colors"
                >
                  LinkedIn
                </a>
              </div>
            </div>
          </div>
        </div>
      </footer>

      <style>{`
        @keyframes float {
          0%, 100% { transform: translateY(0px); }
          50% { transform: translateY(-20px); }
        }

        @keyframes float-delayed {
          0%, 100% { transform: translateY(0px); }
          50% { transform: translateY(-15px); }
        }

        .animate-float {
          animation: float 3s ease-in-out infinite;
        }

        .animate-float-delayed {
          animation: float-delayed 3s ease-in-out infinite 1.5s;
        }
      `}</style>
    </div>
  );
}
