import { AnimatePresence, motion } from "framer-motion";
import { ChevronLeft, ChevronRight, Heart, Sparkles } from "lucide-react";
import { useState } from "react";

const TOTAL_STEPS = 6;

export default function App() {
  const [currentStep, setCurrentStep] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    looking_for: "",
    interests: [],
    lifestyle: "",
    values: [],
    distance: 25,
    vibe: "",
  });

  const updateFormData = (key, value) => {
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const handleNext = () => {
    if (currentStep === TOTAL_STEPS) {
      setIsLoading(true);
      setTimeout(() => {
        // Redirect to dashboard sau khi loading
        console.log("Survey completed!", formData);
      }, 3000);
    } else {
      setCurrentStep((prev) => prev + 1);
    }
  };

  const handleBack = () => {
    if (currentStep > 0) {
      setCurrentStep((prev) => prev - 1);
    }
  };

  const progress = (currentStep / TOTAL_STEPS) * 100;

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <div className="min-h-screen relative overflow-hidden">
      {/* Gradient Background */}
      <div className="absolute inset-0 bg-gradient-to-br from-white via-pink-50/30 to-purple-50/20">
        <div className="absolute top-20 left-20 w-96 h-96 bg-[#FF5C9A]/10 rounded-full blur-3xl" />
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-[#C8B6FF]/10 rounded-full blur-3xl" />
      </div>

      {/* Content */}
      <div className="relative z-10">
        {/* Header */}
        <header className="px-8 py-6 flex items-center justify-between max-w-7xl mx-auto">
          <div className="flex items-center gap-3">
            <Heart className="w-8 h-8 fill-[#FF5C9A] text-[#FF5C9A]" />
            <span className="text-2xl font-semibold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
              Aura Dating
            </span>
          </div>

          {currentStep > 0 && (
            <div className="flex items-center gap-4 min-w-[200px]">
              <span className="text-sm text-[#6B7280]">
                Bước {currentStep} / {TOTAL_STEPS}
              </span>
              <div className="w-32 h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-gradient-to-r from-[#FF5C9A] to-[#FF8FB8] transition-all duration-300"
                  style={{ width: `${progress}%` }}
                ></div>
              </div>
            </div>
          )}
        </header>

        {/* Main Survey Card */}
        <div className="flex items-center justify-center px-4 py-12">
          <motion.div
            className="w-full max-w-3xl bg-white rounded-[32px] shadow-2xl shadow-pink-200/20 p-12"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
          >
            <AnimatePresence mode="wait">
              {currentStep === 0 && (
                <WelcomeStep key="welcome" onStart={handleNext} />
              )}
              {currentStep === 1 && (
                <Step1LookingFor
                  key="step1"
                  value={formData.looking_for}
                  onChange={(v) => updateFormData("looking_for", v)}
                />
              )}
              {currentStep === 2 && (
                <Step2Interests
                  key="step2"
                  value={formData.interests}
                  onChange={(v) => updateFormData("interests", v)}
                />
              )}
              {currentStep === 3 && (
                <Step3Lifestyle
                  key="step3"
                  value={formData.lifestyle}
                  onChange={(v) => updateFormData("lifestyle", v)}
                />
              )}
              {currentStep === 4 && (
                <Step4Values
                  key="step4"
                  value={formData.values}
                  onChange={(v) => updateFormData("values", v)}
                />
              )}
              {currentStep === 5 && (
                <Step5Distance
                  key="step5"
                  value={formData.distance}
                  onChange={(v) => updateFormData("distance", v)}
                />
              )}
              {currentStep === 6 && (
                <Step6Vibe
                  key="step6"
                  value={formData.vibe}
                  onChange={(v) => updateFormData("vibe", v)}
                />
              )}
            </AnimatePresence>

            {/* Navigation */}
            {currentStep > 0 && (
              <motion.div
                className="flex items-center justify-between mt-12 pt-8 border-t border-gray-100"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.3 }}
              >
                <button
                  onClick={handleBack}
                  className="flex items-center gap-2 px-4 py-2 rounded-xl text-[#6B7280] hover:text-[#1F2937] hover:bg-gray-100 transition-all"
                >
                  <ChevronLeft className="w-4 h-4" />
                  Quay lại
                </button>

                <button
                  onClick={handleNext}
                  className="
    group relative flex items-center justify-center gap-2
    px-8 py-3 rounded-xl font-medium text-white
    bg-gradient-to-r from-[#FF5C9A] to-[#FF8FB8]
    shadow-md shadow-pink-300/30
    transition-all duration-300 ease-out
    hover:shadow-lg hover:shadow-pink-300/50
    hover:scale-[1.03] active:scale-[0.98]
    focus:outline-none focus:ring-2 focus:ring-pink-300 focus:ring-offset-2
  "
                >
                  {currentStep === TOTAL_STEPS ? (
                    <>
                      <Heart className="w-4 h-4 transition-transform group-hover:scale-110" />
                      <span>Xem người phù hợp</span>
                    </>
                  ) : (
                    <>
                      <span>Tiếp tục</span>
                      <ChevronRight className="w-4 h-4 transition-transform group-hover:translate-x-1" />
                    </>
                  )}
                </button>
              </motion.div>
            )}
          </motion.div>
        </div>
      </div>
    </div>
  );
}

