import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Bell,
  ChevronRight,
  Eye,
  Heart,
  LogOut,
  MapPin,
  MessageCircle,
  Sparkles,
  TrendingUp,
  X
} from "lucide-react";
import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";
import * as signalR from "@microsoft/signalr";

function calculateAge(dateOfBirth) {
  if (!dateOfBirth) return null;
  const birth = new Date(dateOfBirth);
  if (Number.isNaN(birth.getTime())) return null;
  return Math.max(0, Math.floor((Date.now() - birth.getTime()) / (365.25 * 24 * 60 * 60 * 1000)));
}

export default function Dashboard() {
  const navigate = useNavigate();
  const { logout, user } = useAuth();
  const [profile, setProfile] = useState(null);
  const [discoverUsers, setDiscoverUsers] = useState([]);
  const [matches, setMatches] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Notifications
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [showNotifDropdown, setShowNotifDropdown] = useState(false);
  const [toastNotif, setToastNotif] = useState(null);

  useEffect(() => {
    let ignore = false;
    async function loadData() {
      try {
        setLoading(true);
        setError("");
        const [profileRes, discoverRes, matchesRes, notifRes, statsRes] = await Promise.all([
          api.get("/User/profile"),
          api.get("/User/discover?page=1&pageSize=4"),
          api.get("/Match"),
          api.get("/Notification"),
          api.get("/User/stats"),
        ]);
        if (ignore) return;
        setProfile(profileRes.data);
        setDiscoverUsers(discoverRes.data?.data || []);
        setMatches(Array.isArray(matchesRes.data) ? matchesRes.data : []);
        setNotifications(notifRes.data?.notifications || []);
        setUnreadCount(notifRes.data?.unreadCount || 0);
        setStats(statsRes.data);
      } catch (err) {
        if (ignore) return;
        if (err.response?.status === 401) {
          navigate("/login");
          return;
        }
        setError("Không thể tải dữ liệu từ server. Vui lòng thử lại!");
      } finally {
        if (!ignore) setLoading(false);
      }
    }

    loadData();

    // SignalR cho Notifications
    const token = localStorage.getItem("token");
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5032/chatHub?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.start().catch((err) => console.error("SignalR Connection Error: ", err));

    connection.on("ReceiveNotification", (notif) => {
      setNotifications((prev) => [notif, ...prev]);
      setUnreadCount((prev) => prev + 1);
      
      // Hiển thị toast realtime
      setToastNotif(notif);
      setTimeout(() => setToastNotif(null), 5000);
    });

    return () => {
      ignore = true;
      connection.stop();
    };
  }, [navigate]);

  // Đánh dấu đã đọc
  const markAsRead = async (id) => {
    try {
      await api.put(`/Notification/${id}/read`);
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (e) {
      console.error(e);
    }
  };

  const markAllAsRead = async () => {
    try {
      await api.put(`/Notification/read-all`);
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (e) {
      console.error(e);
    }
  };

  const menuItems = [
    { label: "Dành cho bạn", active: true, onClick: null },
    { label: "Hồ sơ của tôi", active: false, onClick: () => navigate("/profile") },
    { label: "Lượt thích", active: false, onClick: null },
    { label: "Tương hợp cao", active: false, onClick: null },
    { label: "Tin nhắn", active: false, onClick: () => navigate("/matches") },
    { label: "Cài đặt", active: false, onClick: () => navigate("/settings") },
    ...(user?.role === 1 ? [{ label: "Quản trị viên", active: false, onClick: () => navigate("/admin") }] : [])
  ];

  const suggestedMatches = useMemo(
    () =>
      discoverUsers.map((u) => ({
        id: u.id,
        name: u.fullName || "Người dùng",
        age: u.age ?? calculateAge(u.dateOfBirth),
        city: u.location || "Chưa cập nhật",
        compatibility: Math.floor(Math.random() * 16) + 80,
        online: false,
        newMember: false,
        verified: !!u.isVerified,
        image: u.avatarUrl || "https://placehold.co/400x520/f3e8ff/6b7280?text=No+Avatar",
      })),
    [discoverUsers],
  );

  const recentLikes = useMemo(
    () => {
      if (!stats?.recentLikes) return [];
      return stats.recentLikes.map((like) => ({
        id: like.id,
        name: like.fullName || "Người dùng",
        age: like.dateOfBirth ? calculateAge(like.dateOfBirth) : null,
        image: like.avatarUrl || "https://upload.wikimedia.org/wikipedia/commons/8/89/Portrait_Placeholder.png",
        time: "Vừa xong",
      }));
    },
    [stats],
  );

  const recentChats = useMemo(
    () =>
      matches.slice(0, 3).map((m) => ({
        id: m.id,
        partnerId: m.partner?.id,
        name: m.partner?.fullName || "Người dùng",
        message: m.lastMessage?.content || "Bắt đầu cuộc trò chuyện mới!",
        time: m.lastMessage?.sentAt ? "Mới" : "",
        unread: m.unreadCount || 0,
        online: m.partner?.isOnline || false,
        image: m.partner?.avatarUrl || "https://upload.wikimedia.org/wikipedia/commons/8/89/Portrait_Placeholder.png",
      })),
    [matches]
  );

  const profileName = profile?.fullName || "Người dùng";
  const profileAvatar = profile?.avatarUrl || "https://upload.wikimedia.org/wikipedia/commons/8/89/Portrait_Placeholder.png";
  const profileCompletion = [profile?.fullName, profile?.bio, profile?.location, profile?.avatarUrl].filter(Boolean)
    .length * 25;

  return (
    <div className="min-h-screen bg-aura-bg font-sans">
      {/* Ambient Background */}
      <div className="fixed inset-0 pointer-events-none overflow-hidden z-0">
        <div className="absolute top-[-10%] right-[-5%] w-[800px] h-[800px] bg-aura-pink/10 rounded-full blur-[120px] mix-blend-multiply"></div>
        <div className="absolute bottom-[-10%] left-[-5%] w-[600px] h-[600px] bg-aura-blue/10 rounded-full blur-[100px] mix-blend-multiply"></div>
      </div>

      {/* Header */}
      <header className="sticky top-0 z-50 glass border-b-0 rounded-b-3xl">
        <div className="max-w-[1440px] mx-auto px-8 py-4 flex items-center justify-between">
          <div className="flex items-center gap-12">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-2xl bg-aura-dark flex items-center justify-center shadow-md">
                <Heart className="w-5 h-5 text-white fill-white" />
              </div>
              <span className="text-2xl font-semibold text-aura-dark tracking-tight">
                Aura
              </span>
            </div>

            <nav className="hidden md:flex items-center gap-8">
              {menuItems.map((item, index) => (
                <button
                  key={index}
                  onClick={item.onClick}
                  className={`text-sm font-medium transition-colors ${
                    item.active
                      ? "text-aura-dark"
                      : "text-gray-500 hover:text-aura-dark"
                  }`}
                >
                  {item.label}
                  {item.active && (
                    <div className="h-0.5 bg-aura-dark rounded-t-full mt-1 w-full absolute bottom-[-17px]"></div>
                  )}
                </button>
              ))}
            </nav>
          </div>

          <div className="flex items-center gap-4">
            {/* Notification Dropdown */}
            <div className="relative">
              <button 
                onClick={() => setShowNotifDropdown(!showNotifDropdown)}
                className="w-10 h-10 rounded-xl bg-white border border-gray-200 flex items-center justify-center text-gray-500 hover:text-aura-dark hover:border-aura-dark transition-all btn-magnetic"
              >
                <Bell className="w-5 h-5" />
                {unreadCount > 0 && (
                  <span className="absolute -top-1 -right-1 w-4 h-4 bg-aura-pink text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                    {unreadCount}
                  </span>
                )}
              </button>

              {showNotifDropdown && (
                <div className="absolute right-0 mt-2 w-80 glass rounded-2xl shadow-xl z-50 overflow-hidden border border-gray-100">
                  <div className="p-4 border-b border-gray-100 flex justify-between items-center bg-white/50">
                    <h3 className="font-semibold text-gray-900">Thông báo</h3>
                    {unreadCount > 0 && (
                      <button 
                        onClick={markAllAsRead}
                        className="text-xs text-aura-blue hover:underline"
                      >
                        Đánh dấu đã đọc tất cả
                      </button>
                    )}
                  </div>
                  <div className="max-h-[400px] overflow-y-auto">
                    {notifications.length === 0 ? (
                      <div className="p-6 text-center text-gray-500 text-sm">
                        Không có thông báo nào.
                      </div>
                    ) : (
                      notifications.map(notif => (
                        <div 
                          key={notif.id} 
                          className={`p-4 border-b border-gray-50 flex gap-3 hover:bg-white/60 transition-colors cursor-pointer ${notif.isRead ? 'opacity-70' : 'bg-white/80'}`}
                          onClick={() => !notif.isRead && markAsRead(notif.id)}
                        >
                          <div>
                            <p className={`text-sm ${!notif.isRead ? 'font-semibold text-[#1F2937]' : 'text-[#6B7280]'}`}>
                              {notif.content}
                            </p>
                            <span className="text-xs text-[#9CA3AF] mt-1 block">
                              {new Date(notif.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </span>
                          </div>
                          {!notif.isRead && (
                            <div className="w-2 h-2 rounded-full bg-[#FF5C9A] mt-2 ml-auto flex-shrink-0" />
                          )}
                        </div>
                      ))
                    )}
                  </div>
                </div>
              )}
            </div>
            <div className="flex items-center gap-3 pl-4 border-l border-gray-200">
              <div className="relative">
                <img
                  src={profileAvatar}
                  alt="Avatar"
                  className="w-10 h-10 rounded-full object-cover ring-2 ring-[#FF5C9A]/20"
                />
                <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white"></span>
              </div>
              <div className="text-sm">
                <div className="font-medium text-[#1F2937]">{profileName}</div>
                <div className="text-[#6B7280] text-xs">{profile?.isVerified ? "Verified" : "Thành viên"}</div>
              </div>
            </div>
            {/* Nút Đăng xuất */}
            <button
              onClick={logout}
              title="Đăng xuất"
              className="flex items-center gap-1.5 px-3 py-2 text-sm text-[#6B7280] hover:text-red-500 hover:bg-red-50 rounded-xl transition-all"
            >
              <LogOut className="w-4 h-4" />
              <span className="hidden sm:block">Thoát</span>
            </button>
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
                  <button
                    type="button"
                    key={index}
                    onClick={() => item.onClick?.()}
                    className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                      item.active
                        ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white shadow-lg shadow-[#FF5C9A]/20"
                        : "text-[#6B7280] hover:bg-gray-50"
                    }`}
                  >
                    <span className="text-sm font-medium">{item.label}</span>
                  </button>
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
                    <div
                      className="h-full bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full"
                      style={{ width: `${profileCompletion}%` }}
                    />
                  </div>
                </div>
                <div className="text-2xl font-semibold text-[#FF5C9A]">{profileCompletion}%</div>
                <p className="text-xs text-[#6B7280] mt-2">
                  Cập nhật thêm thông tin để đạt 100%
                </p>
              </div>
            </div>
          </aside>

          {/* Main Content */}
          <main className="col-span-7 lg:col-span-5 space-y-8">
            {/* Welcome Hero Card */}
            <div className="relative overflow-hidden glass rounded-[32px] p-8">
              <div className="absolute top-0 right-0 w-64 h-64 bg-aura-pink/20 rounded-full blur-3xl mix-blend-multiply"></div>
              <div className="relative z-10">
                <h1 className="text-4xl font-semibold text-aura-dark tracking-tight mb-2">
                  Chào mừng trở lại, {profileName}
                </h1>
                <p className="text-gray-500 text-lg mb-8">
                  Hôm nay có <span className="font-semibold text-aura-pink">{suggestedMatches.length} người phù hợp mới</span> dành cho bạn.
                </p>
                <div className="flex gap-4">
                  <button
                    type="button"
                    onClick={() => navigate("/discover")}
                    className="btn-magnetic px-8 py-4 bg-aura-dark text-white rounded-2xl font-medium shadow-md shadow-gray-900/10"
                  >
                    Khám phá ngay
                  </button>
                  <button
                    type="button"
                    onClick={() => navigate("/matches")}
                    className="btn-magnetic px-8 py-4 bg-white text-aura-dark rounded-2xl font-medium border border-gray-200 hover:border-aura-dark transition-all"
                  >
                    Xem lượt thích
                  </button>
                </div>
              </div>
            </div>

            {/* Suggested Matches - Bento Grid */}
            <section>
              <div className="flex items-center justify-between mb-6">
                <div>
                  <h2 className="text-2xl font-semibold text-gray-900 tracking-tight">
                    Gợi ý hôm nay
                  </h2>
                </div>
                <button className="text-aura-dark font-medium flex items-center gap-1 hover:gap-2 transition-all">
                  Tất cả <ChevronRight className="w-4 h-4" />
                </button>
              </div>

              {loading && (
                <div className="glass rounded-[32px] p-8 text-center text-gray-500">
                  <div className="w-8 h-8 border-4 border-aura-pink border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                  Đang phân tích độ tương hợp...
                </div>
              )}
              {!loading && error && (
                <div className="bg-red-50 rounded-3xl border border-red-100 p-6 text-red-600 text-center">
                  {error}
                </div>
              )}
              {!loading && !error && suggestedMatches.length > 0 && (
                <div className="grid grid-cols-2 gap-6">
                  {suggestedMatches.map((match, index) => (
                    <div
                      key={match.id || index}
                      onClick={() => navigate("/discover")}
                      className="group glass p-2 rounded-[32px] cursor-pointer hover:border-aura-dark transition-all duration-500 overflow-hidden"
                    >
                      <div className="relative aspect-[4/5] rounded-[24px] overflow-hidden">
                        <img
                          src={match.image}
                          alt={match.name}
                          className="w-full h-full object-cover group-hover:scale-[1.03] transition-transform duration-700"
                        />
                        {match.online && (
                          <div className="absolute top-4 right-4 px-3 py-1.5 bg-white/40 backdrop-blur-md rounded-full flex items-center gap-2 border border-white/40">
                            <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse shadow-[0_0_8px_rgba(74,222,128,0.8)]"></span>
                            <span className="text-xs font-semibold text-white tracking-wide">ONLINE</span>
                          </div>
                        )}
                        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent"></div>
                        <div className="absolute bottom-0 left-0 right-0 p-5">
                          <div className="flex items-end justify-between">
                            <div>
                              <h3 className="font-semibold text-2xl text-white tracking-tight leading-tight flex items-center gap-2 mb-1">
                                {match.name}, {match.age}
                                {match.verified && (
                                  <div className="w-5 h-5 bg-aura-blue rounded-full flex items-center justify-center">
                                    <Sparkles className="w-3 h-3 text-white" />
                                  </div>
                                )}
                              </h3>
                              <div className="flex items-center gap-1.5 text-sm text-white/80">
                                <MapPin className="w-4 h-4" />
                                <span>{match.city}</span>
                              </div>
                            </div>
                            <div className="bg-white/20 backdrop-blur-md px-3 py-2 rounded-2xl border border-white/20 text-center">
                              <div className="text-xl font-bold text-white">{match.compatibility}%</div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </section>
          </main>

          {/* Right Sidebar - Social & Stats */}
          <aside className="col-span-7 lg:col-span-2 space-y-6">
            <div className="sticky top-24 space-y-6">
              {/* Recent Likes Mini-Bento */}
              <div className="glass rounded-[32px] p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 tracking-tight flex items-center justify-between">
                  Vừa thích bạn
                  <button className="text-aura-dark bg-gray-50 p-1.5 rounded-full hover:bg-gray-100 transition-colors">
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </h3>
                <div className="grid grid-cols-2 gap-3">
                  {recentLikes.slice(0, 4).map((like) => (
                    <div key={like.id} className="relative group cursor-pointer aspect-square rounded-2xl overflow-hidden">
                      <img
                        src={like.image}
                        alt={like.name}
                        className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                      />
                      <div className="absolute inset-0 bg-gradient-to-t from-aura-pink/80 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex items-end justify-center pb-2">
                        <Heart className="w-5 h-5 text-white fill-white" />
                      </div>
                    </div>
                  ))}
                  {recentLikes.length === 0 && (
                    <div className="col-span-2 text-center text-sm text-gray-500 py-4">
                      Chưa có lượt thích mới.
                    </div>
                  )}
                </div>
              </div>

              {/* Quick Chats */}
              <div className="glass rounded-[32px] p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 tracking-tight">
                  Tin nhắn mới
                </h3>
                <div className="space-y-4">
                  {recentChats.slice(0, 3).map((chat) => (
                    <div
                      key={chat.id}
                      onClick={() => {
                        if (chat.partnerId) navigate(`/chat/${chat.partnerId}`);
                      }}
                      className="flex items-center gap-3 cursor-pointer group"
                    >
                      <div className="relative">
                        <img
                          src={chat.image}
                          alt={chat.name}
                          className="w-12 h-12 rounded-full object-cover group-hover:ring-2 ring-aura-dark transition-all"
                        />
                        {chat.online && (
                          <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white"></span>
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h4 className="font-semibold text-gray-900 text-sm truncate">{chat.name}</h4>
                        <p className="text-xs text-gray-500 truncate">{chat.message}</p>
                      </div>
                      {chat.unread > 0 && (
                        <div className="w-5 h-5 bg-aura-pink rounded-full flex items-center justify-center">
                          <span className="text-[10px] font-bold text-white">{chat.unread}</span>
                        </div>
                      )}
                    </div>
                  ))}
                  {recentChats.length === 0 && (
                    <div className="text-center text-sm text-gray-500 py-2">
                      Bắt đầu cuộc trò chuyện.
                    </div>
                  )}
                </div>
              </div>

              {/* Connection Tips */}
              <div className="bg-aura-dark rounded-[32px] p-6 relative overflow-hidden">
                <div className="absolute -right-4 -bottom-4 w-32 h-32 bg-aura-pink/30 rounded-full blur-2xl mix-blend-screen"></div>
                <div className="relative z-10">
                  <div className="w-10 h-10 bg-white/10 rounded-xl flex items-center justify-center mb-4 border border-white/10 backdrop-blur-md">
                    <Sparkles className="w-5 h-5 text-white" />
                  </div>
                  <h3 className="font-semibold text-white mb-2">
                    Mẹo tăng tương tác
                  </h3>
                  <p className="text-sm text-white/70 leading-relaxed">
                    Ảnh đại diện tự nhiên, chụp ngoài trời giúp tăng 40% lượt phản hồi tin nhắn.
                  </p>
                </div>
              </div>
            </div>
          </aside>
        </div>
      </div>

      {/* Realtime Toast Notification */}
      {toastNotif && (
        <div className="fixed bottom-6 right-6 z-[100] glass rounded-[24px] p-4 flex items-center gap-4 animate-[slideIn_0.3s_ease-out] shadow-2xl">
          <div className="w-12 h-12 rounded-full bg-aura-dark flex items-center justify-center text-xl text-white">
            {toastNotif.type === 'NewMatch' ? '🎉' : '🔔'}
          </div>
          <div className="pr-4">
            <h4 className="font-bold text-gray-900 text-sm tracking-tight">Thông báo mới</h4>
            <p className="text-gray-500 text-sm line-clamp-1">{toastNotif.content}</p>
          </div>
          <button onClick={() => setToastNotif(null)} className="ml-auto text-gray-400 hover:text-gray-900 p-2">
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      <style>{`
        @keyframes slideIn {
          from { transform: translateX(100%); opacity: 0; }
          to { transform: translateX(0); opacity: 1; }
        }
      `}</style>
    </div>
  );
}
