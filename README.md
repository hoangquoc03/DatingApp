# Aura Dating - WPF & ASP.NET Core Dating Application

Aura Dating là một ứng dụng hẹn hò toàn diện được thiết kế với giao diện cao cấp (Premium "Aura Dating" White-Pink Theme), cung cấp các tính năng cốt lõi như Quẹt thẻ (Swipe), Tương hợp (Match), Chat Real-time, và Quản trị Hệ thống.

Dự án được chia làm 2 phần chính:
1. **DatingApp.Desktop**: Ứng dụng Client viết bằng WPF (.NET 8.0) theo kiến trúc MVVM.
2. **DatingApp.Server**: API Backend viết bằng ASP.NET Core (.NET 8.0) cung cấp RESTful APIs và SignalR Hub.

---

## 🚀 Công Nghệ Sử Dụng

### 1. Client (DatingApp.Desktop)
* **Framework:** WPF (Windows Presentation Foundation) trên nền tảng .NET 8.0.
* **Kiến trúc:** MVVM (Model-View-ViewModel).
* **Thư viện chính:**
  * `CommunityToolkit.Mvvm`: Hỗ trợ MVVM (ObservableProperty, RelayCommand, Messenger).
  * `MaterialDesignThemes`: Bộ UI Controls giao diện Material Design cao cấp dành cho WPF.
  * `Microsoft.AspNetCore.SignalR.Client`: Kết nối WebSocket cho tính năng Chat theo thời gian thực (Real-time Chat).
  * `System.Net.Http.Json`: Gọi API Backend nhanh chóng và tiện lợi.

### 2. Backend (DatingApp.Server)
* **Framework:** ASP.NET Core Web API (.NET 8.0).
* **Database & ORM:** Entity Framework Core.
* **Authentication & Security:**
  * JWT (JSON Web Tokens) cho xác thực.
  * BCrypt.Net cho mã hóa mật khẩu.
* **Real-time:** SignalR Core (Quản lý trạng thái Online, Typing, Gửi/Sửa/Thu hồi tin nhắn realtime).
* **Kiến trúc:** Phân chia rõ ràng Controllers, Services, Models, và Data Context.

---

## ⚙️ Hướng Dẫn Cài Đặt và Chạy Dự Án

### Yêu cầu hệ thống
* **Visual Studio 2022** (hỗ trợ .NET 8.0).
* **.NET 8.0 SDK**.
* SQL Server Express hoặc LocalDB (Mặc định sử dụng SQL Server LocalDB).

### Bước 1: Thiết lập Backend (Server)
1. Mở thư mục chứa mã nguồn bằng Visual Studio hoặc Terminal.
2. Mở file `appsettings.json` tại thư mục gốc và cấu hình lại chuỗi kết nối Database nếu cần (mặc định đã dùng LocalDB: `Server=(localdb)\\mssqllocaldb;Database=DatingAppDb;...`).
3. Mở Terminal / Developer Command Prompt, điều hướng vào thư mục chứa server (ví dụ: `cd server` hoặc thư mục gốc tùy cấu trúc).
4. Áp dụng Migration để tạo Database và Data Mẫu (Seed Data):
   ```bash
   dotnet ef database update --project server/DatingApp.Server.csproj
   ```
   *(Lưu ý: Dự án tự động có `DbInitializer` để seed một lượng lớn người dùng mẫu vào hệ thống).*
5. Khởi chạy Backend:
   ```bash
   cd server
   dotnet run
   ```
   Backend mặc định sẽ chạy ở cổng `http://localhost:5000` (hoặc cấu hình tùy chỉnh trong `launchSettings.json`).

### Bước 2: Thiết lập Client (Desktop)
1. Mở file giải pháp (`.sln`) trong Visual Studio.
2. Kiểm tra `HttpClient` trong `App.xaml.cs` (hoặc nơi đăng ký dịch vụ) để đảm bảo `BaseAddress` đang trỏ đúng về cổng của Backend (Ví dụ: `http://localhost:5000/`).
3. Set project `DatingApp.Desktop` làm **Startup Project**.
4. Chạy ứng dụng (F5).

---

## 🌟 Chức Năng Nổi Bật

1. **Giao Diện Premium (Aura Dating):** 
   * Tone màu chủ đạo Trắng - Hồng thời thượng, sạch sẽ và hiện đại giống Facebook Dating/Tinder.
   * Chuyển động mượt mà, hỗ trợ bo góc (CornerRadius) và DropShadow ấn tượng.
2. **Xác Thực (Authentication):**
   * Đăng nhập, Đăng ký nhiều bước.
   * Reset mật khẩu và Xác minh Email qua mã OTP.
3. **Quẹt Thẻ (Swipe & Discover):**
   * Hiển thị thông tin hồ sơ chi tiết (Sở thích, MBTI, Chiều cao, Học vấn,...).
   * Lọc người dùng (Filter theo tuổi, giới tính, khoảng cách, online).
   * Hiển thị điểm số tương hợp AI (Compatibility Score).
4. **Trò Chuyện Trực Tuyến (Real-time Chat):**
   * Chat tức thì với SignalR.
   * Tính năng "Đang soạn tin nhắn..." (Typing indicator).
   * Hỗ trợ thu hồi, chỉnh sửa tin nhắn.
5. **Dashboard Quản Trị (Admin):**
   * Thống kê lượng người dùng, trạng thái Match.
   * Cấp/Hủy "Tích xanh" (Xác minh).
   * Xử lý báo cáo vi phạm, khóa tài khoản (Ban User).

---

*Developed with ❤️*
