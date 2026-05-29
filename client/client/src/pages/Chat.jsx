import { useEffect, useRef, useState, useCallback } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  ArrowLeft,
  Heart,
  MapPin,
  MessageCircle,
  Send,
  Check,
  CheckCheck,
  Loader2,
} from "lucide-react";
import api from "../services/api";
import * as signalR from "@microsoft/signalr";

// SignalR cần URL gốc (không có /api), lấy từ baseURL của api instance
const SIGNALR_BASE = api.defaults.baseURL.replace(/\/api\/?$/, "");

function getToken() {
  return localStorage.getItem("token") || sessionStorage.getItem("token");
}
function getMe() {
  try {
    const raw = localStorage.getItem("user") || sessionStorage.getItem("user");
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

// ─── MAIN COMPONENT ──────────────────────────────────────────────────────────
export default function Chat() {
  const { id: paramId } = useParams(); // /chat/:id  → id của người đang chat
  const navigate = useNavigate();
  const me = getMe();
  const token = getToken();

  const [matches, setMatches] = useState([]);
  const [matchesLoading, setMatchesLoading] = useState(true);

  const [activeChatId, setActiveChatId] = useState(paramId || null);
  const [activeUser, setActiveUser] = useState(null);

  const [messages, setMessages] = useState([]);
  const [msgLoading, setMsgLoading] = useState(false);
  const [text, setText] = useState("");
  const [sending, setSending] = useState(false);
  const [typing, setTyping] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState(new Set());

  const hubRef = useRef(null);
  const bottomRef = useRef(null);
  const typingTimer = useRef(null);
  const inputRef = useRef(null);

  // ── Guard + Init ─────────────────────────────────────────────────────────
  useEffect(() => {
    fetchMatches();
    connectHub();
    return () => hubRef.current?.stop();
  }, []);

  // ── Khi activeChatId thay đổi → load messages + mark seen ─────────────────
  useEffect(() => {
    if (!activeChatId) return;
    const user = matches.find((m) => m.id === activeChatId);
    setActiveUser(user || null);
    fetchMessages(activeChatId);
    markSeen(activeChatId);
    inputRef.current?.focus();
  }, [activeChatId, matches]);

  // ── Scroll xuống cuối khi có message mới ──────────────────────────────────
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // ── Update activeChatId khi URL thay đổi ──────────────────────────────────
  useEffect(() => {
    if (paramId && paramId !== activeChatId) setActiveChatId(paramId);
  }, [paramId]);

  // ─── Fetch matches (sidebar) ──────────────────────────────────────────────
  async function fetchMatches() {
    try {
      setMatchesLoading(true);
      const { data } = await api.get("/Match");
      setMatches(data);
      if (paramId && !activeChatId) setActiveChatId(paramId);
    } catch (err) {
      if (err.response?.status === 401) navigate("/login");
    } finally {
      setMatchesLoading(false);
    }
  }

  // ─── Fetch messages ───────────────────────────────────────────────────────
  async function fetchMessages(userId) {
    try {
      setMsgLoading(true);
      const { data } = await api.get(`/Messages/${userId}`);
      setMessages(data);
    } catch (err) {
      console.error("fetchMessages:", err);
    } finally {
      setMsgLoading(false);
    }
  }

  // ─── Mark seen ────────────────────────────────────────────────────────────
  async function markSeen(userId) {
    try {
      await api.put(`/Messages/seen/${userId}`);
    } catch {}
  }

  // ─── SignalR ──────────────────────────────────────────────────────────────
  function connectHub() {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNALR_BASE}/chatHub?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveMessage", (msg) => {
      setMessages((prev) => {
        // Tránh duplicate nếu đây là message mình vừa gửi
        if (prev.some((m) => m.id === msg.id)) return prev;
        return [...prev, msg];
      });
      // Nếu đang xem conversation này thì mark seen luôn
      if (msg.senderId === activeChatIdRef.current) {
        markSeen(msg.senderId);
      }
    });

    connection.on("Typing", (senderId) => {
      if (senderId === activeChatIdRef.current) {
        setTyping(true);
        clearTimeout(typingTimer.current);
        typingTimer.current = setTimeout(() => setTyping(false), 2500);
      }
    });

    connection.on("UserOnline", (userId) => {
      setOnlineUsers((prev) => new Set([...prev, userId]));
    });

    connection.on("UserOffline", (userId) => {
      setOnlineUsers((prev) => {
        const next = new Set(prev);
        next.delete(userId);
        return next;
      });
    });

    connection.start().catch(console.error);
    hubRef.current = connection;
  }

  // Ref để dùng trong closure của SignalR
  const activeChatIdRef = useRef(activeChatId);
  useEffect(() => { activeChatIdRef.current = activeChatId; }, [activeChatId]);

  // ─── Send message ─────────────────────────────────────────────────────────
  async function sendMessage(e) {
    e?.preventDefault();
    if (!text.trim() || !activeChatId || sending) return;

    const content = text.trim();
    setText("");

    // Optimistic update
    const optimistic = {
      id: `opt-${Date.now()}`,
      senderId: me?.id,
      receiverId: activeChatId,
      content,
      sentAt: new Date().toISOString(),
      isSeen: false,
      _optimistic: true,
    };
    setMessages((prev) => [...prev, optimistic]);

    try {
      setSending(true);
      const { data: saved } = await api.post("/Messages", {
        receiverId: activeChatId,
        content,
      });
      // Thay thế optimistic bằng bản thật
      setMessages((prev) =>
        prev.map((m) => (m.id === optimistic.id ? saved : m))
      );
    } catch {
      // Rollback optimistic
      setMessages((prev) => prev.filter((m) => m.id !== optimistic.id));
      setText(content);
    } finally {
      setSending(false);
    }
  }

  // ─── Typing indicator ─────────────────────────────────────────────────────
  function handleTextChange(e) {
    setText(e.target.value);
    if (hubRef.current?.state === signalR.HubConnectionState.Connected && activeChatId) {
      hubRef.current.invoke("Typing", activeChatId).catch(() => {});
    }
  }

  const isOnline = (userId) => onlineUsers.has(userId?.toString());

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className="h-screen flex bg-white overflow-hidden">
      {/* ── Sidebar: danh sách match ── */}
      <aside className="w-80 flex-shrink-0 border-r border-gray-100 flex flex-col">
        {/* Sidebar header */}
        <div className="px-5 py-5 border-b border-gray-100">
          <div className="flex items-center justify-between mb-4">
            <button
              onClick={() => navigate("/dashboard")}
              className="flex items-center gap-2 text-[#6B7280] hover:text-[#FF5C9A] transition-colors text-sm"
            >
              <ArrowLeft className="w-4 h-4" />
              Dashboard
            </button>
            <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center">
              <MessageCircle className="w-4 h-4 text-white" />
            </div>
          </div>
          <h1 className="text-xl font-bold text-[#1F2937]">Tin nhắn</h1>
          <p className="text-sm text-[#6B7280] mt-0.5">{matches.length} kết nối</p>
        </div>

        {/* Match list */}
        <div className="flex-1 overflow-y-auto">
          {matchesLoading ? (
            <div className="flex justify-center py-10">
              <Loader2 className="w-6 h-6 animate-spin text-[#FF5C9A]" />
            </div>
          ) : matches.length === 0 ? (
            <div className="flex flex-col items-center py-12 px-6 gap-3 text-center">
              <Heart className="w-12 h-12 text-[#FF5C9A]/30" />
              <p className="text-sm text-[#6B7280]">Chưa có match nào. Hãy tiếp tục swipe!</p>
              <button
                onClick={() => navigate("/discover")}
                className="text-sm text-[#FF5C9A] font-medium hover:underline"
              >
                Khám phá ngay →
              </button>
            </div>
          ) : (
            matches.map((match) => (
              <MatchListItem
                key={match.id}
                match={match}
                active={activeChatId === match.id}
                online={isOnline(match.id)}
                onClick={() => {
                  setActiveChatId(match.id);
                  navigate(`/chat/${match.id}`, { replace: true });
                }}
              />
            ))
          )}
        </div>
      </aside>

      {/* ── Chat area ── */}
      <div className="flex-1 flex flex-col min-w-0">
        {activeChatId && activeUser ? (
          <>
            {/* Chat header */}
            <div className="px-6 py-4 border-b border-gray-100 flex items-center gap-4 bg-white/80 backdrop-blur-sm">
              <div className="relative">
                <Avatar user={activeUser} size={44} />
                {isOnline(activeUser.id) && (
                  <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white" />
                )}
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="font-semibold text-[#1F2937] truncate">{activeUser.fullName}</h2>
                <p className="text-xs text-[#6B7280]">
                  {typing
                    ? <span className="text-[#FF5C9A] animate-pulse">Đang nhập...</span>
                    : isOnline(activeUser.id)
                    ? "Đang online"
                    : activeUser.location || "Offline"}
                </p>
              </div>
            </div>

            {/* Messages */}
            <div className="flex-1 overflow-y-auto px-6 py-5 space-y-2">
              {msgLoading ? (
                <div className="flex justify-center py-10">
                  <Loader2 className="w-6 h-6 animate-spin text-[#FF5C9A]" />
                </div>
              ) : messages.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full gap-4 text-center">
                  <div className="w-20 h-20 rounded-full bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 flex items-center justify-center">
                    <Heart className="w-10 h-10 text-[#FF5C9A]/50 fill-[#FF5C9A]/20" />
                  </div>
                  <div>
                    <p className="font-semibold text-[#1F2937]">Đây là match mới của bạn!</p>
                    <p className="text-sm text-[#6B7280] mt-1">Hãy gửi tin nhắn đầu tiên 👋</p>
                  </div>
                </div>
              ) : (
                <>
                  {messages.map((msg, i) => {
                    const isMine = msg.senderId === me?.id;
                    const prev = messages[i - 1];
                    const showAvatar = !isMine && prev?.senderId !== msg.senderId;
                    return (
                      <MessageBubble
                        key={msg.id}
                        msg={msg}
                        isMine={isMine}
                        showAvatar={showAvatar}
                        otherUser={activeUser}
                        isLastMine={isMine && i === messages.length - 1}
                      />
                    );
                  })}
                  {/* Typing indicator bubble */}
                  {typing && (
                    <div className="flex items-end gap-2">
                      <Avatar user={activeUser} size={28} />
                      <div className="bg-gray-100 rounded-2xl rounded-bl-none px-4 py-3">
                        <div className="flex gap-1">
                          {[0,1,2].map(i => (
                            <span
                              key={i}
                              className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                              style={{ animationDelay: `${i * 0.15}s` }}
                            />
                          ))}
                        </div>
                      </div>
                    </div>
                  )}
                  <div ref={bottomRef} />
                </>
              )}
            </div>

            {/* Input */}
            <form
              onSubmit={sendMessage}
              className="px-6 py-4 border-t border-gray-100 flex items-end gap-3"
            >
              <div className="flex-1 relative">
                <textarea
                  ref={inputRef}
                  value={text}
                  onChange={handleTextChange}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      sendMessage();
                    }
                  }}
                  placeholder="Nhắn tin..."
                  rows={1}
                  className="w-full px-4 py-3 bg-gray-50 border-2 border-gray-100 rounded-2xl outline-none focus:border-[#FF5C9A] focus:bg-white transition-all resize-none text-[#1F2937] placeholder:text-[#9CA3AF] leading-relaxed max-h-32 overflow-y-auto"
                  style={{ minHeight: "48px" }}
                />
              </div>
              <button
                type="submit"
                disabled={!text.trim() || sending}
                className={`
                  flex-shrink-0 w-12 h-12 rounded-2xl flex items-center justify-center transition-all
                  ${text.trim() && !sending
                    ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] shadow-lg shadow-pink-500/30 hover:scale-105 active:scale-95"
                    : "bg-gray-100 cursor-not-allowed"
                  }
                `}
              >
                {sending
                  ? <Loader2 className="w-5 h-5 text-white animate-spin" />
                  : <Send className={`w-5 h-5 ${text.trim() ? "text-white" : "text-gray-400"}`} />
                }
              </button>
            </form>
          </>
        ) : (
          /* Empty state khi chưa chọn conversation */
          <div className="flex-1 flex flex-col items-center justify-center gap-5 text-center px-8">
            <div className="relative">
              <div className="w-28 h-28 rounded-full bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 flex items-center justify-center">
                <MessageCircle className="w-14 h-14 text-[#FF5C9A]/30" />
              </div>
              <div className="absolute -top-1 -right-1 w-10 h-10 bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] rounded-full flex items-center justify-center shadow-lg">
                <Heart className="w-5 h-5 text-white fill-white" />
              </div>
            </div>
            <div>
              <h2 className="text-2xl font-bold text-[#1F2937]">Chọn một cuộc trò chuyện</h2>
              <p className="text-[#6B7280] mt-2 max-w-xs">
                Chọn một match từ danh sách bên trái để bắt đầu trò chuyện
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// ─── SUB COMPONENTS ───────────────────────────────────────────────────────────

function Avatar({ user, size = 40 }) {
  const initials = user?.fullName
    ?.split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  if (user?.avatarUrl) {
    return (
      <img
        src={user.avatarUrl}
        alt={user.fullName}
        className="rounded-full object-cover flex-shrink-0"
        style={{ width: size, height: size }}
      />
    );
  }
  return (
    <div
      className="rounded-full bg-gradient-to-br from-[#FF5C9A]/20 to-[#C8B6FF]/20 flex items-center justify-center flex-shrink-0"
      style={{ width: size, height: size }}
    >
      <span
        className="font-semibold text-[#FF5C9A]"
        style={{ fontSize: size * 0.35 }}
      >
        {initials}
      </span>
    </div>
  );
}

function MatchListItem({ match, active, online, onClick }) {
  return (
    <button
      onClick={onClick}
      className={`
        w-full flex items-center gap-3 px-5 py-3.5 transition-all text-left
        ${active
          ? "bg-gradient-to-r from-[#FF5C9A]/8 to-[#C8B6FF]/5 border-r-2 border-[#FF5C9A]"
          : "hover:bg-gray-50"
        }
      `}
    >
      <div className="relative flex-shrink-0">
        <Avatar user={match} size={46} />
        {online && (
          <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white" />
        )}
      </div>
      <div className="flex-1 min-w-0">
        <p className={`font-medium text-sm truncate ${active ? "text-[#FF5C9A]" : "text-[#1F2937]"}`}>
          {match.fullName}
        </p>
        {match.location && (
          <p className="text-xs text-[#9CA3AF] flex items-center gap-1 mt-0.5 truncate">
            <MapPin className="w-3 h-3 flex-shrink-0" />
            {match.location}
          </p>
        )}
      </div>
    </button>
  );
}

function MessageBubble({ msg, isMine, showAvatar, otherUser, isLastMine }) {
  const time = new Date(msg.sentAt).toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
  });

  return (
    <div className={`flex items-end gap-2 ${isMine ? "flex-row-reverse" : ""}`}>
      {/* Avatar của người kia */}
      {!isMine && (
        <div className="w-7 flex-shrink-0">
          {showAvatar && <Avatar user={otherUser} size={28} />}
        </div>
      )}

      <div className={`flex flex-col gap-1 max-w-[65%] ${isMine ? "items-end" : "items-start"}`}>
        <div
          className={`
            px-4 py-2.5 rounded-2xl text-sm leading-relaxed break-words
            ${isMine
              ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white rounded-br-none"
              : "bg-gray-100 text-[#1F2937] rounded-bl-none"
            }
            ${msg._optimistic ? "opacity-70" : "opacity-100"}
          `}
        >
          {msg.content}
        </div>

        {/* Time + seen status */}
        <div className="flex items-center gap-1 px-1">
          <span className="text-[10px] text-[#9CA3AF]">{time}</span>
          {isMine && isLastMine && (
            msg.isSeen
              ? <CheckCheck className="w-3 h-3 text-[#FF5C9A]" />
              : <Check className="w-3 h-3 text-[#9CA3AF]" />
          )}
        </div>
      </div>
    </div>
  );
}
