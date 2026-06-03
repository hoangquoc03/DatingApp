import { useEffect, useState, useRef } from "react";
import { useNavigate } from "react-router-dom";
import {
  Heart,
  MessageCircle,
  MapPin,
  Search,
  Sparkles,
  MoreVertical,
  Shield,
  UserX,
  Flag,
  X,
  AlertTriangle,
  Loader2,
} from "lucide-react";
import api from "../services/api";

// ─── MAIN COMPONENT ───────────────────────────────────────────────────────────
export default function Matches() {
  const navigate = useNavigate();
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [newMatchId, setNewMatchId] = useState(null);

  // Action modals
  const [actionTarget, setActionTarget] = useState(null); // { matchId, partnerId, partnerName }
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportReason, setReportReason] = useState("");
  const [reportDescription, setReportDescription] = useState("");
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
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

  // ─── Actions ────────────────────────────────────────────────────────────────
  async function handleUnmatch() {
    if (!actionTarget) return;
    if (!window.confirm(`Bạn có chắc muốn huỷ match với ${actionTarget.partnerName}?`)) return;
    try {
      setActionLoading(true);
      await api.delete(`/Match/${actionTarget.matchId}`);
      setMatches((prev) => prev.filter((m) => m.id !== actionTarget.matchId));
      setActionTarget(null);
    } catch (err) {
      alert(err.response?.data?.message || "Huỷ match thất bại");
    } finally {
      setActionLoading(false);
    }
  }

  async function handleBlock() {
    if (!actionTarget) return;
    if (!window.confirm(`Chặn ${actionTarget.partnerName}? Người này sẽ không thể liên lạc với bạn.`)) return;
    try {
      setActionLoading(true);
      await api.post(`/Match/block/${actionTarget.partnerId}`);
      setMatches((prev) => prev.filter((m) => m.id !== actionTarget.matchId));
      setActionTarget(null);
    } catch (err) {
      alert(err.response?.data?.message || "Chặn thất bại");
    } finally {
      setActionLoading(false);
    }
  }

  async function handleReport() {
    if (!actionTarget || !reportReason) return;
    try {
      setActionLoading(true);
      await api.post(`/Match/report/${actionTarget.partnerId}`, {
        reason: reportReason,
        description: reportDescription,
      });
      setShowReportModal(false);
      setReportReason("");
      setReportDescription("");
      setActionTarget(null);
      alert("Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất.");
    } catch (err) {
      alert(err.response?.data?.message || "Báo cáo thất bại");
    } finally {
      setActionLoading(false);
    }
  }

  const filtered = matches.filter((m) =>
    m.partner?.fullName?.toLowerCase().includes(search.toLowerCase())
  );

  function timeAgo(dateStr) {
    if (!dateStr) return "";
    const d = new Date(dateStr);
    const now = Date.now();
    const diff = Math.floor((now - d.getTime()) / 1000);
    if (diff < 60) return "Vừa xong";
    if (diff < 3600) return `${Math.floor(diff / 60)} phút`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} giờ`;
    return `${Math.floor(diff / 86400)} ngày`;
  }

  return (
    <div className="min-h-screen bg-aura-bg font-sans">
      {/* Ambient bg */}
      <div className="fixed inset-0 pointer-events-none overflow-hidden z-0">
        <div className="absolute top-[-10%] right-[-5%] w-[800px] h-[800px] bg-aura-pink/10 rounded-full blur-[120px] mix-blend-multiply" />
        <div className="absolute bottom-[-10%] left-[-5%] w-[600px] h-[600px] bg-aura-blue/10 rounded-full blur-[100px] mix-blend-multiply" />
      </div>

      <div className="relative z-10 max-w-[1200px] mx-auto px-8 py-12">
        {/* Header */}
        <div className="flex items-center justify-between mb-10">
          <div>
            <h1 className="text-4xl font-semibold text-aura-dark tracking-tight">
              Tương hợp của bạn
            </h1>
            <p className="text-gray-500 mt-2 text-lg">
              {matches.length > 0
                ? `${matches.length} kết nối đang chờ bạn`
                : "Hãy tiếp tục khám phá để tìm match!"}
            </p>
          </div>
          <div className="flex items-center gap-6">
            <button
              onClick={() => navigate("/dashboard")}
              className="text-sm font-medium text-gray-500 hover:text-aura-dark transition-colors"
            >
              Quay lại Dashboard
            </button>
            <div className="w-14 h-14 rounded-2xl bg-aura-dark flex items-center justify-center shadow-lg">
              <Heart className="w-6 h-6 text-white fill-white" />
            </div>
          </div>
        </div>

        {/* Search */}
        {matches.length > 0 && (
          <div className="relative mb-10 max-w-md">
            <Search className="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Tìm kiếm match..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-14 pr-5 py-4 glass border-white/50 rounded-[20px] outline-none focus:border-aura-dark/30 focus:shadow-lg transition-all text-gray-900 placeholder:text-gray-400 font-medium"
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
          <div className="text-center py-20 glass rounded-[32px]">
            <p className="text-red-500 mb-6 font-medium">{error}</p>
            <button
              onClick={() => fetchMatches()}
              className="btn-magnetic px-8 py-3 bg-aura-dark text-white rounded-2xl font-medium"
            >
              Thử lại
            </button>
          </div>
        )}

        {/* Empty state */}
        {!loading && !error && matches.length === 0 && (
          <div className="flex flex-col items-center justify-center py-32 gap-8 glass rounded-[40px]">
            <div className="relative">
              <div className="w-32 h-32 rounded-full bg-white/40 flex items-center justify-center backdrop-blur-sm border border-white/50">
                <Heart className="w-14 h-14 text-aura-pink/40" />
              </div>
              <div className="absolute -top-2 -right-2 w-12 h-12 bg-aura-dark rounded-full flex items-center justify-center shadow-lg animate-bounce">
                <Sparkles className="w-6 h-6 text-white" />
              </div>
            </div>
            <div className="text-center space-y-3">
              <h2 className="text-3xl font-semibold text-aura-dark tracking-tight">Chưa có tương hợp nào</h2>
              <p className="text-gray-500 max-w-sm text-lg">
                Tiếp tục khám phá và vuốt để tìm người phù hợp với bạn!
              </p>
            </div>
            <button
              onClick={() => navigate("/discover")}
              className="btn-magnetic px-10 py-4 bg-aura-dark text-white rounded-2xl font-medium shadow-lg mt-4"
            >
              Khám phá ngay
            </button>
          </div>
        )}

        {/* Match grid */}
        {!loading && !error && filtered.length > 0 && (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-6">
            {filtered.map((match) => (
              <MatchCard
                key={match.id}
                match={match}
                isNew={match.partner?.id === newMatchId}
                onClick={() => {
                  if (match.partner?.id) navigate(`/chat/${match.partner.id}`);
                }}
                onAction={() =>
                  setActionTarget({
                    matchId: match.id,
                    partnerId: match.partner?.id,
                    partnerName: match.partner?.fullName,
                  })
                }
                timeAgo={timeAgo}
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

      {/* ── Action Sheet Modal ────────────────────────────────────────────────── */}
      {actionTarget && !showReportModal && (
        <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center">
          <div
            className="absolute inset-0 bg-black/40 backdrop-blur-sm"
            onClick={() => setActionTarget(null)}
          />
          <div className="relative w-full sm:max-w-sm bg-white rounded-t-3xl sm:rounded-3xl p-6 shadow-2xl">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-bold text-[#1F2937]">
                {actionTarget.partnerName}
              </h3>
              <button
                onClick={() => setActionTarget(null)}
                className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center hover:bg-gray-200"
              >
                <X className="w-4 h-4 text-[#6B7280]" />
              </button>
            </div>

            <div className="space-y-2">
              <button
                onClick={handleUnmatch}
                disabled={actionLoading}
                className="w-full flex items-center gap-3 px-4 py-3.5 rounded-2xl hover:bg-gray-50 transition-colors text-left"
              >
                <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center">
                  <UserX className="w-5 h-5 text-amber-500" />
                </div>
                <div>
                  <p className="font-medium text-[#1F2937]">Huỷ Match</p>
                  <p className="text-xs text-[#6B7280]">Xoá kết nối và lịch sử chat</p>
                </div>
              </button>

              <button
                onClick={handleBlock}
                disabled={actionLoading}
                className="w-full flex items-center gap-3 px-4 py-3.5 rounded-2xl hover:bg-gray-50 transition-colors text-left"
              >
                <div className="w-10 h-10 rounded-xl bg-red-50 flex items-center justify-center">
                  <Shield className="w-5 h-5 text-red-500" />
                </div>
                <div>
                  <p className="font-medium text-[#1F2937]">Chặn người dùng</p>
                  <p className="text-xs text-[#6B7280]">Người này sẽ không thể tìm thấy bạn</p>
                </div>
              </button>

              <button
                onClick={() => setShowReportModal(true)}
                disabled={actionLoading}
                className="w-full flex items-center gap-3 px-4 py-3.5 rounded-2xl hover:bg-gray-50 transition-colors text-left"
              >
                <div className="w-10 h-10 rounded-xl bg-orange-50 flex items-center justify-center">
                  <Flag className="w-5 h-5 text-orange-500" />
                </div>
                <div>
                  <p className="font-medium text-[#1F2937]">Báo cáo vi phạm</p>
                  <p className="text-xs text-[#6B7280]">Báo cáo nội dung không phù hợp</p>
                </div>
              </button>
            </div>

            {actionLoading && (
              <div className="absolute inset-0 bg-white/80 rounded-3xl flex items-center justify-center">
                <Loader2 className="w-8 h-8 text-[#FF5C9A] animate-spin" />
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── Report Modal ──────────────────────────────────────────────────────── */}
      {showReportModal && actionTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center px-6">
          <div
            className="absolute inset-0 bg-black/40 backdrop-blur-sm"
            onClick={() => {
              setShowReportModal(false);
              setReportReason("");
              setReportDescription("");
            }}
          />
          <div className="relative w-full max-w-md bg-white rounded-3xl p-6 shadow-2xl">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-xl bg-orange-50 flex items-center justify-center">
                <AlertTriangle className="w-5 h-5 text-orange-500" />
              </div>
              <div>
                <h3 className="font-bold text-[#1F2937]">Báo cáo {actionTarget.partnerName}</h3>
                <p className="text-xs text-[#6B7280]">Chọn lý do báo cáo</p>
              </div>
            </div>

            <div className="space-y-2 mb-4">
              {[
                { value: "fake_profile", label: "🎭 Hồ sơ giả mạo" },
                { value: "harassment", label: "⚠️ Quấy rối" },
                { value: "inappropriate", label: "🚫 Nội dung không phù hợp" },
                { value: "spam", label: "📧 Spam" },
                { value: "other", label: "📝 Khác" },
              ].map((opt) => (
                <button
                  key={opt.value}
                  onClick={() => setReportReason(opt.value)}
                  className={`w-full text-left px-4 py-3 rounded-xl border-2 text-sm font-medium transition-all ${
                    reportReason === opt.value
                      ? "border-[#FF5C9A] bg-[#FF5C9A]/5 text-[#FF5C9A]"
                      : "border-gray-100 text-[#6B7280] hover:border-gray-200"
                  }`}
                >
                  {opt.label}
                </button>
              ))}
            </div>

            {reportReason === "other" && (
              <textarea
                placeholder="Mô tả chi tiết..."
                value={reportDescription}
                onChange={(e) => setReportDescription(e.target.value)}
                rows={3}
                className="w-full px-4 py-3 border-2 border-gray-100 rounded-xl outline-none focus:border-[#FF5C9A] text-sm text-[#1F2937] mb-4 resize-none"
              />
            )}

            <div className="flex gap-3">
              <button
                onClick={() => {
                  setShowReportModal(false);
                  setReportReason("");
                  setReportDescription("");
                }}
                className="flex-1 py-3 bg-gray-100 text-[#6B7280] rounded-2xl font-medium hover:bg-gray-200 transition-colors"
              >
                Huỷ
              </button>
              <button
                onClick={handleReport}
                disabled={!reportReason || actionLoading}
                className="flex-1 py-3 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-2xl font-medium shadow-lg disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {actionLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                Gửi báo cáo
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── MatchCard Component ──────────────────────────────────────────────────────
function MatchCard({ match, isNew, onClick, onAction, timeAgo }) {
  const partner = match.partner || {};
  const initials = partner.fullName
    ?.split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <div
      className={`
        group relative cursor-pointer rounded-[28px] overflow-hidden
        glass transition-all duration-500 p-2
        hover:shadow-xl hover:-translate-y-1
        ${isNew
          ? "border-aura-pink border-[1.5px] shadow-[0_0_20px_rgba(255,92,154,0.15)]"
          : "border-white/40 hover:border-aura-dark/20"
        }
      `}
    >
      {/* Avatar */}
      <div className="aspect-[4/5] relative overflow-hidden rounded-[20px] bg-white/50" onClick={onClick}>
        {partner.avatarUrl ? (
          <img
            src={partner.avatarUrl}
            alt={partner.fullName}
            className="w-full h-full object-cover group-hover:scale-[1.03] transition-transform duration-700"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <span className="text-4xl font-semibold text-aura-pink/60">{initials}</span>
          </div>
        )}

        {/* Online indicator */}
        {partner.isOnline && (
          <div className="absolute top-3 right-3 w-3 h-3 bg-green-400 rounded-full shadow-[0_0_8px_rgba(74,222,128,0.8)]" />
        )}

        {/* New match badge */}
        {isNew && (
          <div className="absolute top-3 left-3 px-3 py-1 bg-white/70 backdrop-blur-md rounded-full border border-white/50">
            <span className="text-[10px] font-bold text-aura-pink tracking-widest uppercase">Mới</span>
          </div>
        )}

        {/* Chat button overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/10 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-end justify-center pb-5">
          <div className="flex items-center gap-2 px-5 py-2.5 bg-white/30 backdrop-blur-md border border-white/40 rounded-full text-sm font-semibold text-white shadow-lg">
            <MessageCircle className="w-4 h-4" />
            Nhắn tin
          </div>
        </div>
      </div>

      {/* Info */}
      <div className="p-4">
        <div className="flex items-center justify-between">
          <p className="font-semibold text-gray-900 text-base truncate flex-1 tracking-tight">{partner.fullName}</p>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onAction?.();
            }}
            className="w-8 h-8 rounded-full hover:bg-white flex items-center justify-center transition-colors opacity-0 group-hover:opacity-100"
          >
            <MoreVertical className="w-4 h-4 text-gray-400" />
          </button>
        </div>
        {match.lastMessage ? (
          <p className={`text-sm mt-1 truncate ${match.unreadCount > 0 ? "font-semibold text-aura-dark" : "text-gray-500"}`}>
            {match.lastMessage.isMine ? "Bạn: " : ""}
            {match.lastMessage.content} <span className="text-gray-400 text-xs ml-1">· {timeAgo(match.lastMessage.sentAt)}</span>
          </p>
        ) : (
          <p className="text-sm text-aura-pink mt-1 italic">Hãy gửi lời chào 👋</p>
        )}
        {match.unreadCount > 0 && (
          <div className="absolute top-4 right-4 z-10 w-5 h-5 bg-aura-pink text-white text-[10px] font-bold rounded-full flex items-center justify-center shadow-md">
            {match.unreadCount}
          </div>
        )}
      </div>
    </div>
  );
}
