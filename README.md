# 💕 Aura Dating — WPF & ASP.NET Core Dating Application

Aura Dating là một ứng dụng hẹn hò toàn diện với giao diện cao cấp (Premium White-Pink Theme), cung cấp đầy đủ các tính năng cốt lõi: **Quẹt thẻ (Swipe)**, **Tương hợp (Match)**, **Chat Real-time**, **Thả cảm xúc tin nhắn (Reactions)**, **Thông báo hệ thống (System Tray & Toast)**, và **Quản trị Hệ thống (Admin Panel)**.

---

## 📐 Kiến Trúc Dự Án

```
DatingApp/
├── DatingApp.csproj          # Server (ASP.NET Core Web API)
├── Program.cs                # Entry point của Server
├── appsettings.Example.json  # Template cấu hình (COPY thành appsettings.json)
├── server/
│   ├── API/Controllers/      # Các REST API Controllers
│   ├── Application/Services/ # Business Logic (Auth, Match, Swipe, Chat...)
│   ├── Infrastructure/
│   │   ├── Data/             # AppDbContext & DbInitializer (Dữ liệu mẫu)
│   │   └── Migrations/       # EF Core Migrations
│   └── Models/               # Entity Models (User, Message, Match, Report...)
├── DatingApp.Desktop/        # Client (WPF Desktop App)
│   ├── DatingApp.Desktop.csproj
│   ├── App.xaml / App.xaml.cs # Entry point & DI Configuration
│   ├── Views/                # XAML Views (Login, Dashboard, Admin, Onboarding)
│   ├── ViewModels/           # MVVM ViewModels
│   ├── Models/               # Client-side DTOs
│   ├── Services/             # AuthService, HTTP Handlers
│   └── Converters/           # WPF Value Converters
└── DatingApp.sln             # Solution file
```

---

## 🚀 Công Nghệ Sử Dụng

### Server (Backend API)
| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core Web API (.NET 8.0) |
| Database | **PostgreSQL** + Entity Framework Core (Npgsql) |
| Authentication | JWT Bearer Tokens + BCrypt password hashing |
| Real-time | SignalR Core (Chat, Typing, Reactions, Block/Unmatch) |
| Image Upload | Cloudinary SDK |
| API Docs | Swagger / Swashbuckle |
| Rate Limiting | ASP.NET Core Rate Limiter (chống Brute-force) |

### Client (Desktop App)
| Thành phần | Công nghệ |
|---|---|
| Framework | WPF (.NET 8.0) + Windows Forms (System Tray) |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| UI Library | Material Design In XAML Toolkit |
| Real-time | Microsoft.AspNetCore.SignalR.Client |
| Notifications | System Tray Icon + Windows Balloon Toast |

---

## ⚙️ Hướng Dẫn Cài Đặt Cho Người Mới Clone Dự Án

### 📋 Yêu Cầu Hệ Thống