// Welcome Step
function WelcomeStep({ onStart }) {
  return (
    <motion.div
      className="text-center space-y-8"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.4 }}
    >
      <div className="space-y-4">
        <motion.div
          initial={{ scale: 0.8 }}
          animate={{ scale: 1 }}
          transition={{ delay: 0.2, type: "spring", stiffness: 200 }}
        >
          <Heart className="w-20 h-20 mx-auto fill-[#FF5C9A] text-[#FF5C9A] drop-shadow-lg" />
        </motion.div>

        <h1 className="text-5xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
          Hãy để Aura hiểu bạn hơn 💖
        </h1>

        <p className="text-xl text-[#6B7280] max-w-2xl mx-auto leading-relaxed">
          Trả lời vài câu hỏi ngắn để chúng tôi tìm những người thật sự phù hợp
          với bạn.
        </p>
      </div>

      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.4 }}
      >
        <button
          onClick={onStart}
          className="group inline-flex items-center justify-center gap-3 px-10 py-4 rounded-2xl bg-gradient-to-r from-[#FF5C9A] to-[#FF8FB8] text-white font-semibold text-lg shadow-lg shadow-pink-400/30 hover:shadow-2xl hover:shadow-pink-400/40 hover:-translate-y-0.5 active:translate-y-0 active:scale-[0.98] transition-all duration-300"
        >
          <span className="flex items-center justify-center w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm group-hover:rotate-12 transition-transform duration-300">
            <Sparkles className="w-5 h-5" />
          </span>

          <span>Bắt đầu khảo sát</span>

          <ChevronRight className="w-5 h-5 group-hover:translate-x-1 transition-transform duration-300" />
        </button>
      </motion.div>
    </motion.div>
  );
}

// Step 1: Looking For
function Step1LookingFor({ value, onChange }) {
  const options = [
    { id: "serious", label: "Mối quan hệ nghiêm túc", emoji: "💕" },
    { id: "explore", label: "Tìm hiểu trước rồi tính", emoji: "🌟" },
    { id: "friends", label: "Kết bạn mới", emoji: "🤝" },
    { id: "casual", label: "Hẹn hò vui vẻ", emoji: "✨" },
    { id: "unsure", label: "Chưa xác định", emoji: "🤔" },
  ];

  return (
    <StepContainer
      title="Bạn đang tìm kiếm điều gì?"
      subtitle="Hãy chọn mục đích chính của bạn khi tham gia Aura"
    >
      <div className="grid grid-cols-1 gap-4">
        {options.map((option, idx) => (
          <motion.button
            key={option.id}
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: idx * 0.1 }}
            onClick={() => onChange(option.id)}
            className={`
              p-6 rounded-2xl border-2 transition-all duration-300 text-left
              ${
                value === option.id
                  ? "border-[#FF5C9A] bg-gradient-to-r from-[#FFE4EF] to-[#FFF1F7] shadow-lg shadow-pink-200/50"
                  : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:shadow-md"
              }
            `}
          >
            <div className="flex items-center gap-4">
              <span className="text-3xl">{option.emoji}</span>
              <span className="text-lg font-medium text-[#1F2937]">
                {option.label}
              </span>
            </div>
          </motion.button>
        ))}
      </div>
    </StepContainer>
  );
}

