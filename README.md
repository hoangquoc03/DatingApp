git # Yêu cầu hệ thống Dating App

## 1. Authentication (Bắt buộc)
* **Đăng ký**
  * Email + password
  * OTP email
  * Google login
  * Facebook login
  * Apple login
* **Đăng nhập**
  * JWT
  * Refresh token
  * Remember me
* **Quên mật khẩu**
  * Send email reset
* **Bảo mật**
  * Verify email
  * Verify phone
  * 2FA (nâng cao)

## 2. Hồ sơ người dùng (Profile System)
* **Thông tin cơ bản**
  * Avatar
  * Ảnh gallery
  * Full name
  * Tuổi
  * Giới tính
  * Interested in
  * Location
  * Bio
* **Nâng cao**
  * Chiều cao
  * Nghề nghiệp
  * Học vấn
  * Sở thích
  * Zodiac
  * MBTI
  * Lifestyle
  * Smoking/Drinking
* **Media**
  * Upload nhiều ảnh
  * Video intro
  * Story

## 3. Discover / Swipe System (Core)
* **Tinder Style**
  * Swipe left = dislike
  * Swipe right = like
  * Super like
* **Matching**
  * Nếu 2 người cùng like → match
* **Filters**
  * Age range
  * Distance
  * Gender
  * Verified only
  * Online only
* **Recommendation**
  * Nearby users
  * AI suggestion
  * Popular users

## 4. Match System
* Danh sách match
* Match date
* Last message
* Online status
* **Match logic**
  * Auto match
  * Unmatch
  * Block

## 5. Realtime Chat (Quan trọng)
* **Basic**
  * Send message
  * Image message
  * Emoji
* **Realtime**
  * SignalR/WebSocket
  * Seen
  * Typing...
  * Online/offline
* **Advanced**
  * Voice message
  * Video call
  * Message reactions
  * Delete message
  * Edit message

## 6. Notification System
* **Push notification**
  * New match
  * New message
  * Someone liked you
* **In-app notification**
  * Bell icon
  * Notification center

## 7. Search & Discovery
* **Search**
  * Name
  * Location
  * Interests
* **Nearby**
  * GPS
  * Distance radius
* **AI Recommendation**
  * Similar interests
  * Compatibility score

## 8. Premium Features (Kiếm tiền)
* **Subscription**
  * Gold
  * Platinum
  * VIP
* **Premium chức năng**
  * Unlimited likes
  * See who liked you
  * Boost profile
  * Rewind swipe
  * Passport location

## 9. Safety & Moderation
* **User safety**
  * Report user
  * Block user
  * Hide profile
* **AI moderation**
  * Detect nude images
  * Toxic chat detection
  * Spam detection
* **Verification**
  * Selfie verification
  * Blue tick

## 10. Admin Dashboard
* **Quản lý user**
  * Ban user
  * View reports
  * Verify users
* **Analytics**
  * Daily users
  * Matches/day
  * Revenue
  * Active users
* **Moderation**
  * Delete messages
  * Delete photos

## 11. Social Features
* **Story**
  * Instagram-like story
* **Feed**
  * Post status
  * Like/comment
* **Events**
  * Dating events
  * Nearby meetups

## 12. AI Features (Modern Dating App)
* **AI Matchmaking**
  * Match by behavior
  * Match by personality
* **AI Chat Assistant**
  * Gợi ý mở đầu cuộc trò chuyện
* **AI Photo Rating**
  * Chọn ảnh đẹp nhất profile

## 13. Mobile Features
* Mobile responsive
* PWA
* Native app

## 14. Performance & Security
* **Backend**
  * JWT
  * Refresh token
  * Rate limiting
  * Caching
* **Security**
  * Encrypt password
  * Secure image upload
  * Anti spam

## 15. Database Design (Quan trọng)
* **Tables chính**
  * Users
  * Swipes
  * Matches
  * Messages
  * Notifications
  * Reports
  * Photos
  * Subscriptions

## MVP tối thiểu nên có trước
* Phase 1
