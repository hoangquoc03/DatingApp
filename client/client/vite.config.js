import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,       // 🔹 Ép buộc Vite luôn luôn chạy trên cổng 5173
    strictPort: true, // Nếu cổng 5173 bị ứng dụng khác chiếm, Vite sẽ báo lỗi ngay chứ không tự đổi sang 5174
    host: true,       // Cho phép mở rộng lắng nghe các kết nối trong mạng LAN
  },
});