// Step 2: Interests
function Step2Interests({ value, onChange }) {
  const interests = [
    { id: "travel", label: "Du lịch", emoji: "✈️" },
    { id: "music", label: "Âm nhạc", emoji: "🎵" },
    { id: "coffee", label: "Cà phê", emoji: "☕" },
    { id: "gym", label: "Gym", emoji: "💪" },
    { id: "gaming", label: "Game", emoji: "🎮" },
    { id: "reading", label: "Đọc sách", emoji: "📚" },
    { id: "movies", label: "Phim ảnh", emoji: "🎬" },
    { id: "cooking", label: "Nấu ăn", emoji: "🍳" },
    { id: "pets", label: "Thú cưng", emoji: "🐾" },
    { id: "tech", label: "Công nghệ", emoji: "💻" },
    { id: "fashion", label: "Thời trang", emoji: "👗" },
    { id: "art", label: "Nghệ thuật", emoji: "🎨" },
  ];

  const toggleInterest = (id) => {
    if (value.includes(id)) {
      onChange(value.filter((i) => i !== id));
    } else {
      onChange([...value, id]);
    }
  };

  return (
    <StepContainer
      title="Sở thích của bạn"
      subtitle="Chọn những gì bạn yêu thích (có thể chọn nhiều)"
    >
      <div className="grid grid-cols-3 gap-3">
        {interests.map((interest, idx) => (
          <motion.button
            key={interest.id}
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: idx * 0.05 }}
            onClick={() => toggleInterest(interest.id)}
            className={`
              p-4 rounded-xl border-2 transition-all duration-300
              ${
                value.includes(interest.id)
                  ? "border-[#FF5C9A] bg-gradient-to-br from-[#FFE4EF] to-[#FFF1F7] shadow-md shadow-pink-200/50 scale-105"
                  : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:scale-105"
              }
            `}
          >
            <div className="flex flex-col items-center gap-2">
              <span className="text-2xl">{interest.emoji}</span>
              <span className="text-sm font-medium text-[#1F2937]">
                {interest.label}
              </span>
            </div>
          </motion.button>
        ))}
      </div>

      {value.length > 0 && (
        <motion.p
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="text-center text-sm text-[#6B7280] mt-6"
        >
          ✨ Bạn đã chọn {value.length} sở thích
        </motion.p>
      )}
    </StepContainer>
  );
}

// Step 3: Lifestyle
function Step3Lifestyle({ value, onChange }) {
  const lifestyles = [
    {
      id: "introvert",
      label: "Hướng nội",
      desc: "Thích ở nhà, không gian riêng tư",
      emoji: "🏠",
    },
    {
      id: "balanced",
      label: "Cân bằng",
      desc: "Linh hoạt tùy tâm trạng",
      emoji: "⚖️",
    },
    {
      id: "extrovert",
      label: "Hướng ngoại",
      desc: "Thích ra ngoài, gặp gỡ bạn bè",
      emoji: "🎉",
    },
  ];

  return (
    <StepContainer title="Phong cách sống" subtitle="Bạn là người như thế nào?">
      <div className="grid grid-cols-1 gap-4">
        {lifestyles.map((lifestyle, idx) => (
          <motion.button
            key={lifestyle.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: idx * 0.1 }}
            onClick={() => onChange(lifestyle.id)}
            className={`
              p-6 rounded-2xl border-2 transition-all duration-300 text-left
              ${
                value === lifestyle.id
                  ? "border-[#FF5C9A] bg-gradient-to-r from-[#FFE4EF] to-[#FFF1F7] shadow-lg shadow-pink-200/50 scale-105"
                  : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:shadow-md"
              }
            `}
          >
            <div className="flex items-center gap-4">
              <span className="text-4xl">{lifestyle.emoji}</span>
              <div className="flex-1">
                <div className="text-lg font-semibold text-[#1F2937]">
                  {lifestyle.label}
                </div>
                <div className="text-sm text-[#6B7280] mt-1">
                  {lifestyle.desc}
                </div>
              </div>
            </div>
          </motion.button>
        ))}
      </div>
    </StepContainer>
  );
}

