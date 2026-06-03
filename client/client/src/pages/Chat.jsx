import { useEffect, useRef, useState, useCallback } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  ArrowLeft,
  Heart,
  Image,
  MapPin,
  MessageCircle,
  Send,
  Check,
  CheckCheck,
  Loader2,
  X,
} from "lucide-react";
import api from "../services/api";
import * as signalR from "@microsoft/signalr";

// SignalR URL gốc (bỏ /api)
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
  const { id: paramId } = useParams();
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

  // Upload ảnh
  const [imagePreview, setImagePreview] = useState(null); // { file, url }
  const [uploadingImage, setUploadingImage] = useState(false);
  const fileInputRef = useRef(null);

  const hubRef = useRef(null);
  const bottomRef = useRef(null);
  const typingTimer = useRef(null);
  const inputRef = useRef(null);
  const activeChatIdRef = useRef(activeChatId);

  useEffect(() => { activeChatIdRef.current = activeChatId; }, [activeChatId]);

  // ── Init ──────────────────────────────────────────────────────────────────
  useEffect(() => {
    fetchMatches();
    connectHub();
    return () => hubRef.current?.stop();
  }, []);

  useEffect(() => {
    if (!activeChatId) return;
    const matchWrap = matches.find((m) => m.partner?.id === activeChatId);
    setActiveUser(matchWrap?.partner || null);
    fetchMessages(activeChatId);
    markSeen(activeChatId);
    inputRef.current?.focus();
  }, [activeChatId, matches]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  useEffect(() => {
    if (paramId && paramId !== activeChatId) setActiveChatId(paramId);
  }, [paramId]);

  // ─── Fetch matches ────────────────────────────────────────────────────────
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
        if (prev.some((m) => m.id === msg.id)) return prev;
        return [...prev, msg];
      });
      // Auto mark seen nếu đang xem conversation này
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

    // ✅ Nhận sự kiện đã đọc realtime — cập nhật tick
    connection.on("MessagesSeen", ({ byUserId, seenAt }) => {
      if (byUserId === activeChatIdRef.current) {
        setMessages((prev) =>
          prev.map((m) =>
            m.senderId === me?.id && !m.isSeen
              ? { ...m, isSeen: true, seenAt }
              : m
          )
        );
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

  // ─── Send text message ────────────────────────────────────────────────────
  async function sendMessage(e) {
    e?.preventDefault();
    if (!text.trim() || !activeChatId || sending) return;

    const content = text.trim();
    setText("");

    const optimistic = {
      id: `opt-${Date.now()}`,
      senderId: me?.id,
      receiverId: activeChatId,
      content,
      imageUrl: null,
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
      setMessages((prev) =>
        prev.map((m) => (m.id === optimistic.id ? saved : m))
      );
    } catch {
      setMessages((prev) => prev.filter((m) => m.id !== optimistic.id));
      setText(content);
    } finally {
      setSending(false);
    }
  }

  // ─── Send image message ───────────────────────────────────────────────────
  async function sendImage() {
    if (!imagePreview || !activeChatId || uploadingImage) return;

    const optimistic = {
      id: `opt-img-${Date.now()}`,
      senderId: me?.id,
      receiverId: activeChatId,
      content: "",
      imageUrl: imagePreview.url, // preview tạm
      sentAt: new Date().toISOString(),
      isSeen: false,
      _optimistic: true,
    };
    setMessages((prev) => [...prev, optimistic]);
    setImagePreview(null);

    try {
      setUploadingImage(true);
      const formData = new FormData();
      formData.append("receiverId", activeChatId);
      formData.append("file", imagePreview.file);

      const { data: saved } = await api.post("/Messages/image", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setMessages((prev) =>
        prev.map((m) => (m.id === optimistic.id ? saved : m))
      );
    } catch {
      setMessages((prev) => prev.filter((m) => m.id !== optimistic.id));
    } finally {
      setUploadingImage(false);
    }
  }

  // ─── Chọn ảnh ─────────────────────────────────────────────────────────────
  function handleFileSelect(e) {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 10 * 1024 * 1024) {
      alert("Ảnh không được vượt quá 10MB");
      return;
    }
    const url = URL.createObjectURL(file);
    setImagePreview({ file, url });
    e.target.value = "";
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
    <div className="h-screen flex bg-aura-bg font-sans overflow-hidden relative">
      {/* Ambient bg */}
      <div className="absolute inset-0 pointer-events-none z-0 overflow-hidden">
        <div className="absolute top-[-10%] right-[-5%] w-[800px] h-[800px] bg-aura-pink/10 rounded-full blur-[120px] mix-blend-multiply" />
        <div className="absolute bottom-[-10%] left-[-5%] w-[600px] h-[600px] bg-aura-blue/10 rounded-full blur-[100px] mix-blend-multiply" />
      </div>

      {/* ── Sidebar: danh sách match ── */}
      <aside className="w-80 flex-shrink-0 flex flex-col z-10 glass border-r border-white/50 shadow-xl">
        <div className="px-5 py-6 border-b border-white/40">
          <div className="flex items-center justify-between mb-5">
            <button
              onClick={() => navigate("/dashboard")}
              className="flex items-center gap-2 text-gray-500 hover:text-aura-dark transition-colors text-sm font-medium"
            >
              <ArrowLeft className="w-4 h-4" />
              Dashboard
            </button>
            <div className="w-10 h-10 rounded-xl bg-aura-dark flex items-center justify-center shadow-md">
              <MessageCircle className="w-5 h-5 text-white" />
            </div>
          </div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">Tin nhắn</h1>
          <p className="text-sm text-gray-500 mt-1">{matches.length} kết nối</p>
        </div>

        <div className="flex-1 overflow-y-auto custom-scrollbar">
          {matchesLoading ? (
            <div className="flex justify-center py-10">
              <Loader2 className="w-6 h-6 animate-spin text-aura-pink" />
            </div>
          ) : matches.length === 0 ? (
            <div className="flex flex-col items-center py-12 px-6 gap-4 text-center">
              <div className="w-16 h-16 rounded-full bg-white/50 flex items-center justify-center shadow-sm">
                <Heart className="w-8 h-8 text-aura-pink/50" />
              </div>
              <p className="text-sm text-gray-500">Chưa có match nào. Hãy tiếp tục vuốt!</p>
              <button
                onClick={() => navigate("/discover")}
                className="text-sm text-aura-dark font-semibold hover:underline"
              >
                Khám phá ngay →
              </button>
            </div>
          ) : (
            matches.map((match) => (
              <MatchListItem
                key={match.partner?.id}
                user={match.partner}
                active={activeChatId === match.partner?.id}
                online={isOnline(match.partner?.id)}
                onClick={() => {
                  setActiveChatId(match.partner?.id);
                  navigate(`/chat/${match.partner?.id}`, { replace: true });
                }}
              />
            ))
          )}
        </div>
      </aside>

      {/* ── Chat area ── */}
      <div className="flex-1 flex flex-col min-w-0 z-10 bg-white/30 backdrop-blur-3xl">
        {activeChatId && activeUser ? (
          <>
            {/* Chat header */}
            <div className="px-6 py-5 border-b border-white/40 flex items-center gap-4 bg-white/40 backdrop-blur-xl shadow-sm">
              <div className="relative">
                <Avatar user={activeUser} size={48} />
                {isOnline(activeUser.id) && (
                  <span className="absolute bottom-0 right-0 w-3.5 h-3.5 bg-green-400 rounded-full border-2 border-white shadow-sm" />
                )}
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="font-semibold text-lg text-gray-900 tracking-tight truncate">{activeUser.fullName}</h2>
                <p className="text-sm text-gray-500">
                  {typing
                    ? <span className="text-aura-pink font-medium animate-pulse">Đang soạn tin...</span>
                    : isOnline(activeUser.id)
                    ? "Đang trực tuyến"
                    : activeUser.location || "Ngoại tuyến"}
                </p>
              </div>
            </div>

            {/* Messages */}
            <div className="flex-1 overflow-y-auto px-6 py-6 space-y-4 custom-scrollbar">
              {msgLoading ? (
                <div className="flex justify-center py-10">
                  <Loader2 className="w-6 h-6 animate-spin text-aura-pink" />
                </div>
              ) : messages.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full gap-5 text-center">
                  <div className="w-24 h-24 rounded-full bg-white/60 shadow-lg border border-white/50 flex items-center justify-center">
                    <Heart className="w-12 h-12 text-aura-pink/50 fill-aura-pink/20" />
                  </div>
                  <div>
                    <p className="text-xl font-semibold text-gray-900 tracking-tight">Tương hợp mới!</p>
                    <p className="text-gray-500 mt-2">Bắt đầu câu chuyện với {activeUser.fullName} ngay.</p>
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
                  {/* Typing bubble */}
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

            {/* Image preview bar */}
            {imagePreview && (
              <div className="px-6 py-3 border-t border-white/40 bg-white/50 backdrop-blur-md flex items-center gap-4 shadow-[0_-4px_10px_rgba(0,0,0,0.02)]">
                <div className="relative group">
                  <img
                    src={imagePreview.url}
                    alt="preview"
                    className="w-20 h-20 object-cover rounded-[16px] shadow-sm"
                  />
                  <button
                    onClick={() => setImagePreview(null)}
                    className="absolute -top-2 -right-2 w-6 h-6 bg-gray-900 text-white rounded-full flex items-center justify-center hover:bg-red-500 transition-colors opacity-0 group-hover:opacity-100 shadow-md"
                  >
                    <X className="w-3 h-3" />
                  </button>
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-gray-900">Sẵn sàng gửi ảnh</p>
                  <p className="text-xs text-gray-500 mt-0.5 truncate max-w-xs">{imagePreview.file.name}</p>
                </div>
                <button
                  onClick={sendImage}
                  disabled={uploadingImage}
                  className="btn-magnetic px-5 py-2.5 bg-aura-dark text-white text-sm rounded-xl font-medium hover:bg-gray-800 transition-colors disabled:opacity-60 flex items-center gap-2 shadow-md"
                >
                  {uploadingImage
                    ? <Loader2 className="w-4 h-4 animate-spin" />
                    : <Send className="w-4 h-4" />
                  }
                  Gửi ảnh
                </button>
              </div>
            )}

            {/* Input */}
            <form
              onSubmit={sendMessage}
              className="px-6 py-5 bg-white/60 backdrop-blur-xl border-t border-white/40 flex items-end gap-3 shadow-[0_-4px_20px_rgba(0,0,0,0.03)]"
            >
              {/* Nút chọn ảnh */}
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                onChange={handleFileSelect}
              />
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="flex-shrink-0 w-12 h-12 rounded-2xl bg-white border border-gray-200 hover:border-aura-dark hover:text-aura-dark flex items-center justify-center transition-all text-gray-400 btn-magnetic shadow-sm"
                title="Gửi ảnh"
              >
                <Image className="w-5 h-5" />
              </button>

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
                  placeholder="Nhập tin nhắn..."
                  rows={1}
                  className="w-full px-5 py-3.5 bg-white border border-gray-200 rounded-2xl outline-none focus:border-aura-dark focus:ring-4 focus:ring-aura-dark/5 transition-all resize-none text-gray-900 placeholder:text-gray-400 font-medium leading-relaxed max-h-32 overflow-y-auto shadow-sm"
                  style={{ minHeight: "52px" }}
                />
              </div>
              <button
                type="submit"
                disabled={!text.trim() || sending}
                className={`
                  flex-shrink-0 w-12 h-12 rounded-2xl flex items-center justify-center transition-all btn-magnetic
                  ${text.trim() && !sending
                    ? "bg-aura-dark text-white shadow-md shadow-gray-900/10"
                    : "bg-gray-100 text-gray-300 cursor-not-allowed"
                  }
                `}
              >
                {sending
                  ? <Loader2 className="w-5 h-5 animate-spin" />
                  : <Send className={`w-5 h-5`} />
                }
              </button>
            </form>
          </>
        ) : (
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

function MatchListItem({ user, active, online, onClick }) {
  if (!user) return null;
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
        <Avatar user={user} size={46} />
        {online && (
          <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 rounded-full border-2 border-white" />
        )}
      </div>
      <div className="flex-1 min-w-0">
        <p className={`font-medium text-sm truncate ${active ? "text-[#FF5C9A]" : "text-[#1F2937]"}`}>
          {user.fullName}
        </p>
        {user.location && (
          <p className="text-xs text-[#9CA3AF] flex items-center gap-1 mt-0.5 truncate">
            <MapPin className="w-3 h-3 flex-shrink-0" />
            {user.location}
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
      {!isMine && (
        <div className="w-8 flex-shrink-0">
          {showAvatar ? (
            <Avatar user={otherUser} size={32} />
          ) : (
            <div className="w-8" />
          )}
        </div>
      )}

      <div className={`flex flex-col gap-1.5 max-w-[70%] ${isMine ? "items-end" : "items-start"}`}>
        {/* Ảnh */}
        {msg.imageUrl && (
          <a href={msg.imageUrl} target="_blank" rel="noopener noreferrer">
            <img
              src={msg.imageUrl}
              alt="ảnh"
              className={`
                max-w-[260px] max-h-[300px] rounded-3xl object-cover cursor-pointer
                hover:opacity-95 transition-opacity shadow-sm
                ${msg._optimistic ? "opacity-60" : "opacity-100"}
                ${isMine ? "rounded-br-sm" : "rounded-bl-sm"}
              `}
            />
          </a>
        )}

        {/* Text */}
        {msg.content && (
          <div
            className={`
              px-5 py-3.5 rounded-3xl text-sm leading-relaxed break-words font-medium shadow-sm
              ${isMine
                ? "bg-aura-dark text-white rounded-br-sm"
                : "bg-white/80 backdrop-blur-xl text-gray-900 border border-white/60 rounded-bl-sm"
              }
              ${msg._optimistic ? "opacity-70" : "opacity-100"}
            `}
          >
            {msg.content}
          </div>
        )}

        {/* Time + seen tick */}
        <div className={`flex items-center gap-1.5 px-2 mt-0.5 ${isMine ? "flex-row-reverse" : ""}`}>
          <span className="text-[10px] font-medium text-gray-400">{time}</span>
          {isMine && isLastMine && (
            msg.isSeen
              ? <CheckCheck className="w-3.5 h-3.5 text-aura-pink" />
              : <Check className="w-3.5 h-3.5 text-gray-400" />
          )}
        </div>
      </div>
    </div>
  );
}
