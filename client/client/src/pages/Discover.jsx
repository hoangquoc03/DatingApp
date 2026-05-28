import { useEffect, useState, useRef, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { Heart, X, Star, MapPin, Info, Loader2, RefreshCw } from "lucide-react";
import TinderCard from "react-tinder-card";
import api from "../services/api";

// Màu gradient ngẫu nhiên cho avatar placeholder
const GRADIENTS = [
  "from-pink-200 to-purple-300",
  "from-rose-200 to-pink-300",
  "from-violet-200 to-purple-300",
  "from-fuchsia-200 to-pink-300",
  "from-purple-200 to-indigo-300",
];

export default function Discover() {
  const navigate = useNavigate();

  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [page, setPage] = useState(1);
  const [hasNextPage, setHasNextPage] = useState(true);
  const pageSize = 12;

  // Index card đang hiện trên cùng (users[currentIndex])
  const [currentIndex, setCurrentIndex] = useState(0);
  const [lastAction, setLastAction] = useState(null); // "like" | "dislike" | "superlike"
  const [matchPopup, setMatchPopup] = useState(null); // user vừa match

  // Ref để gọi swipe từ button
  const cardRefs = useRef([]);

  useEffect(() => {
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    if (!token) {
      navigate("/login");
      return;
    }
    fetchUsers(1, true);
  }, [navigate]);

  async function fetchUsers(nextPage, reset = false) {
    try {
      setLoading(true);
      setError("");
      const { data } = await api.get(`/User/discover?page=${nextPage}&pageSize=${pageSize}`);
      const incoming = data?.data || [];
      const pagination = data?.pagination;

      setUsers((prev) => {
        const merged = reset ? incoming : [...prev, ...incoming];
        cardRefs.current = merged.map(() => null);
        return merged;
      });
      if (reset) setCurrentIndex(0);
      setPage(nextPage);
      setHasNextPage(Boolean(pagination?.hasNext));
    } catch (err) {
      if (err.response?.status === 401) {
        navigate("/login");
      } else {
        setError("Không thể tải danh sách người dùng");
      }
    } finally {
      setLoading(false);
    }
  }

  // ── Gửi swipe lên API ────────────────────────────────────────────────────
  async function sendSwipe(toUserId, isLike) {
    try {
      const { data } = await api.post("/Swipe", { toUserId, isLike });
      if (data.isMatch) {
        const matchedUser = users.find((u) => u.id === toUserId);
        if (matchedUser) setMatchPopup(matchedUser);
      }
    } catch (err) {
      console.error("swipe error:", err.response?.data || err.message);
    }
  }

  // ── Callback khi card bị swipe (từ gesture hoặc button) ──────────────────
  const onSwipe = useCallback(
    (direction, user, index) => {
      const isLike = direction === "right" || direction === "up";
      const isSuperLike = direction === "up";

      setLastAction(isSuperLike ? "superlike" : isLike ? "like" : "dislike");
      setCurrentIndex((prev) => prev + 1);

      sendSwipe(user.id, isLike);

      // Reset animation feedback sau 1.5s
      setTimeout(() => setLastAction(null), 1500);
    },
    [users],
  );

  // ── Trigger swipe từ action buttons ──────────────────────────────────────
  const swipeCard = useCallback(
    (dir) => {
      if (currentIndex >= users.length) return;
      cardRefs.current[currentIndex]?.swipe(dir);
    },
    [currentIndex, users.length]
  );

  const remaining = users.length - currentIndex;

  useEffect(() => {
    if (!loading && !error && remaining <= 2 && hasNextPage) {
      fetchUsers(page + 1);
    }
  }, [remaining, hasNextPage, loading, error, page]);

  // ── RENDER ────────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50/50 via-white to-purple-50/50 flex flex-col">
      {/* Header */}
      <header className="sticky top-0 z-40 bg-white/80 backdrop-blur-xl border-b border-gray-100">
        <div className="max-w-lg mx-auto px-5 py-3.5 flex items-center justify-between">
          <button
            onClick={() => navigate("/dashboard")}
            className="text-sm text-[#6B7280] hover:text-[#FF5C9A] transition-colors"
          >
            ← Dashboard
          </button>
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center">
              <Heart className="w-4 h-4 text-white fill-white" />
            </div>
            <span className="font-semibold text-[#1F2937]">Khám phá</span>
          </div>
          {remaining > 0 && (
            <span className="text-sm text-[#6B7280]">{remaining} người</span>
          )}
        </div>
      </header>

      {/* Action feedback banner */}
      {lastAction && (
        <div
          className={`
            fixed top-20 left-1/2 -translate-x-1/2 z-50
            px-5 py-2.5 rounded-2xl text-white font-semibold text-sm
            shadow-xl transition-all animate-fade-in
            ${lastAction === "like"
              ? "bg-gradient-to-r from-green-400 to-emerald-500"
              : lastAction === "superlike"
              ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF]"
              : "bg-gradient-to-r from-gray-400 to-gray-500"
            }
          `}
        >
          {lastAction === "like" && "💚 Thích!"}
          {lastAction === "dislike" && "👋 Bỏ qua"}
          {lastAction === "superlike" && "⭐ Super Like!"}
        </div>
      )}

      {/* Main */}
      <main className="flex-1 flex flex-col items-center justify-center px-4 py-6 gap-6">
        {loading && (
          <div className="flex flex-col items-center gap-4">
            <Loader2 className="w-10 h-10 animate-spin text-[#FF5C9A]" />
            <p className="text-[#6B7280]">Đang tìm người phù hợp...</p>
          </div>
        )}

        {error && (
          <div className="text-center space-y-4">
            <p className="text-red-500">{error}</p>
            <button
              onClick={() => fetchUsers(1, true)}
              className="flex items-center gap-2 px-5 py-2.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-xl mx-auto"
            >
              <RefreshCw className="w-4 h-4" /> Thử lại
            </button>
          </div>
        )}

        {!loading && !error && remaining === 0 && (
          <div className="flex flex-col items-center gap-5 text-center py-10">
            <div className="w-28 h-28 rounded-full bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 flex items-center justify-center">
              <Heart className="w-14 h-14 text-[#FF5C9A]/30" />
            </div>
            <div>
              <h2 className="text-2xl font-bold text-[#1F2937]">Đã hết người rồi!</h2>
              <p className="text-[#6B7280] mt-2">Bạn đã swipe hết danh sách hiện có</p>
            </div>
            <button
              onClick={() => fetchUsers(1, true)}
              className="flex items-center gap-2 px-8 py-3 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-2xl font-medium shadow-lg shadow-pink-500/30"
            >
              <RefreshCw className="w-4 h-4" /> Tải lại
            </button>
          </div>
        )}

        {!loading && !error && remaining > 0 && (
          <>
            {/* Card stack */}
            <div className="relative w-full max-w-sm" style={{ height: 520 }}>
              {/* Hiển thị tối đa 3 card (stack effect) */}
              {users.slice(currentIndex, currentIndex + 3).map((user, stackIdx) => {
                const absIdx = currentIndex + stackIdx;
                const isTop = stackIdx === 0;

                return (
                  <TinderCard
                    key={user.id}
                    ref={(el) => { if (isTop) cardRefs.current[absIdx] = el; }}
                    onSwipe={(dir) => isTop && onSwipe(dir, user, absIdx)}
                    preventSwipe={isTop ? [] : ["left", "right", "up", "down"]}
                    swipeRequirementType="position"
                    swipeThreshold={80}
                    className="absolute inset-0"
                  >
                    <SwipeCard
                      user={user}
                      style={{
                        transform: stackIdx === 0
                          ? "scale(1) translateY(0)"
                          : stackIdx === 1
                          ? "scale(0.95) translateY(12px)"
                          : "scale(0.9) translateY(24px)",
                        zIndex: 10 - stackIdx,
                        pointerEvents: isTop ? "auto" : "none",
                      }}
                    />
                  </TinderCard>
                );
              })}
            </div>

            {/* Action buttons */}
            <div className="flex items-center gap-5">
              <ActionBtn
                onClick={() => swipeCard("left")}
                icon={<X className="w-7 h-7" />}
                color="text-gray-400"
                bg="bg-white border-2 border-gray-200"
                size="w-14 h-14"
                label="Bỏ qua"
              />
              <ActionBtn
                onClick={() => swipeCard("up")}
                icon={<Star className="w-6 h-6" />}
                color="text-[#C8B6FF]"
                bg="bg-white border-2 border-[#C8B6FF]/40"
                size="w-12 h-12"
                label="Super Like"
              />
              <ActionBtn
                onClick={() => swipeCard("right")}
                icon={<Heart className="w-7 h-7 fill-white" />}
                color="text-white"
                bg="bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] shadow-lg shadow-pink-500/40"
                size="w-14 h-14"
                label="Thích"
              />
            </div>
          </>
        )}
      </main>

      {/* ── Match popup ── */}
      {matchPopup && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm px-6">
          <div className="bg-white rounded-3xl p-8 max-w-xs w-full text-center space-y-5 shadow-2xl">
            {/* Hearts animation */}
            <div className="relative flex justify-center">
              <div className="w-24 h-24 rounded-full bg-gradient-to-br from-[#FF5C9A]/20 to-[#C8B6FF]/20 flex items-center justify-center">
                {matchPopup.avatarUrl ? (
                  <img src={matchPopup.avatarUrl} alt="" className="w-24 h-24 rounded-full object-cover" />
                ) : (
                  <span className="text-4xl font-bold text-[#FF5C9A]">
                    {matchPopup.fullName?.[0]}
                  </span>
                )}
              </div>
              <div className="absolute -top-2 -right-2 w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full flex items-center justify-center shadow-lg animate-bounce">
                <Heart className="w-5 h-5 text-white fill-white" />
              </div>
            </div>

            <div>
              <h2 className="text-2xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
                It's a Match! 🎉
              </h2>
              <p className="text-[#6B7280] mt-2 text-sm">
                Bạn và <strong className="text-[#1F2937]">{matchPopup.fullName}</strong> đã thích nhau!
              </p>
            </div>

            <div className="flex flex-col gap-3">
              <button
                onClick={() => {
                  setMatchPopup(null);
                  navigate(`/chat/${matchPopup.id}`);
                }}
                className="w-full py-3 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-2xl font-medium shadow-lg shadow-pink-500/30"
              >
                Nhắn tin ngay
              </button>
              <button
                onClick={() => setMatchPopup(null)}
                className="w-full py-3 bg-gray-100 text-[#6B7280] rounded-2xl font-medium hover:bg-gray-200 transition-colors"
              >
                Tiếp tục khám phá
              </button>
            </div>
          </div>
        </div>
      )}

      <style>{`
        @keyframes fade-in {
          from { opacity: 0; transform: translateX(-50%) translateY(-8px); }
          to   { opacity: 1; transform: translateX(-50%) translateY(0); }
        }
        .animate-fade-in { animation: fade-in 0.25s ease-out; }
      `}</style>
    </div>
  );
}

