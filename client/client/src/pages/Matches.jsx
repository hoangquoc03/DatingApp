import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Heart, MessageCircle, MapPin, Search, Sparkles } from "lucide-react";
import api from "../services/api";

function getToken() {
  return localStorage.getItem("token") || sessionStorage.getItem("token");
}

export default function Matches() {
  const navigate = useNavigate();
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [newMatchId, setNewMatchId] = useState(null); // animate khi vừa match

  useEffect(() => {
    const token = getToken();
    if (!token) { navigate("/login"); return; }

    // Kiểm tra xem có match mới từ state navigation không
    // (SwipeController trả về isMatch: true)
    const state = window.history.state?.usr;
    if (state?.newMatchUserId) setNewMatchId(state.newMatchUserId);

    fetchMatches();
  }, []);

  async function fetchMatches() {
    try {
      setLoading(true);
      const { data } = await api.get("/Match");
      setMatches(data);
    } catch (err) {
      if (err.response?.status === 401) navigate("/login");
      else setError("Không thể tải danh sách match");
    } finally {
      setLoading(false);
    }
  }

  const filtered = matches.filter((m) =>
    m.partner?.fullName?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-white">
      {/* Ambient bg */}
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
        <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-gradient-to-br from-[#FF5C9A]/8 via-[#C8B6FF]/8 to-transparent rounded-full blur-3xl" />
        <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-gradient-to-tr from-[#C8B6FF]/8 to-transparent rounded-full blur-3xl" />
      </div>

      <div className="relative max-w-4xl mx-auto px-6 py-10">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold text-[#1F2937]">
              Matches của bạn
            </h1>
            <p className="text-[#6B7280] mt-1">
              {matches.length > 0
                ? `${matches.length} kết nối đang chờ bạn`
                : "Hãy tiếp tục khám phá để tìm match!"}
            </p>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center shadow-lg shadow-pink-500/30">
            <Heart className="w-6 h-6 text-white fill-white" />
          </div>
        </div>

        {/* Search */}
        {matches.length > 0 && (
          <div className="relative mb-8">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#9CA3AF]" />
            <input
              type="text"
              placeholder="Tìm kiếm match..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-12 pr-4 py-3.5 bg-white border-2 border-gray-100 rounded-2xl outline-none focus:border-[#FF5C9A] focus:ring-4 focus:ring-[#FF5C9A]/10 transition-all text-[#1F2937] placeholder:text-[#9CA3AF]"
            />
          </div>
        )}

        {/* Loading */}
        {loading && (
          <div className="flex flex-col items-center justify-center py-24 gap-4">
            <div className="w-12 h-12 border-4 border-[#FF5C9A]/20 border-t-[#FF5C9A] rounded-full animate-spin" />
            <p className="text-[#6B7280]">Đang tải matches...</p>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="text-center py-16">
            <p className="text-red-500 mb-4">{error}</p>
            <button
              onClick={() => fetchMatches()}
              className="px-6 py-2 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-xl"
            >
              Thử lại
            </button>
          </div>
        )}

        {/* Empty state */}
        {!loading && !error && matches.length === 0 && (
          <div className="flex flex-col items-center justify-center py-24 gap-6">
            <div className="relative">
              <div className="w-32 h-32 rounded-full bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 flex items-center justify-center">
                <Heart className="w-16 h-16 text-[#FF5C9A]/40" />
              </div>
              <div className="absolute -top-2 -right-2 w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full flex items-center justify-center shadow-lg animate-bounce">
                <Sparkles className="w-5 h-5 text-white" />
              </div>
            </div>
            <div className="text-center space-y-2">
              <h2 className="text-2xl font-bold text-[#1F2937]">Chưa có match nào</h2>
              <p className="text-[#6B7280] max-w-xs">
                Tiếp tục khám phá và swipe để tìm người phù hợp với bạn!
              </p>
            </div>
            <button
              onClick={() => navigate("/discover")}
              className="px-8 py-3 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-2xl font-medium shadow-lg shadow-pink-500/30 hover:scale-105 transition-transform"
            >
              Khám phá ngay
            </button>
          </div>
        )}

        {/* Match grid */}
        {!loading && !error && filtered.length > 0 && (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {filtered.map((match) => (
              <MatchCard
                key={match.id}
                match={match}
                isNew={match.partner?.id === newMatchId}
                onClick={() => {
                  if (match.partner?.id) navigate(`/chat/${match.partner.id}`);
                }}
              />
            ))}
          </div>
        )}

        {/* No search results */}
        {!loading && !error && matches.length > 0 && filtered.length === 0 && (
          <div className="text-center py-16 text-[#6B7280]">
            Không tìm thấy match nào với từ khoá "{search}"
          </div>
        )}
      </div>
    </div>
  );
}

function MatchCard({ match, isNew, onClick }) {
  const partner = match.partner || {};
  const initials = partner.fullName
    ?.split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <div
      onClick={onClick}
      className={`
        group relative cursor-pointer rounded-3xl overflow-hidden
        bg-white border-2 transition-all duration-300
        hover:shadow-2xl hover:-translate-y-1
        ${isNew
          ? "border-[#FF5C9A] shadow-lg shadow-pink-500/20 animate-pulse-once"
          : "border-gray-100 hover:border-[#FF5C9A]/30"
        }
      `}
    >
      {/* Avatar */}
      <div className="aspect-[3/4] relative overflow-hidden bg-gradient-to-br from-pink-100 to-purple-100">
        {partner.avatarUrl ? (
          <img
            src={partner.avatarUrl}
            alt={partner.fullName}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <span className="text-4xl font-bold text-[#FF5C9A]/60">{initials}</span>
          </div>
        )}

        {/* New match badge */}
        {isNew && (
          <div className="absolute top-3 left-3 px-2.5 py-1 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full">
            <span className="text-xs font-semibold text-white">✨ Mới</span>
          </div>
        )}

        {/* Chat button overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-end justify-center pb-4">
          <div className="flex items-center gap-1.5 px-4 py-2 bg-white/90 backdrop-blur-sm rounded-full text-sm font-medium text-[#FF5C9A]">
            <MessageCircle className="w-4 h-4" />
            Nhắn tin
          </div>
        </div>
      </div>

      {/* Info */}
      <div className="p-3">
        <p className="font-semibold text-[#1F2937] text-sm truncate">{partner.fullName}</p>
        {partner.bio && (
          <p className="text-xs text-[#9CA3AF] flex items-center gap-1 mt-0.5 truncate">
            <MapPin className="w-3 h-3 flex-shrink-0" />
            {partner.bio}
          </p>
        )}
      </div>
    </div>
  );
}
