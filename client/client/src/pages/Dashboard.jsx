import {
  Bell,
  ChevronRight,
  Eye,
  Heart,
  MapPin,
  MessageCircle,
  Sparkles,
  TrendingUp,
} from "lucide-react";
export default function App() {
  const menuItems = [
    { label: "Dành cho bạn", active: true },
    { label: "Hồ sơ của tôi", active: false },
    { label: "Lượt thích", active: false },
    { label: "Tương hợp cao", active: false },
    { label: "Tin nhắn", active: false },
    { label: "Cài đặt", active: false },
  ];

  const suggestedMatches = [
    {
      name: "Thu Hà",
      age: 25,
      city: "Hà Nội",
      compatibility: 92,
      online: true,
      newMember: false,
      verified: true,
      image:
        "https://images.unsplash.com/photo-1758600587853-3d2138df6b2b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    },
    {
      name: "Minh Tâm",
      age: 27,
      city: "TP.HCM",
      compatibility: 88,
      online: false,
      newMember: true,
      verified: false,
      image:
        "https://images.unsplash.com/photo-1618593706014-06782cd3bb3b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    },
    {
      name: "Khánh Linh",
      age: 24,
      city: "Đà Nẵng",
      compatibility: 85,
      online: true,
      newMember: false,
      verified: true,
      image:
        "https://images.unsplash.com/photo-1758600587839-56ba05596c69?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    },
    {
      name: "Phương Anh",
      age: 26,
      city: "Hà Nội",
      compatibility: 90,
      online: true,
      newMember: false,
      verified: true,
      image:
        "https://images.unsplash.com/photo-1758691737605-69a0e78bd193?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    },
  ];

  const recentLikes = [
    {
      name: "Bảo Anh",
      age: 23,
      image:
        "https://images.unsplash.com/photo-1770058443069-e384cd001e9b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
      time: "5 phút trước",
    },
    {
      name: "Minh Châu",
      age: 25,
      image:
        "https://images.unsplash.com/photo-1769636929130-56648d6e9c6d?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
      time: "12 phút trước",
    },
    {
      name: "Hải Yến",
      age: 24,
      image:
        "https://images.unsplash.com/photo-1758600587853-3d2138df6b2b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
      time: "1 giờ trước",
    },
  ];

  const recentChats = [
    {
      name: "Thu Hà",
      message: "Hôm nay bạn thế nào? 😊",
      time: "2 phút",
      unread: 2,
      online: true,
      image:
        "https://images.unsplash.com/photo-1758600587853-3d2138df6b2b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
    },
    {
      name: "Khánh Linh",
      message: "Cảm ơn bạn đã chia sẻ!",
      time: "15 phút",
      unread: 0,
      online: true,
      image:
        "https://images.unsplash.com/photo-1758600587839-56ba05596c69?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
    },
    {
      name: "Minh Tâm",
      message: "Mình rất vui được nói chuyện với...",
      time: "1 giờ",
      unread: 0,
      online: false,
      image:
        "https://images.unsplash.com/photo-1618593706014-06782cd3bb3b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=200",
    },
  ];

  return (
    <div className="min-h-screen bg-white">
      {/* Ambient Background */}
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
        <div className="absolute top-0 right-0 w-[800px] h-[800px] bg-gradient-to-br from-[#FF5C9A]/10 via-[#C8B6FF]/10 to-transparent rounded-full blur-3xl"></div>
        <div className="absolute bottom-0 left-0 w-[600px] h-[600px] bg-gradient-to-tr from-[#C8B6FF]/10 via-[#FF5C9A]/5 to-transparent rounded-full blur-3xl"></div>
      </div>

      {/* Header */}
      <header className="sticky top-0 z-50 bg-white/80 backdrop-blur-xl border-b border-gray-100">
        <div className="max-w-[1440px] mx-auto px-8 py-4 flex items-center justify-between">
          <div className="flex items-center gap-12">
            <div className="flex items-center gap-2">
              <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center">
                <Heart className="w-6 h-6 text-white fill-white" />
              </div>
              <span className="text-xl font-semibold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                Aura Dating
              </span>
            </div>
            <nav className="flex items-center gap-8">
              <a
                href="#"
                className="text-[#1F2937] font-medium hover:text-[#FF5C9A] transition-colors"
              >
                Trang chủ
              </a>
              <a
                href="#"
                className="text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
              >
                Khám phá
              </a>
              <a
                href="#"
                className="text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
              >
                Ghép đôi
              </a>
              <a
                href="#"
                className="text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
              >
                Tin nhắn
              </a>
              <a
                href="#"
                className="text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
              >
                Yêu thích
              </a>
            </nav>
          </div>
          <div className="flex items-center gap-4">
            <button className="relative p-2.5 hover:bg-gray-50 rounded-xl transition-colors">
              <Bell className="w-5 h-5 text-[#6B7280]" />
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-[#FF5C9A] rounded-full"></span>
            </button>
            <div className="flex items-center gap-3 pl-4 border-l border-gray-200">
              <div className="relative">
                <img
                  src="https://images.unsplash.com/photo-1758600587853-3d2138df6b2b?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=100"
                  alt="Avatar"
                  className="w-10 h-10 rounded-full object-cover ring-2 ring-[#FF5C9A]/20"
                />
                <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white"></span>
              </div>
              <div className="text-sm">
                <div className="font-medium text-[#1F2937]">Minh Anh</div>
                <div className="text-[#6B7280] text-xs">Premium</div>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="max-w-[1440px] mx-auto px-8 py-8">
        <div className="grid grid-cols-12 gap-6">
          {/* Left Sidebar */}
          <aside className="col-span-2">
            <div className="sticky top-24 space-y-4">
              {/* Menu */}
              <nav className="bg-white rounded-3xl border border-gray-100 p-3 shadow-sm">
                {menuItems.map((item, index) => (
                  <a
                    key={index}
                    href="#"
                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                      item.active
                        ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white shadow-lg shadow-[#FF5C9A]/20"
                        : "text-[#6B7280] hover:bg-gray-50"
                    }`}
                  >
                    <span className="text-sm font-medium">{item.label}</span>
                  </a>
                ))}
              </nav>

              {/* Progress Card */}
              <div className="bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 rounded-3xl border border-[#FF5C9A]/20 p-6">
                <div className="flex items-center gap-2 mb-3">
                  <Heart className="w-5 h-5 text-[#FF5C9A] fill-[#FF5C9A]" />
                  <span className="text-sm font-medium text-[#1F2937]">
                    Hoàn thiện hồ sơ
                  </span>
                </div>
                <div className="mb-2">
                  <div className="h-2 bg-white/50 rounded-full overflow-hidden">
                    <div className="h-full w-[85%] bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full"></div>
                  </div>
                </div>
                <div className="text-2xl font-semibold text-[#FF5C9A]">85%</div>
                <p className="text-xs text-[#6B7280] mt-2">
                  Thêm 3 ảnh để đạt 100%
                </p>
              </div>
            </div>
          </aside>

          {/* Main Content */}
          <main className="col-span-7">
            <div className="space-y-6">
              {/* Welcome Hero Card */}
              <div className="relative overflow-hidden bg-gradient-to-br from-[#FF5C9A]/20 via-[#C8B6FF]/15 to-white rounded-[28px] border border-[#FF5C9A]/20 p-8 shadow-xl">
                <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-[#FF5C9A]/20 to-transparent rounded-full blur-3xl"></div>
                <div className="relative">
                  <h1 className="text-3xl font-semibold text-[#1F2937] mb-2">
                    Chào mừng trở lại, Minh Anh 💖
                  </h1>
                  <p className="text-[#6B7280] mb-6">
                    Hôm nay có{" "}
                    <span className="font-semibold text-[#FF5C9A]">
                      18 người phù hợp mới
                    </span>{" "}
                    dành cho bạn.
                  </p>
                  <div className="flex gap-4">
                    <button className="px-6 py-3 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-2xl font-medium shadow-lg shadow-[#FF5C9A]/30 hover:shadow-xl hover:shadow-[#FF5C9A]/40 transition-all hover:scale-105">
                      Khám phá ngay
                    </button>
                    <button className="px-6 py-3 bg-white/80 backdrop-blur-sm text-[#FF5C9A] rounded-2xl font-medium border border-[#FF5C9A]/30 hover:bg-white hover:border-[#FF5C9A] transition-all">
                      Xem lượt thích
                    </button>
                  </div>
                </div>
              </div>

              {/* Suggested Matches */}
              <section>
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h2 className="text-2xl font-semibold text-[#1F2937]">
                      Gợi ý phù hợp hôm nay
                    </h2>
                    <p className="text-sm text-[#6B7280] mt-1">
                      Những người có mức tương hợp cao đang chờ kết nối
                    </p>
                  </div>
                  <button className="text-[#FF5C9A] font-medium flex items-center gap-1 hover:gap-2 transition-all">
                    Xem tất cả <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  {suggestedMatches.map((match, index) => (
                    <div
                      key={index}
                      className="group bg-white rounded-[28px] border border-gray-100 overflow-hidden shadow-sm hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 cursor-pointer"
                    >
                      <div className="relative aspect-[3/4] overflow-hidden">
                        <img
                          src={match.image}
                          alt={match.name}
                          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                        />
                        {match.online && (
                          <div className="absolute top-4 right-4 px-3 py-1.5 bg-white/90 backdrop-blur-sm rounded-full flex items-center gap-1.5">
                            <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
                            <span className="text-xs font-medium text-[#1F2937]">
                              Online
                            </span>
                          </div>
                        )}
                        {match.newMember && (
                          <div className="absolute top-4 left-4 px-3 py-1.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full">
                            <span className="text-xs font-medium text-white">
                              Mới tham gia
                            </span>
                          </div>
                        )}
                        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-4">
                          <div className="flex items-center justify-between text-white">
                            <div>
                              <div className="flex items-center gap-2">
                                <h3 className="font-semibold text-lg">
                                  {match.name}, {match.age}
                                </h3>
                                {match.verified && (
                                  <div className="w-5 h-5 bg-[#C8B6FF] rounded-full flex items-center justify-center">
                                    <Sparkles className="w-3 h-3 text-white" />
                                  </div>
                                )}
                              </div>
                              <div className="flex items-center gap-1 text-sm text-white/80 mt-1">
                                <MapPin className="w-3 h-3" />
                                <span>{match.city}</span>
                              </div>
                            </div>
                            <div className="text-center">
                              <div className="text-2xl font-bold">
                                {match.compatibility}%
                              </div>
                              <div className="text-xs text-white/70">
                                Phù hợp
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div className="p-4 flex gap-2">
                        <button className="flex-1 py-2.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-xl font-medium hover:shadow-lg hover:shadow-[#FF5C9A]/30 transition-all">
                          <Heart className="w-4 h-4 inline mr-1" />
                          Thích
                        </button>
                        <button className="flex-1 py-2.5 bg-gray-50 text-[#1F2937] rounded-xl font-medium hover:bg-gray-100 transition-all">
                          Xem hồ sơ
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </section>

              {/* Recent Likes */}
              <section>
                <h2 className="text-2xl font-semibold text-[#1F2937] mb-4">
                  Người vừa thích bạn
                </h2>
                <div className="bg-white rounded-[28px] border border-gray-100 p-6 shadow-sm">
                  <div className="flex gap-4 overflow-x-auto">
                    {recentLikes.map((like, index) => (
                      <div
                        key={index}
                        className="flex-shrink-0 group cursor-pointer"
                      >
                        <div className="relative">
                          <img
                            src={like.image}
                            alt={like.name}
                            className="w-24 h-24 rounded-2xl object-cover group-hover:scale-105 transition-transform ring-4 ring-[#FF5C9A]/20"
                          />
                          <div className="absolute -bottom-2 -right-2 w-8 h-8 bg-[#FF5C9A] rounded-full flex items-center justify-center shadow-lg">
                            <Heart className="w-4 h-4 text-white fill-white" />
                          </div>
                        </div>
                        <div className="mt-3 text-center">
                          <div className="font-medium text-[#1F2937] text-sm">
                            {like.name}, {like.age}
                          </div>
                          <div className="text-xs text-[#6B7280] mt-0.5">
                            {like.time}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </section>

              {/* Recent Chats */}
              <section>
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-2xl font-semibold text-[#1F2937]">
                    Trò chuyện gần đây
                  </h2>
                  <button className="text-[#FF5C9A] font-medium flex items-center gap-1 hover:gap-2 transition-all">
                    Xem tất cả <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
                <div className="bg-white rounded-[28px] border border-gray-100 shadow-sm overflow-hidden">
                  {recentChats.map((chat, index) => (
                    <div
                      key={index}
                      className={`flex items-center gap-4 p-5 hover:bg-gray-50 cursor-pointer transition-colors ${
                        index !== recentChats.length - 1
                          ? "border-b border-gray-100"
                          : ""
                      }`}
                    >
                      <div className="relative flex-shrink-0">
                        <img
                          src={chat.image}
                          alt={chat.name}
                          className="w-14 h-14 rounded-full object-cover"
                        />
                        {chat.online && (
                          <span className="absolute bottom-0 right-0 w-4 h-4 bg-green-400 rounded-full border-2 border-white"></span>
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between mb-1">
                          <h3 className="font-medium text-[#1F2937]">
                            {chat.name}
                          </h3>
                          <span className="text-xs text-[#6B7280]">
                            {chat.time}
                          </span>
                        </div>
                        <p className="text-sm text-[#6B7280] truncate">
                          {chat.message}
                        </p>
                      </div>
                      {chat.unread > 0 && (
                        <div className="flex-shrink-0 w-6 h-6 bg-[#FF5C9A] rounded-full flex items-center justify-center">
                          <span className="text-xs font-medium text-white">
                            {chat.unread}
                          </span>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </section>
            </div>
          </main>

          {/* Right Sidebar */}
          <aside className="col-span-3">
            <div className="sticky top-24 space-y-6">
              {/* Connection Stats */}
              <div className="bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-[28px] p-6 text-white shadow-xl">
                <h3 className="font-semibold mb-4 flex items-center gap-2">
                  <TrendingUp className="w-5 h-5" />
                  Chỉ số kết nối
                </h3>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Eye className="w-4 h-4 text-white/80" />
                      <span className="text-sm text-white/90">
                        Lượt xem hồ sơ
                      </span>
                    </div>
                    <span className="text-2xl font-bold">24</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Heart className="w-4 h-4 text-white/80" />
                      <span className="text-sm text-white/90">
                        Lượt thích mới
                      </span>
                    </div>
                    <span className="text-2xl font-bold">12</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <MessageCircle className="w-4 h-4 text-white/80" />
                      <span className="text-sm text-white/90">
                        Cuộc trò chuyện mới
                      </span>
                    </div>
                    <span className="text-2xl font-bold">3</span>
                  </div>
                </div>
              </div>

              {/* Featured Profile */}
              <div className="bg-white rounded-[28px] border border-gray-100 overflow-hidden shadow-sm hover:shadow-lg transition-shadow">
                <div className="relative aspect-[4/5]">
                  <img
                    src="https://images.unsplash.com/photo-1758534063829-a72058381e21?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400"
                    alt="Featured"
                    className="w-full h-full object-cover"
                  />
                  <div className="absolute top-4 left-4 px-3 py-1.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full">
                    <span className="text-xs font-medium text-white flex items-center gap-1">
                      <Sparkles className="w-3 h-3" />
                      Nổi bật
                    </span>
                  </div>
                </div>
                <div className="p-4">
                  <h3 className="font-semibold text-[#1F2937] mb-1">
                    Hồ sơ nổi bật hôm nay
                  </h3>
                  <p className="text-sm text-[#6B7280] mb-3">
                    Đức Anh, 28 tuổi
                  </p>
                  <button className="w-full py-2.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-xl font-medium hover:shadow-lg transition-all">
                    Xem hồ sơ
                  </button>
                </div>
              </div>

              {/* Connection Tips */}
              <div className="bg-gradient-to-br from-[#C8B6FF]/10 to-[#FF5C9A]/5 rounded-[28px] border border-[#C8B6FF]/20 p-6">
                <div className="flex items-start gap-3">
                  <div className="w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-xl flex items-center justify-center flex-shrink-0">
                    <Sparkles className="w-5 h-5 text-white" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-[#1F2937] mb-2">
                      Mẹo kết nối
                    </h3>
                    <p className="text-sm text-[#6B7280] leading-relaxed">
                      "Ảnh đại diện tự nhiên giúp tăng{" "}
                      <span className="font-semibold text-[#FF5C9A]">
                        40% lượt tương tác
                      </span>
                      ."
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}