// ─── SwipeCard ────────────────────────────────────────────────────────────────
function SwipeCard({ user, style }) {
  const [showInfo, setShowInfo] = useState(false);
  const initials = user.fullName?.split(" ").map((w) => w[0]).join("").slice(0, 2).toUpperCase();
  const gradient = GRADIENTS[Math.abs(user.fullName?.charCodeAt(0) || 0) % GRADIENTS.length];
  const age = user.dateOfBirth
    ? Math.floor((Date.now() - new Date(user.dateOfBirth)) / (365.25 * 24 * 3600 * 1000))
    : null;

  return (
    <div
      className="w-full h-full rounded-3xl overflow-hidden shadow-2xl bg-white cursor-grab active:cursor-grabbing select-none"
      style={style}
    >
      {/* Photo */}
      <div className="relative h-[75%] overflow-hidden">
        {user.avatarUrl ? (
          <img
            src={user.avatarUrl}
            alt={user.fullName}
            className="w-full h-full object-cover pointer-events-none"
            draggable={false}
          />
        ) : (
          <div className={`w-full h-full bg-gradient-to-br ${gradient} flex items-center justify-center`}>
            <span className="text-7xl font-bold text-white/60">{initials}</span>
          </div>
        )}

        {/* Gradient overlay bottom */}
        <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-black/60 to-transparent" />

        {/* Info toggle */}
        <button
          onClick={(e) => {
            e.stopPropagation();
            setShowInfo((v) => !v);
          }}
          className="absolute top-4 right-4 w-9 h-9 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center hover:bg-white/30 transition-colors"
        >
          <Info className="w-4 h-4 text-white" />
        </button>

        {/* Name overlay */}
        <div className="absolute bottom-4 left-5 right-5">
          <h2 className="text-2xl font-bold text-white">
            {user.fullName}{age ? `, ${age}` : ""}
          </h2>
          {user.location && (
            <p className="text-white/80 text-sm flex items-center gap-1 mt-0.5">
              <MapPin className="w-3.5 h-3.5" />
              {user.location}
            </p>
          )}
        </div>
      </div>

      {/* Bio / info panel */}
      <div className="h-[25%] px-5 py-4 flex flex-col justify-between">
        {showInfo || user.bio ? (
          <p className="text-sm text-[#6B7280] line-clamp-3 leading-relaxed">
            {user.bio || "Vuốt để khám phá thêm ✨"}
          </p>
        ) : <p className="text-sm text-[#9CA3AF]">Vuốt để khám phá thêm ✨</p>}
        <p className="text-xs text-[#9CA3AF] text-center">
          Vuốt phải 💚 để thích · Vuốt trái 👋 để bỏ qua
        </p>
      </div>
    </div>
  );
}

// ─── ActionBtn ────────────────────────────────────────────────────────────────
function ActionBtn({ onClick, icon, color, bg, size, label }) {
  return (
    <button
      onClick={onClick}
      title={label}
      className={`
        ${size} ${bg} ${color}
        rounded-full flex items-center justify-center
        hover:scale-110 active:scale-95
        transition-transform duration-150 shadow-sm
      `}
    >
      {icon}
    </button>
  );
}