| Phần mềm | Phiên bản tối thiểu | Ghi chú |
|---|---|---|
| **.NET SDK** | 8.0 trở lên | [Download .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **PostgreSQL** | 14 trở lên | [Download PostgreSQL](https://www.postgresql.org/download/) |
| **Visual Studio 2022** | 17.8+ | Cài workload: "ASP.NET and web development" + ".NET desktop development" |
| **Git** | Bất kỳ | Để clone dự án |
| **Hệ điều hành** | Windows 10/11 | WPF Desktop chỉ chạy trên Windows |

### 🔧 Bước 1: Clone Dự Án

```bash
git clone https://github.com/hoangquoc03/DatingApp.git
cd DatingApp
```

### 🔧 Bước 2: Cài Đặt PostgreSQL

1. Tải và cài đặt PostgreSQL từ [postgresql.org](https://www.postgresql.org/download/).
2. Trong quá trình cài đặt, ghi nhớ **username** (mặc định: `postgres`) và **password** mà bạn đã đặt.
3. Mở **pgAdmin** hoặc **psql** và tạo một database mới:
   ```sql
   CREATE DATABASE "datingApp";
   ```

### 🔧 Bước 3: Cấu Hình Server

1. Copy file cấu hình mẫu thành file cấu hình thật:
   ```bash
   copy appsettings.Example.json appsettings.json
   ```
2. Mở file `appsettings.json` và điền thông tin của bạn:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=datingApp;Username=postgres;Password=MẬT_KHẨU_POSTGRESQL_CỦA_BẠN"
     },
     "Jwt": {
       "Key": "KHÓA_BÍ_MẬT_ÍT_NHẤT_32_KÝ_TỰ_TUỲ_CHỌN",
       "Issuer": "DatingApp"
     },
     "Cloudinary": {
       "CloudName": "TÊN_CLOUD_CLOUDINARY",
       "ApiKey": "API_KEY_CLOUDINARY",
       "ApiSecret": "API_SECRET_CLOUDINARY",
       "AppFolder": "aura"
     },
     "Smtp": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "Username": "EMAIL_CỦA_BẠN@gmail.com",
       "Password": "MẬT_KHẨU_ỨNG_DỤNG_GMAIL"
     }
   }
   ```

   > **📌 Lưu ý quan trọng:**
   > - **Cloudinary**: Tạo tài khoản miễn phí tại [cloudinary.com](https://cloudinary.com/) để lấy thông tin Cloud Name, API Key, API Secret. Tính năng upload ảnh đại diện và ảnh trong chat phụ thuộc vào Cloudinary.
   > - **SMTP Gmail**: Để gửi email OTP xác thực, bạn cần bật [Mật khẩu ứng dụng Gmail](https://support.google.com/accounts/answer/185833) (App Password). Nếu không cần tính năng gửi email, có thể để trống.
   > - **JWT Key**: Đặt một chuỗi ký tự bất kỳ dài ít nhất 32 ký tự.

### 🔧 Bước 4: Tạo Database & Chạy Migration

```bash
# Cài EF Core CLI tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# Áp dụng tất cả migrations để tạo bảng trong PostgreSQL
dotnet ef database update
```

> **💡 Lưu ý**: Khi Server khởi chạy lần đầu tiên, hệ thống sẽ tự động nạp **dữ liệu mẫu (Seed Data)** bao gồm ~10 tài khoản demo với đầy đủ hồ sơ, Matches, và tin nhắn mẫu thông qua file `DbInitializer.cs`.

### 🔧 Bước 5: Khởi Chạy Server (Backend API)

```bash
# Chạy Server ở chế độ HTTPS (bắt buộc cho SignalR)
dotnet run --launch-profile https
```

Server sẽ khởi chạy tại:
- **HTTPS**: `https://localhost:7150`
- **HTTP**: `http://localhost:5267`
- **Swagger UI**: `https://localhost:7150/swagger`

> **📌 Nếu gặp lỗi chứng chỉ HTTPS (SSL Certificate):**
> ```bash
> dotnet dev-certs https --trust
> ```
> Chạy lệnh trên một lần duy nhất để tin tưởng chứng chỉ phát triển của .NET.

### 🔧 Bước 6: Khởi Chạy Client (Desktop App)

**Cách 1: Dùng Visual Studio**
1. Mở file `DatingApp.sln` bằng Visual Studio 2022.
2. Click chuột phải vào project `DatingApp.Desktop` → **Set as Startup Project**.
3. Nhấn **F5** hoặc **Ctrl+F5** để chạy.

**Cách 2: Dùng Terminal**
```bash
cd DatingApp.Desktop
dotnet run
```

> ⚠️ **Quan trọng**: Server (Bước 5) phải đang chạy trước khi khởi chạy Desktop Client. Client kết nối tới API Server tại `https://localhost:7150`.

---

## 🔑 Tài Khoản Mẫu (Seed Data)

Hệ thống tự động tạo các tài khoản demo khi Server khởi chạy lần đầu tiên. Tất cả tài khoản đều có mật khẩu: **`123456`**