// Step 4: Values
function Step4Values({ value, onChange }) {
  const values = [
    { id: "honest", label: "Chân thành", emoji: "💎" },
    { id: "listener", label: "Biết lắng nghe", emoji: "👂" },
    { id: "ambitious", label: "Có chí hướng", emoji: "🚀" },
    { id: "funny", label: "Hài hước", emoji: "😄" },
    { id: "mature", label: "Trưởng thành", emoji: "🌱" },
    { id: "romantic", label: "Lãng mạn", emoji: "🌹" },
    { id: "caring", label: "Biết quan tâm", emoji: "🤗" },
    { id: "smart", label: "Thông minh", emoji: "🧠" },
    { id: "stable", label: "Ổn định", emoji: "⚓" },
  ];

  const toggleValue = (id) => {
    if (value.includes(id)) {
      onChange(value.filter((v) => v !== id));
    } else if (value.length < 3) {
      onChange([...value, id]);
    }
  };

  return (
    <StepContainer
      title="Điều bạn coi trọng ở đối phương"
      subtitle="Chọn tối đa 3 điều quan trọng nhất"
    >
      <div className="grid grid-cols-3 gap-3">
        {values.map((val, idx) => (
          <motion.button
            key={val.id}
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: idx * 0.05 }}
            onClick={() => toggleValue(val.id)}
            disabled={!value.includes(val.id) && value.length >= 3}
            className={`
              p-4 rounded-xl border-2 transition-all duration-300
              ${
                value.includes(val.id)
                  ? "border-[#FF5C9A] bg-gradient-to-br from-[#FFE4EF] to-[#FFF1F7] shadow-md shadow-pink-200/50 scale-105"
                  : value.length >= 3
                    ? "border-gray-200 bg-gray-50 opacity-50 cursor-not-allowed"
                    : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:scale-105"
              }
            `}
          >
            <div className="flex flex-col items-center gap-2">
              <span className="text-2xl">{val.emoji}</span>
              <span className="text-sm font-medium text-[#1F2937] text-center">
                {val.label}
              </span>
            </div>
          </motion.button>
        ))}
      </div>

      <motion.p
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className="text-center text-sm text-[#6B7280] mt-6"
      >
        {value.length === 0 && "Chọn tối đa 3 giá trị"}
        {value.length > 0 && value.length < 3 && `Đã chọn ${value.length}/3`}
        {value.length === 3 && "✨ Hoàn thành! Bạn đã chọn đủ 3 giá trị"}
      </motion.p>
    </StepContainer>
  );
}

// Step 5: Distance
function Step5Distance({ value, onChange }) {
  const distances = [
    { value: 5, label: "5km", desc: "Rất gần" },
    { value: 10, label: "10km", desc: "Trong khu vực" },
    { value: 25, label: "25km", desc: "Trong thành phố" },
    { value: 50, label: "50km", desc: "Rộng hơn" },
    { value: 999, label: "Toàn quốc", desc: "Không giới hạn" },
  ];

  return (
    <StepContainer
      title="Khoảng cách mong muốn"
      subtitle="Bạn muốn gặp người ở bán kính bao xa?"
    >
      <div className="space-y-8">
        <div className="grid grid-cols-5 gap-3">
          {distances.map((dist, idx) => (
            <motion.button
              key={dist.value}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: idx * 0.1 }}
              onClick={() => onChange(dist.value)}
              className={`
                p-4 rounded-xl border-2 transition-all duration-300
                ${
                  value === dist.value
                    ? "border-[#FF5C9A] bg-gradient-to-br from-[#FFE4EF] to-[#FFF1F7] shadow-lg shadow-pink-200/50 scale-110"
                    : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:scale-105"
                }
              `}
            >
              <div className="flex flex-col items-center gap-2">
                <span className="text-xl font-bold text-[#FF5C9A]">
                  {dist.label}
                </span>
                <span className="text-xs text-[#6B7280]">{dist.desc}</span>
              </div>
            </motion.button>
          ))}
        </div>

        <div className="text-center p-6 bg-gradient-to-r from-[#FFE4EF]/50 to-[#F0EBFF]/50 rounded-2xl">
          <p className="text-sm text-[#6B7280]">
            Bạn đã chọn bán kính:{" "}
            <span className="font-semibold text-[#FF5C9A]">
              {value === 999
                ? "Toàn quốc"
                : `${value}km - ${
                    distances.find((d) => d.value === value)?.desc
                  }`}
            </span>
          </p>
        </div>
      </div>
    </StepContainer>
  );
}

