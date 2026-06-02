import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  ArrowLeft,
  Camera,
  Check,
  ChevronRight,
  Edit3,
  Heart,
  Loader2,
  MapPin,
  Save,
  Sparkles,
  User,
  X,
  Briefcase,
  BookOpen,
  Wine,
  Cigarette,
  Plus,
  Star,
} from "lucide-react";
import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";

// ─── Constants ────────────────────────────────────────────────────────────────
const INTERESTS_OPTIONS = [
  "🎵 Âm nhạc", "🎬 Phim ảnh", "📚 Đọc sách", "🍳 Nấu ăn",
  "✈️ Du lịch", "🏋️ Thể thao", "🎮 Game", "📸 Nhiếp ảnh",
  "🎨 Nghệ thuật", "🌿 Thiên nhiên", "🐾 Thú cưng", "☕ Cà phê",
  "🍜 Ẩm thực", "🧘 Yoga", "🎭 Nghệ thuật biểu diễn", "🏊 Bơi lội",
  "🚴 Đạp xe", "🎸 Nhạc cụ", "💃 Khiêu vũ", "🌸 Làm vườn",
];

const LOOKING_FOR_OPTIONS = [
  { value: "serious", label: "💍 Quan hệ nghiêm túc" },
  { value: "casual", label: "😊 Gặp gỡ vui vẻ" },
  { value: "friendship", label: "🤝 Kết bạn" },
  { value: "unsure", label: "🤔 Chưa chắc" },
];

const LIFESTYLE_OPTIONS = [
  { value: "active", label: "⚡ Năng động" },
  { value: "homebody", label: "🏠 Thích ở nhà" },
  { value: "balanced", label: "⚖️ Cân bằng" },
  { value: "adventurous", label: "🧗 Ưa mạo hiểm" },
];

const ZODIAC_OPTIONS = [
  "♈ Bạch Dương", "♉ Kim Ngưu", "♊ Song Tử", "♋ Cự Giải",
  "♌ Sư Tử", "♍ Xử Nữ", "♎ Thiên Bình", "♏ Bọ Cạp",
  "♐ Nhân Mã", "♑ Ma Kết", "♒ Bảo Bình", "♓ Song Ngư",
];

const MBTI_OPTIONS = [
  "INTJ", "INTP", "ENTJ", "ENTP",
  "INFJ", "INFP", "ENFJ", "ENFP",
  "ISTJ", "ISFJ", "ESTJ", "ESFJ",
  "ISTP", "ISFP", "ESTP", "ESFP",
];

function calculateAge(dob) {
  if (!dob) return null;
  const diff = Date.now() - new Date(dob).getTime();
  return Math.floor(diff / (365.25 * 24 * 60 * 60 * 1000));
}