| Email | Vai trò | Tên |
|---|---|---|
| `admin@gmail.com` | 🛡️ Admin | Quản trị viên |
| `demo1@gmail.com` | 👤 User | Nguyễn Thảo Linh |
| `demo2@gmail.com` | 👤 User | Trần Minh Quân |
| `aura3@gmail.com` | 👤 User | Lê Quỳnh Trang |
| `aura4@gmail.com` | 👤 User | Phạm Đức Anh |
| `aura5@gmail.com` | 👤 User | Hoàng Mỹ Duyên |
| ... | 👤 User | Và thêm nhiều tài khoản khác |

---

## 🌟 Chức Năng Nổi Bật

### Người Dùng (User)
- 🃏 **Quẹt thẻ (Swipe & Discover)**: Vuốt phải để Thích, vuốt trái để Bỏ qua, với hoạt ảnh vật lý nảy mượt mà (ElasticEase Spring Physics).
- 💬 **Chat Real-time**: Nhắn tin tức thì qua SignalR, hỗ trợ gửi ảnh, thu hồi/chỉnh sửa tin nhắn, trạng thái "Đang soạn tin nhắn..." (Typing Indicator).
- 😂 **Thả cảm xúc tin nhắn (Reactions)**: Click phải vào tin nhắn để thả 👍 ❤️ 😂 😢 😠, đồng bộ real-time.
- 🔔 **Thông báo Windows Toast**: Khi thu nhỏ ứng dụng xuống khay hệ thống (System Tray), nhận thông báo Balloon Tip khi có tin nhắn mới.
- 🚫 **Chặn & Báo cáo vi phạm**: Chặn người dùng, báo cáo lý do vi phạm. Hệ thống tự động đóng chat khi bị chặn/hủy tương hợp.
- 🔄 **Onboarding đa bước**: Chọn giới tính muốn tìm kiếm, sở thích, MBTI, cung hoàng đạo, lối sống, tải ảnh đại diện.

### Quản Trị Viên (Admin)
- 📊 **Dashboard thống kê**: Tổng Users, Users hoạt động, Lượt Match, Báo cáo vi phạm, Users đã xác thực.
- 🔒 **Khóa/Mở khóa tài khoản (Ban/Unban)**: Vô hiệu hóa tài khoản vi phạm.
- ✅ **Cấp/Hủy tích xanh xác thực (Verify)**: Xác minh danh tính người dùng.
- 🛡️ **Phân quyền Admin (Role Promotion)**: Thăng cấp User thường thành Admin hoặc hạ cấp Admin về User. Có cơ chế bảo mật chống tự hạ quyền và bảo vệ Admin cuối cùng.
- 🗑️ **Xóa tài khoản vĩnh viễn**: Xóa toàn bộ dữ liệu liên quan (Swipes, Matches, Messages, Photos, Reports).
- 📝 **Quản lý Báo cáo vi phạm**: Xem và xử lý các báo cáo vi phạm từ người dùng.

---

## 🛠️ Xử Lý Sự Cố Thường Gặp

### 1. Lỗi kết nối PostgreSQL
```
Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432
```
**Giải pháp**: Đảm bảo PostgreSQL đang chạy và thông tin kết nối trong `appsettings.json` là chính xác (username, password, port).

### 2. Lỗi chứng chỉ HTTPS
```
System.Net.Http.HttpRequestException: The SSL connection could not be established
```
**Giải pháp**: Chạy lệnh `dotnet dev-certs https --trust` trong Terminal và khởi động lại máy tính.

### 3. Desktop Client không kết nối được Server
**Giải pháp**: Đảm bảo Server đang chạy ở profile `https` (cổng 7150). Nếu muốn đổi cổng, cập nhật BaseAddress trong `DatingApp.Desktop/App.xaml.cs` (dòng 33 và 39).

### 4. Lỗi Migration
```
dotnet ef database update
```
Nếu gặp lỗi, xóa database cũ và tạo lại:
```sql
DROP DATABASE IF EXISTS "datingApp";
CREATE DATABASE "datingApp";
```
Sau đó chạy lại `dotnet ef database update`.

### 5. Build thất bại do file bị khóa
```
error MSB3027: Could not copy ... The file is locked by ...
```
**Giải pháp**: Tắt hoàn toàn ứng dụng (cả Server và Desktop) trước khi build lại.

---

## 📜 Giấy Phép

*Developed with ❤️ by Hoàng Quốc*