// Step 6: Vibe
function Step6Vibe({ value, onChange }) {
  const vibes = [
    { id: "gentle", label: "Dịu dàng", emoji: "🌸", color: "from-pink-100" },
    {
      id: "energetic",
      label: "Năng động",
      emoji: "⚡",
      color: "from-yellow-100",
    },
    { id: "cute", label: "Cute", emoji: "🎀", color: "from-pink-100" },
    {
      id: "edgy",
      label: "Cá tính",
      emoji: "🔥",
      color: "from-orange-100",
    },
    {
      id: "mature",
      label: "Trưởng thành",
      emoji: "👔",
      color: "from-blue-100",
    },
    {
      id: "mysterious",
      label: "Bí ẩn",
      emoji: "🌙",
      color: "from-purple-100",
    },
    {
      id: "elegant",
      label: "Sang trọng",
      emoji: "💎",
      color: "from-indigo-100",
    },
  ];

  return (
    <StepContainer
      title="Tạo vibe profile"
      subtitle="Bạn muốn gặp người có phong cách nào?"
    >
      <div className="grid grid-cols-2 gap-4">
        {vibes.map((vibe, idx) => (
          <motion.button
            key={vibe.id}
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: idx * 0.08 }}
            onClick={() => onChange(vibe.id)}
            className={`
              p-6 rounded-2xl border-2 transition-all duration-300
              ${
                value === vibe.id
                  ? "border-[#FF5C9A] bg-gradient-to-br from-[#FFE4EF] to-[#FFF1F7] shadow-xl shadow-pink-300/50 scale-105"
                  : "border-gray-200 bg-white hover:border-[#FF5C9A]/50 hover:shadow-md hover:scale-105"
              }
            `}
          >
            <div className="flex items-center gap-4">
              <span className="text-4xl">{vibe.emoji}</span>
              <span className="text-lg font-semibold text-[#1F2937]">
                {vibe.label}
              </span>
            </div>
          </motion.button>
        ))}
      </div>

      {value && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-8 text-center p-6 bg-gradient-to-r from-[#FFE4EF] to-[#F0EBFF] rounded-2xl"
        >
          <p className="text-sm text-[#6B7280]">
            ✨{" "}
            <span className="font-semibold text-[#FF5C9A]">85% hoàn tất</span> —
            Sắp đến lúc gặp đúng người rồi!
          </p>
        </motion.div>
      )}
    </StepContainer>
  );
}

// Step Container
function StepContainer({ title, subtitle, children }) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.4 }}
      className="space-y-8"
    >
      <div className="text-center space-y-3">
        <h2 className="text-3xl font-bold text-[#1F2937]">{title}</h2>
        <p className="text-lg text-[#6B7280]">{subtitle}</p>
      </div>
      <div>{children}</div>
    </motion.div>
  );
}

// Loading Screen
function LoadingScreen() {
  return (
    <div className="min-h-screen relative overflow-hidden flex items-center justify-center">
      {/* Animated Background */}
      <div className="absolute inset-0 bg-gradient-to-br from-white via-pink-50/30 to-purple-50/20">
        <motion.div
          className="absolute top-20 left-20 w-96 h-96 bg-[#FF5C9A]/20 rounded-full blur-3xl"
          animate={{
            scale: [1, 1.2, 1],
            opacity: [0.3, 0.5, 0.3],
          }}
          transition={{
            duration: 3,
            repeat: Infinity,
            ease: "easeInOut",
          }}
        />
        <motion.div
          className="absolute bottom-20 right-20 w-96 h-96 bg-[#C8B6FF]/20 rounded-full blur-3xl"
          animate={{
            scale: [1.2, 1, 1.2],
            opacity: [0.5, 0.3, 0.5],
          }}
          transition={{
            duration: 3,
            repeat: Infinity,
            ease: "easeInOut",
          }}
        />
      </div>

      {/* Loading Content */}
      <motion.div
        className="relative z-10 text-center space-y-8"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        <motion.div
          animate={{
            scale: [1, 1.1, 1],
          }}
          transition={{
            duration: 1.5,
            repeat: Infinity,
            ease: "easeInOut",
          }}
        >
          <Heart className="w-24 h-24 mx-auto fill-[#FF5C9A] text-[#FF5C9A] drop-shadow-2xl" />
        </motion.div>

        <div className="space-y-4">
          <h2 className="text-4xl font-bold bg-gradient-to-r from-[#FF5C9A] to-[#C8B6FF] bg-clip-text text-transparent">
            Đang tìm người phù hợp nhất cho bạn...
          </h2>
          <p className="text-lg text-[#6B7280]">
            AI đang phân tích sở thích và tính cách của bạn
          </p>
        </div>

        {/* Animated Dots */}
        <div className="flex items-center justify-center gap-2">
          {[0, 1, 2].map((i) => (
            <motion.div
              key={i}
              className="w-3 h-3 rounded-full bg-[#FF5C9A]"
              animate={{
                y: [0, -10, 0],
              }}
              transition={{
                duration: 0.6,
                repeat: Infinity,
                delay: i * 0.2,
              }}
            />
          ))}
        </div>
      </motion.div>
    </div>
  );
}