// ─── MAIN COMPONENT ───────────────────────────────────────────────────────────
export default function Profile() {
  const navigate = useNavigate();
  const { updateUser } = useAuth();

  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);
  const [activeTab, setActiveTab] = useState("basic"); // "basic" | "interests" | "preview"
  const [savedOk, setSavedOk] = useState(false);

  // Edit state — basic info
  const [form, setForm] = useState({
    fullName: "",
    bio: "",
    location: "",
    height: "",
    occupation: "",
    education: "",
    zodiac: "",
    mbti: "",
    smoking: "",
    drinking: "",
    lookingFor: "",
    lifestyle: "",
    interests: [],
  });

  const fileInputRef = useRef(null);

  // ── Load profile ─────────────────────────────────────────────────────────
  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const { data } = await api.get("/User/profile");
        setProfile(data);
        setForm({
          fullName: data.fullName || "",
          bio: data.bio || "",
          location: data.location || "",
          height: data.height || "",
          occupation: data.occupation || "",
          education: data.education || "",
          zodiac: data.zodiac || "",
          mbti: data.mbti || "",
          smoking: data.smoking || "",
          drinking: data.drinking || "",
          lookingFor: data.lookingFor || "",
          lifestyle: data.lifestyle || "",
          interests: data.interests || [],
        });
      } catch (err) {
        if (err.response?.status === 401) navigate("/login");
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [navigate]);

  // ── Gallery Photos ────────────────────────────────────────────────────────

  async function uploadPhoto(e) {
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = "";

    try {
      setUploadingAvatar(true);
      const formData = new FormData();
      formData.append("file", file);
      
      const { data: newPhoto } = await api.post("/User/photos", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      const updatedPhotos = [...(profile.photos || []), newPhoto];
      const updatedProfile = { 
        ...profile, 
        photos: updatedPhotos,
        avatarUrl: newPhoto.isMain ? newPhoto.url : profile.avatarUrl 
      };
      setProfile(updatedProfile);
      if (newPhoto.isMain) updateUser(updatedProfile);
    } catch (err) {
      alert(err.response?.data?.message || "Upload ảnh thất bại");
    } finally {
      setUploadingAvatar(false);
    }
  }

  async function deletePhoto(id) {
    if (!window.confirm("Bạn có chắc muốn xoá ảnh này?")) return;
    try {
      await api.delete(`/User/photos/${id}`);
      const updatedPhotos = profile.photos.filter((p) => p.id !== id);
      setProfile({ ...profile, photos: updatedPhotos });
    } catch (err) {
      alert(err.response?.data?.message || "Xoá ảnh thất bại");
    }
  }

  async function setMainPhoto(id) {
    try {
      await api.put(`/User/photos/${id}/setMain`);
      const updatedPhotos = profile.photos.map((p) => ({
        ...p,
        isMain: p.id === id,
      }));
      const newMainUrl = updatedPhotos.find((p) => p.id === id)?.url;
      const updatedProfile = { ...profile, photos: updatedPhotos, avatarUrl: newMainUrl };
      setProfile(updatedProfile);
      updateUser(updatedProfile);
    } catch (err) {
      alert(err.response?.data?.message || "Đặt ảnh chính thất bại");
    }
  }

  // ── Toggle interest ───────────────────────────────────────────────────────
  function toggleInterest(interest) {
    setForm((f) => ({
      ...f,
      interests: f.interests.includes(interest)
        ? f.interests.filter((i) => i !== interest)
        : f.interests.length < 10
        ? [...f.interests, interest]
        : f.interests,
    }));
  }

  // ── Save profile ──────────────────────────────────────────────────────────
  async function saveProfile() {
    try {
      setSaving(true);
      const { data } = await api.put("/User/profile", {
        fullName: form.fullName,
        bio: form.bio,
        location: form.location,
        height: form.height ? parseInt(form.height) : null,
        occupation: form.occupation,
        education: form.education,
        zodiac: form.zodiac,
        mbti: form.mbti,
        smoking: form.smoking,
        drinking: form.drinking,
        lookingFor: form.lookingFor,
        lifestyle: form.lifestyle,
        interests: form.interests,
      });

      const updated = { ...profile, ...data };
      setProfile(updated);
      updateUser(updated);

      setSavedOk(true);
      setTimeout(() => setSavedOk(false), 2500);
    } catch (err) {
      alert(err.response?.data?.message || "Lưu thất bại. Vui lòng thử lại!");
    } finally {
      setSaving(false);
    }
  }

  // ─── LOADING ──────────────────────────────────────────────────────────────
  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-pink-50 via-white to-purple-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center shadow-lg">
            <Loader2 className="w-8 h-8 text-white animate-spin" />
          </div>
          <p className="text-[#6B7280] text-sm">Đang tải hồ sơ...</p>
        </div>
      </div>
    );
  }

  const age = calculateAge(profile?.dateOfBirth);
  const completionScore = (() => {
    const fields = [
      form.fullName, form.bio, form.location, form.lookingFor,
      form.lifestyle, form.occupation, profile?.avatarUrl,
      form.interests.length > 0 ? "yes" : "",
    ];
    return Math.round((fields.filter(Boolean).length / fields.length) * 100);
  })();

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-gradient-to-br from-pink-50/50 via-white to-purple-50/50">
      {/* ── Header ── */}
      <header className="sticky top-0 z-40 bg-white/80 backdrop-blur-xl border-b border-gray-100">
        <div className="max-w-2xl mx-auto px-5 py-3.5 flex items-center justify-between">
          <button
            onClick={() => navigate("/dashboard")}
            className="flex items-center gap-2 text-[#6B7280] hover:text-[#FF5C9A] transition-colors text-sm"
          >
            <ArrowLeft className="w-4 h-4" />
            Dashboard
          </button>

          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-[#FF5C9A] to-[#C8B6FF] flex items-center justify-center">
              <User className="w-4 h-4 text-white" />
            </div>
            <span className="font-semibold text-[#1F2937]">Hồ sơ của tôi</span>
          </div>

          {/* Nút lưu */}
          <button
            onClick={saveProfile}
            disabled={saving}
            className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-sm font-semibold transition-all ${
              savedOk
                ? "bg-green-100 text-green-600"
                : "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white shadow-md shadow-pink-300/30 hover:opacity-90"
            } disabled:opacity-60`}
          >
            {saving ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : savedOk ? (
              <Check className="w-4 h-4" />
            ) : (
              <Save className="w-4 h-4" />
            )}
            {savedOk ? "Đã lưu!" : "Lưu"}
          </button>
        </div>
      </header>

      <div className="max-w-2xl mx-auto px-4 py-6 space-y-5">

        {/* ── Photo Gallery ── */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="font-bold text-[#1F2937]">Ảnh của bạn</h3>
              <p className="text-sm text-[#6B7280]">Thêm ảnh để hồ sơ nổi bật hơn</p>
            </div>
            <span className="text-sm font-bold text-[#FF5C9A]">
              {(profile?.photos?.length) || 0}/6
            </span>
          </div>
          
          <div className="grid grid-cols-3 gap-3">
            {[0, 1, 2, 3, 4, 5].map((index) => {
              const photo = profile?.photos?.[index];
              return (
                <div key={index} className="relative aspect-[3/4] rounded-2xl overflow-hidden bg-gray-50 border-2 border-dashed border-gray-200 group">
                  {photo ? (
                    <>
                      <img src={photo.url} alt="gallery" className="w-full h-full object-cover" />
                      {photo.isMain && (
                        <div className="absolute top-2 left-2 px-2 py-0.5 bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white text-[10px] font-bold rounded-lg shadow-sm">
                          MAIN
                        </div>
                      )}
                      
                      {/* Lớp phủ hiển thị nút hành động khi hover */}
                      <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                        {!photo.isMain && (
                          <button 
                            onClick={() => setMainPhoto(photo.id)} 
                            title="Đặt làm ảnh chính"
                            className="w-8 h-8 bg-white rounded-full flex items-center justify-center text-blue-500 hover:scale-110 transition-transform"
                          >
                            <Star className="w-4 h-4" />
                          </button>
                        )}
                        <button 
                          onClick={() => deletePhoto(photo.id)} 
                          title="Xoá ảnh"
                          className="w-8 h-8 bg-white rounded-full flex items-center justify-center text-red-500 hover:scale-110 transition-transform"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </>
                  ) : (
                    <button 
                      onClick={() => fileInputRef.current?.click()} 
                      disabled={uploadingAvatar}
                      className="w-full h-full flex flex-col items-center justify-center text-[#FF5C9A]/50 hover:text-[#FF5C9A] hover:bg-[#FF5C9A]/5 transition-colors disabled:opacity-50"
                    >
                      {uploadingAvatar ? (
                        <Loader2 className="w-6 h-6 animate-spin" />
                      ) : (
                        <Plus className="w-8 h-8" />
                      )}
                    </button>
                  )}
                </div>
              );
            })}
          </div>
          <input 
            ref={fileInputRef} 
            type="file" 
            accept="image/*" 
            className="hidden" 
            onChange={uploadPhoto} 
          />
        </div>

        {/* ── Thông tin cá nhân cơ bản ── */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100">
          <div className="flex items-start gap-5">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <h2 className="text-xl font-bold text-[#1F2937] truncate">
                  {form.fullName || "Tên của bạn"}
                </h2>
                {age && (
                  <span className="text-[#6B7280] text-sm">{age} tuổi</span>
                )}
              </div>
              {form.location && (
                <p className="text-sm text-[#9CA3AF] flex items-center gap-1 mb-4">
                  <MapPin className="w-3 h-3" />
                  {form.location}
                </p>
              )}

              {/* Completion bar */}
              <div className="space-y-1">
                <div className="flex justify-between items-center">
                  <span className="text-xs text-[#6B7280]">Độ hoàn thiện hồ sơ</span>
                  <span className={`text-xs font-bold ${completionScore >= 80 ? "text-green-500" : completionScore >= 50 ? "text-amber-500" : "text-[#FF5C9A]"}`}>
                    {completionScore}%
                  </span>
                </div>
                <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] rounded-full transition-all duration-500"
                    style={{ width: `${completionScore}%` }}
                  />
                </div>
                {completionScore < 80 && (
                  <p className="text-xs text-[#9CA3AF]">
                    Hồ sơ đầy đủ hơn → được xem nhiều hơn ✨
                  </p>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* ── Tabs ── */}
        <div className="bg-white rounded-2xl p-1.5 shadow-sm border border-gray-100 flex gap-1">
          {[
            { id: "basic", label: "📝 Cơ bản" },
            { id: "interests", label: "✨ Sở thích" },
            { id: "preview", label: "👁 Xem trước" },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex-1 py-2.5 rounded-xl text-sm font-medium transition-all ${
                activeTab === tab.id
                  ? "bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] text-white shadow-md"
                  : "text-[#6B7280] hover:text-[#1F2937]"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* ── Tab: Thông tin cơ bản ── */}
        {activeTab === "basic" && (
          <div className="space-y-4">
            {/* Tên & Bio */}
            <FormSection title="Thông tin cá nhân" icon={<User className="w-4 h-4" />}>
              <FormField label="Tên hiển thị">
                <input
                  type="text"
                  value={form.fullName}
                  onChange={(e) => setForm((f) => ({ ...f, fullName: e.target.value }))}
                  placeholder="Tên của bạn"
                  maxLength={50}
                  className="form-input"
                />
              </FormField>

              <FormField label={`Bio (${form.bio.length}/300)`}>
                <textarea
                  value={form.bio}
                  onChange={(e) => setForm((f) => ({ ...f, bio: e.target.value }))}
                  placeholder="Mô tả bản thân một cách thú vị... 😄"
                  maxLength={300}
                  rows={3}
                  className="form-input resize-none"
                />
              </FormField>

              <FormField label="Vị trí">
                <input
                  type="text"
                  value={form.location}
                  onChange={(e) => setForm((f) => ({ ...f, location: e.target.value }))}
                  placeholder="Hà Nội, Việt Nam"
                  className="form-input"
                />
              </FormField>
            </FormSection>

            {/* Chi tiết */}
            <FormSection title="Chi tiết" icon={<Edit3 className="w-4 h-4" />}>
              <FormField label="Chiều cao (cm)">
                <input
                  type="number"
                  value={form.height}
                  onChange={(e) => setForm((f) => ({ ...f, height: e.target.value }))}
                  placeholder="170"
                  min={140}
                  max={220}
                  className="form-input"
                />
              </FormField>

              <FormField label="Nghề nghiệp">
                <input
                  type="text"
                  value={form.occupation}
                  onChange={(e) => setForm((f) => ({ ...f, occupation: e.target.value }))}
                  placeholder="Kỹ sư, Sinh viên, Freelancer..."
                  className="form-input"
                />
              </FormField>

              <FormField label="Học vấn">
                <input
                  type="text"
                  value={form.education}
                  onChange={(e) => setForm((f) => ({ ...f, education: e.target.value }))}
                  placeholder="Đại học Bách Khoa..."
                  className="form-input"
                />
              </FormField>
            </FormSection>

            {/* Tính cách */}
            <FormSection title="Tính cách" icon={<Sparkles className="w-4 h-4" />}>
              <FormField label="Cung hoàng đạo">
                <ChipSelect
                  options={ZODIAC_OPTIONS}
                  value={form.zodiac}
                  onChange={(v) => setForm((f) => ({ ...f, zodiac: v }))}
                />
              </FormField>

              <FormField label="MBTI">
                <ChipSelect
                  options={MBTI_OPTIONS}
                  value={form.mbti}
                  onChange={(v) => setForm((f) => ({ ...f, mbti: v }))}
                  cols={4}
                />
              </FormField>
            </FormSection>

            {/* Lối sống */}
            <FormSection title="Lối sống" icon={<Heart className="w-4 h-4" />}>
              <FormField label="Tìm kiếm">
                <ChipSelect
                  options={LOOKING_FOR_OPTIONS.map((o) => o.label)}
                  value={LOOKING_FOR_OPTIONS.find((o) => o.value === form.lookingFor)?.label || ""}
                  onChange={(v) => {
                    const found = LOOKING_FOR_OPTIONS.find((o) => o.label === v);
                    setForm((f) => ({ ...f, lookingFor: found?.value || "" }));
                  }}
                />
              </FormField>

              <FormField label="Phong cách sống">
                <ChipSelect
                  options={LIFESTYLE_OPTIONS.map((o) => o.label)}
                  value={LIFESTYLE_OPTIONS.find((o) => o.value === form.lifestyle)?.label || ""}
                  onChange={(v) => {
                    const found = LIFESTYLE_OPTIONS.find((o) => o.label === v);
                    setForm((f) => ({ ...f, lifestyle: found?.value || "" }));
                  }}
                />
              </FormField>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="🚬 Hút thuốc">
                  <select
                    value={form.smoking}
                    onChange={(e) => setForm((f) => ({ ...f, smoking: e.target.value }))}
                    className="form-input"
                  >
                    <option value="">Chưa chọn</option>
                    <option value="never">Không bao giờ</option>
                    <option value="sometimes">Thỉnh thoảng</option>
                    <option value="regularly">Thường xuyên</option>
                  </select>
                </FormField>

                <FormField label="🍺 Uống bia rượu">
                  <select
                    value={form.drinking}
                    onChange={(e) => setForm((f) => ({ ...f, drinking: e.target.value }))}
                    className="form-input"
                  >
                    <option value="">Chưa chọn</option>
                    <option value="never">Không bao giờ</option>
                    <option value="sometimes">Thỉnh thoảng</option>
                    <option value="socially">Khi có tiệc</option>
                    <option value="regularly">Thường xuyên</option>
                  </select>
                </FormField>
              </div>
            </FormSection>
          </div>
        )}

        {/* ── Tab: Sở thích ── */}
        {activeTab === "interests" && (
          <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h3 className="font-bold text-[#1F2937]">Sở thích của bạn</h3>
                <p className="text-sm text-[#6B7280] mt-0.5">Chọn tối đa 10 sở thích</p>
              </div>
              <span className="text-sm font-bold text-[#FF5C9A]">
                {form.interests.length}/10
              </span>
            </div>

            <div className="flex flex-wrap gap-2">
              {INTERESTS_OPTIONS.map((interest) => {
                const selected = form.interests.includes(interest);
                return (
                  <button
                    key={interest}
                    onClick={() => toggleInterest(interest)}
                    disabled={!selected && form.interests.length >= 10}
                    className={`px-3.5 py-2 rounded-2xl text-sm font-medium border-2 transition-all ${
                      selected
                        ? "bg-gradient-to-r from-[#FF5C9A]/10 to-[#C8B6FF]/10 border-[#FF5C9A] text-[#FF5C9A] scale-105"
                        : "border-gray-100 text-[#6B7280] hover:border-gray-200 disabled:opacity-40 disabled:cursor-not-allowed"
                    }`}
                  >
                    {selected && <span className="mr-1">✓</span>}
                    {interest}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* ── Tab: Xem trước profile card ── */}
        {activeTab === "preview" && (
          <div className="flex flex-col items-center gap-4">
            <p className="text-sm text-[#6B7280]">Đây là cách người khác thấy bạn</p>
            <div className="w-full max-w-sm">
              <ProfileCard profile={profile} form={form} age={age} />
            </div>
          </div>
        )}

      </div>

      {/* Inline CSS */}
      <style>{`
        .form-input {
          width: 100%;
          padding: 10px 14px;
          border: 2px solid #F3F4F6;
          border-radius: 12px;
          outline: none;
          font-size: 14px;
          color: #1F2937;
          background: #FAFAFA;
          transition: border-color 0.2s, background 0.2s;
        }
        .form-input:focus {
          border-color: #FF5C9A;
          background: white;
        }
        .form-input::placeholder { color: #9CA3AF; }
      `}</style>
    </div>
  );
}

// ─── SUB COMPONENTS ───────────────────────────────────────────────────────────

function FormSection({ title, icon, children }) {
  return (
    <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 space-y-4">
      <div className="flex items-center gap-2 pb-2 border-b border-gray-50">
        <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-[#FF5C9A]/10 to-[#C8B6FF]/10 flex items-center justify-center text-[#FF5C9A]">
          {icon}
        </div>
        <h3 className="font-bold text-[#1F2937] text-sm">{title}</h3>
      </div>
      {children}
    </div>
  );
}

function FormField({ label, children }) {
  return (
    <div className="space-y-1.5">
      <label className="block text-xs font-semibold text-[#6B7280] uppercase tracking-wide">
        {label}
      </label>
      {children}
    </div>
  );
}

function ChipSelect({ options, value, onChange, cols = 2 }) {
  return (
    <div className={`grid gap-2`} style={{ gridTemplateColumns: `repeat(${cols}, 1fr)` }}>
      {options.map((opt) => (
        <button
          key={opt}
          type="button"
          onClick={() => onChange(value === opt ? "" : opt)}
          className={`py-2 px-2 rounded-xl text-xs font-medium border-2 transition-all text-center ${
            value === opt
              ? "border-[#FF5C9A] bg-[#FF5C9A]/10 text-[#FF5C9A]"
              : "border-gray-100 text-[#6B7280] hover:border-gray-200"
          }`}
        >
          {opt}
        </button>
      ))}
    </div>
  );
}

// Preview card giống card trong Discover
function ProfileCard({ profile, form, age }) {
  const GRADIENTS = ["from-pink-200 to-purple-300", "from-rose-200 to-pink-300"];
  const grad = GRADIENTS[0];

  return (
    <div className="relative rounded-3xl overflow-hidden shadow-2xl aspect-[3/4] bg-gray-100">
      {profile?.avatarUrl ? (
        <img
          src={profile.avatarUrl}
          alt="avatar"
          className="w-full h-full object-cover"
        />
      ) : (
        <div className={`w-full h-full bg-gradient-to-br ${grad} flex items-center justify-center`}>
          <span className="text-8xl font-bold text-white/60">
            {form.fullName?.[0]?.toUpperCase() || "?"}
          </span>
        </div>
      )}

      {/* Gradient overlay */}
      <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

      {/* Info overlay */}
      <div className="absolute bottom-0 left-0 right-0 p-6 text-white">
        <div className="flex items-end justify-between mb-2">
          <div>
            <h2 className="text-2xl font-bold">
              {form.fullName || "Tên của bạn"}
              {age && <span className="font-normal text-white/80 ml-2">{age}</span>}
            </h2>
            {form.location && (
              <p className="text-sm text-white/70 flex items-center gap-1 mt-0.5">
                <MapPin className="w-3 h-3" />
                {form.location}
              </p>
            )}
          </div>
          {profile?.isVerified && (
            <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
              <Check className="w-4 h-4 text-white" />
            </div>
          )}
        </div>

        {form.bio && (
          <p className="text-sm text-white/80 line-clamp-2 mb-3">{form.bio}</p>
        )}

        {/* Tags */}
        <div className="flex flex-wrap gap-1.5">
          {form.lookingFor && (
            <span className="px-2.5 py-1 bg-white/20 backdrop-blur-sm rounded-full text-xs">
              {LOOKING_FOR_OPTIONS.find((o) => o.value === form.lookingFor)?.label}
            </span>
          )}
          {form.occupation && (
            <span className="px-2.5 py-1 bg-white/20 backdrop-blur-sm rounded-full text-xs flex items-center gap-1">
              <Briefcase className="w-3 h-3" />
              {form.occupation}
            </span>
          )}
          {form.zodiac && (
            <span className="px-2.5 py-1 bg-white/20 backdrop-blur-sm rounded-full text-xs">
              {form.zodiac}
            </span>
          )}
          {form.interests.slice(0, 3).map((i) => (
            <span key={i} className="px-2.5 py-1 bg-white/20 backdrop-blur-sm rounded-full text-xs">
              {i}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}
