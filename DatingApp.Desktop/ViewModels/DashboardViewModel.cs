using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Models;
using DatingApp.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Linq;

namespace DatingApp.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private string _currentUserName = "Đang tìm kiếm...";

    [ObservableProperty]
    private string _currentUserBio = "";

    [ObservableProperty]
    private string _currentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=Loading...";

    [ObservableProperty]
    private bool _isCurrentUserVerified;

    // --- Tabs ---
    [ObservableProperty]
    private bool _isDiscoverVisible = true;

    [ObservableProperty]
    private bool _isProfileVisible = false;

    [ObservableProperty]
    private bool _isMessagesVisible = false;

    [ObservableProperty]
    private string _discoverTabColor = "#E6005C";

    [ObservableProperty]
    private string _profileTabColor = "#6B7280";

    [ObservableProperty]
    private string _messagesTabColor = "#6B7280";

    [ObservableProperty]
    private int _unreadNotificationCount;

    [ObservableProperty]
    private ObservableCollection<NotificationDto> _notifications = new();

    [ObservableProperty]
    private bool _isNotificationPanelOpen;

    // --- Profile Data ---
    [ObservableProperty]
    private string? _profileAvatarUrl = null;

    [ObservableProperty]
    private string _profileFullName = "";

    [ObservableProperty]
    private string _profileBio = "";

    [ObservableProperty]
    private string _profileGender = "";

    [ObservableProperty]
    private DateTime? _profileDateOfBirth = null;

    [ObservableProperty]
    private string _profileInterestedIn = "";

    [ObservableProperty]
    private string _profileHeight = "";

    [ObservableProperty]
    private string _profileEducation = "";

    [ObservableProperty]
    private string _profileSmoking = "";

    [ObservableProperty]
    private string _profileDrinking = "";

    [ObservableProperty]
    private string _profileOccupation = "";

    [ObservableProperty]
    private string _profileLocation = "";

    [ObservableProperty]
    private string _profileZodiac = "";

    [ObservableProperty]
    private string _profileMbti = "";

    [ObservableProperty]
    private string _profileLookingFor = "";

    [ObservableProperty]
    private string _profileLifestyle = "";

    [ObservableProperty]
    private string _profileVibe = "";

    [ObservableProperty]
    private string _profileMaxDistance = "";

    [ObservableProperty]
    private string _profileInterests = "";

    [ObservableProperty]
    private string _profileValues = "";

    [ObservableProperty]
    private ObservableCollection<PhotoDto> _profilePhotos = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfileComplete))]
    private int _profileCompletionScore = 0;

    public bool IsProfileComplete => ProfileCompletionScore == 100;

    // --- Match System ---
    [ObservableProperty]
    private bool _isMatchPopupVisible = false;

    private List<DiscoverUserDto> _discoverQueue = new();
    private DiscoverUserDto? _currentUserDto;

    // --- Discover Filters ---
    [ObservableProperty]
    private string _filterAgeMin = "18";

    [ObservableProperty]
    private string _filterAgeMax = "99";

    [ObservableProperty]
    private string _filterGender = ""; // Empty = All, "0" = Nam, "1" = Nữ

    [ObservableProperty]
    private string _filterMaxDistance = "50";

    // --- Chat System ---
    private HubConnection? _hubConnection;

    [ObservableProperty]
    private ObservableCollection<MatchDto> _matches = new();

    [ObservableProperty]
    private ObservableCollection<MessageDto> _currentMessages = new();

    private bool _isReloadingMatches;

    public bool IsChatActive => SelectedMatch != null;

    private MatchDto? _selectedMatch;
    public MatchDto? SelectedMatch
    {
        get => _selectedMatch;
        set
        {
            if (_isReloadingMatches && value == null)
            {
                return;
            }

            var oldPartnerId = _selectedMatch?.Partner?.Id;
            if (SetProperty(ref _selectedMatch, value))
            {
                OnPropertyChanged(nameof(IsChatActive));
                if (value != null && value.Partner.Id != oldPartnerId)
                {
                    _ = LoadMessagesAsync(value.Partner.Id);
                }
            }
        }
    }

    [ObservableProperty]
    private string _messageDraft = "";

    public DashboardViewModel(AuthService authService, IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        IsAdmin = _authService.CurrentUser?.IsAdmin == true;

        // Set token trực tiếp — đảm bảo luôn có Bearer token
        if (!string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        _ = LoadDiscoverUsersAsync();
        _ = InitializeSignalRAsync();
        _ = LoadUnreadNotificationCountAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadDiscoverUsersAsync();
    }

    private async Task LoadDiscoverUsersAsync()
    {
        try
        {
            CurrentUserName = "Đang tìm kiếm...";
            CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=Loading...";
            
            var query = new List<string>();
            if (int.TryParse(FilterAgeMin, out int ageMin)) query.Add($"ageMin={ageMin}");
            if (int.TryParse(FilterAgeMax, out int ageMax)) query.Add($"ageMax={ageMax}");
            if (int.TryParse(FilterMaxDistance, out int maxDistance)) query.Add($"maxDistance={maxDistance}");
            
            if (FilterGender == "Nam") query.Add("gender=0");
            else if (FilterGender == "Nữ") query.Add("gender=1");
            else if (FilterGender == "Khác") query.Add("gender=2");

            string queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
            var response = await _httpClient.GetFromJsonAsync<DiscoverResponse>($"/api/User/discover{queryString}");
            
            if (response != null && response.Data.Count > 0)
            {
                _discoverQueue = response.Data;
                NextProfile();
            }
            else
            {
                CurrentUserName = "Hết người để khám phá!";
                CurrentUserBio = "Hãy thử lại sau nhé.";
                CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles"; 
                _currentUserDto = null;
            }
        }
        catch (Exception ex)
        {
            CurrentUserName = "Lỗi kết nối mạng";
            CurrentUserBio = ex.Message;
        }
    }

    [RelayCommand]
    public async Task LikeAsync()
    {
        if (_currentUserDto == null) return;

        try
        {
            var dto = new { ToUserId = _currentUserDto.Id, IsLike = true };
            var response = await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                if (result.TryGetProperty("isMatch", out var isMatchProp) && isMatchProp.GetBoolean())
                {
                    IsMatchPopupVisible = true;
                }
            }
        }
        catch {}

        NextProfile();
    }

    [RelayCommand]
    public async Task PassAsync()
    {
        if (_currentUserDto == null) return;

        try
        {
            var dto = new { ToUserId = _currentUserDto.Id, IsLike = false };
            await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
        }
        catch {}

        NextProfile();
    }

    [RelayCommand]
    private void OpenAdmin()
    {
        // Navigate to AdminView
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
            new DatingApp.Desktop.Messages.NavigationMessage(((App)System.Windows.Application.Current).Services.GetService(typeof(AdminViewModel)))
        );
    }

    private void NextProfile()
    {
        if (_discoverQueue.Count > 0)
        {
            _currentUserDto = _discoverQueue[0];
            _discoverQueue.RemoveAt(0);

            CurrentUserName = $"{_currentUserDto.FullName}, {_currentUserDto.Age}";
            CurrentUserBio = _currentUserDto.Bio;
            CurrentUserImage = string.IsNullOrEmpty(_currentUserDto.AvatarUrl) 
                ? "pack://application:,,,/Resources/default-avatar.jpg" // Fallback cho có ảnh
                : _currentUserDto.AvatarUrl;
            IsCurrentUserVerified = _currentUserDto.IsVerified;
        }
        else
        {
            CurrentUserName = "Hết người để khám phá!";
            CurrentUserBio = "Hãy thử lại sau nhé.";
            CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles";
            _currentUserDto = null;
            IsCurrentUserVerified = false;
        }
    }

    [RelayCommand]
    private void CloseMatchPopup()
    {
        IsMatchPopupVisible = false;
    }

    // --- TABS & PROFILE COMMANDS ---

    [RelayCommand]
    private void ShowDiscover()
    {
        IsDiscoverVisible = true;
        IsProfileVisible = false;
        IsMessagesVisible = false;
        DiscoverTabColor = "#E6005C";
        ProfileTabColor = "#6B7280";
        MessagesTabColor = "#6B7280";
    }

    [RelayCommand]
    private void ShowMessages()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = false;
        IsMessagesVisible = true;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#6B7280";
        MessagesTabColor = "#E6005C";

        _ = LoadMatchesAsync();
    }

    [RelayCommand]
    private async Task ShowProfileAsync()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = true;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#E6005C";

        try
        {
            var profile = await _httpClient.GetFromJsonAsync<UserDto>("/api/User/profile");
            if (profile != null)
            {
                ProfileFullName = profile.FullName ?? "";
                ProfileBio = profile.Bio ?? "";
                ProfileGender = profile.Gender == 0 ? "Nam" : (profile.Gender == 1 ? "Nữ" : "Khác");
                ProfileAvatarUrl = string.IsNullOrEmpty(profile.AvatarUrl) ? "pack://application:,,,/Resources/default-avatar.jpg" : profile.AvatarUrl;
                ProfileCompletionScore = profile.ProfileCompletionScore;
                
                ProfileDateOfBirth = profile.DateOfBirth;
                ProfileHeight = profile.Height?.ToString() ?? "";
                ProfileEducation = profile.Education ?? "";
                ProfileOccupation = profile.Occupation ?? "";
                ProfileLocation = profile.Location ?? "";
                ProfileZodiac = profile.Zodiac ?? "";
                ProfileMbti = profile.Mbti ?? "";
                ProfileLookingFor = profile.LookingFor ?? "";
                ProfileLifestyle = profile.Lifestyle ?? "";
                ProfileVibe = profile.Vibe ?? "";
                ProfileSmoking = profile.Smoking ?? "";
                ProfileDrinking = profile.Drinking ?? "";
                ProfileMaxDistance = profile.MaxDistance?.ToString() ?? "";
                ProfileInterests = profile.Interests != null ? string.Join(", ", profile.Interests) : "";
                ProfileValues = profile.Values != null ? string.Join(", ", profile.Values) : "";

                ProfilePhotos.Clear();
                if (profile.Photos != null)
                {
                    foreach (var photo in profile.Photos)
                    {
                        ProfilePhotos.Add(photo);
                    }
                }
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        try
        {
            var dto = new 
            { 
                FullName = ProfileFullName,
                Bio = ProfileBio,
                Height = int.TryParse(ProfileHeight, out var h) ? h : (int?)null,
                MaxDistance = int.TryParse(ProfileMaxDistance, out var d) ? d : (int?)null,
                Education = ProfileEducation,
                Occupation = ProfileOccupation,
                Location = ProfileLocation,
                Zodiac = ProfileZodiac,
                Mbti = ProfileMbti,
                LookingFor = ProfileLookingFor,
                Lifestyle = ProfileLifestyle,
                Vibe = ProfileVibe,
                Smoking = ProfileSmoking,
                Drinking = ProfileDrinking,
                Interests = !string.IsNullOrWhiteSpace(ProfileInterests) ? new List<string>(ProfileInterests.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) : new List<string>(),
                Values = !string.IsNullOrWhiteSpace(ProfileValues) ? new List<string>(ProfileValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) : new List<string>()
            };
            await _httpClient.PutAsJsonAsync("/api/User/profile", dto);
            System.Windows.MessageBox.Show("Cập nhật hồ sơ thành công!", "Thành công");
        }
        catch { }
    }

    [RelayCommand]
    private async Task UploadAvatarAsync()
    {
        await UploadPhotoImplAsync(true);
    }

    [RelayCommand]
    private async Task AddPhotoAsync()
    {
        await UploadPhotoImplAsync(false);
    }

    private async Task UploadPhotoImplAsync(bool isAvatar)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                
                form.Add(streamContent, "file", System.IO.Path.GetFileName(openFileDialog.FileName));

                var endpoint = isAvatar ? "/api/User/avatar" : "/api/User/photos";
                var response = await _httpClient.PostAsync(endpoint, form);
                
                if (response.IsSuccessStatusCode)
                {
                    if (isAvatar)
                    {
                        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                        if (result.TryGetProperty("avatarUrl", out var avatarUrlProp))
                        {
                            ProfileAvatarUrl = avatarUrlProp.GetString() ?? ProfileAvatarUrl;
                            _authService.CurrentUser!.AvatarUrl = ProfileAvatarUrl;
                        }
                        System.Windows.MessageBox.Show("Cập nhật Avatar thành công!", "Thành công");
                    }
                    else
                    {
                        var newPhoto = await response.Content.ReadFromJsonAsync<PhotoDto>();
                        if (newPhoto != null)
                        {
                            ProfilePhotos.Add(newPhoto);
                        }
                        System.Windows.MessageBox.Show("Thêm ảnh thành công!", "Thành công");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi upload ảnh: " + ex.Message, "Lỗi");
            }
        }
    }

    [RelayCommand]
    private async Task DeletePhotoAsync(PhotoDto photo)
    {
        if (photo == null) return;
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/User/photos/{photo.Id}");
            if (response.IsSuccessStatusCode)
            {
                ProfilePhotos.Remove(photo);
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task SetMainPhotoAsync(PhotoDto photo)
    {
        if (photo == null) return;
        try
        {
            var response = await _httpClient.PutAsync($"/api/User/photos/{photo.Id}/setMain", null);
            if (response.IsSuccessStatusCode)
            {
                foreach (var p in ProfilePhotos) p.IsMain = false;
                photo.IsMain = true;
                
                ProfileAvatarUrl = photo.Url;
                _authService.CurrentUser!.AvatarUrl = photo.Url;
            }
        }
        catch { }
    }

    // --- CHAT & MATCH LOGIC ---

    private async Task InitializeSignalRAsync()
    {
        if (string.IsNullOrEmpty(_authService.CurrentToken)) return;

        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_authService.CurrentToken)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<System.Text.Json.JsonElement>("ReceiveMessage", message =>
        {
            var msgDto = new MessageDto
            {
                Id = message.GetProperty("id").GetGuid(),
                SenderId = message.GetProperty("senderId").GetGuid(),
                ReceiverId = message.GetProperty("receiverId").GetGuid(),
                Content = message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != System.Text.Json.JsonValueKind.Null ? contentProp.GetString() ?? "" : "",
                ImageUrl = message.TryGetProperty("imageUrl", out var imageProp) && imageProp.ValueKind != System.Text.Json.JsonValueKind.Null ? imageProp.GetString() ?? "" : "",
                IsSeen = message.GetProperty("isSeen").GetBoolean(),
                SentAt = message.GetProperty("sentAt").GetDateTime(),
                IsMine = false
            };

            // Kiểm tra xem tin nhắn có thuộc về cuộc trò chuyện đang mở không
            if (SelectedMatch != null && msgDto.SenderId == SelectedMatch.Partner.Id)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentMessages.Add(msgDto);
                });
                // Đánh dấu đã đọc trên server
                _ = _httpClient.PutAsync($"/api/Messages/seen/{msgDto.SenderId}", null);
            }
            
            _ = LoadMatchesAsync(); // Làm mới danh sách match để hiển thị last message
        });

        _hubConnection.On<System.Text.Json.JsonElement>("ReceiveNotification", notif =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UnreadNotificationCount++;
                try
                {
                    var newNotif = System.Text.Json.JsonSerializer.Deserialize<NotificationDto>(notif.GetRawText(), new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (newNotif != null)
                    {
                        Notifications.Insert(0, newNotif);
                    }
                }
                catch {}
            });

            if (notif.TryGetProperty("type", out var type) && type.GetString() == "NewMatch")
            {
                _ = LoadMatchesAsync();
            }
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch { }
    }

    private async Task LoadMatchesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<MatchDto>>("/api/Match");
            if (list != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _isReloadingMatches = true;
                    try
                    {
                        var selectedId = SelectedMatch?.Id;

                        Matches.Clear();
                        foreach (var match in list)
                        {
                            Matches.Add(match);
                        }

                        _isReloadingMatches = false;

                        if (selectedId.HasValue)
                        {
                            var newSelection = Matches.FirstOrDefault(m => m.Id == selectedId.Value);
                            SelectedMatch = newSelection;
                        }
                    }
                    finally
                    {
                        _isReloadingMatches = false;
                    }
                });
            }
        }
        catch { }
    }

    private async Task LoadMessagesAsync(Guid partnerId)
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<List<MessageDto>>($"/api/Messages/{partnerId}");
            if (messages != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentMessages.Clear();
                    foreach (var msg in messages)
                    {
                        msg.IsMine = (msg.SenderId.ToString() == _authService.CurrentUser?.Id);
                        CurrentMessages.Add(msg);
                    }
                });
                
                // Đánh dấu đã đọc
                _ = _httpClient.PutAsync($"/api/Messages/seen/{partnerId}", null);
                _ = LoadMatchesAsync(); // Update unread count
            }
        }
        catch { }
    }

    [RelayCommand]
    private void SelectMatch(MatchDto match)
    {
        SelectedMatch = match;
        ShowMessages();
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageDraft) || SelectedMatch == null) return;

        var dto = new { ReceiverId = SelectedMatch.Partner.Id, Content = MessageDraft };
        var draft = MessageDraft;
        MessageDraft = "";

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Messages", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var msgDto = new MessageDto
                {
                    Id = result.GetProperty("id").GetGuid(),
                    SenderId = result.GetProperty("senderId").GetGuid(),
                    ReceiverId = result.GetProperty("receiverId").GetGuid(),
                    Content = result.GetProperty("content").GetString() ?? "",
                    IsSeen = result.GetProperty("isSeen").GetBoolean(),
                    SentAt = result.GetProperty("sentAt").GetDateTime(),
                    IsMine = true
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentMessages.Add(msgDto);
                });
                
                _ = LoadMatchesAsync();
            }
            else
            {
                MessageDraft = draft; // Revert
            }
        }
        catch
        {
            MessageDraft = draft; // Revert
        }
    }

    [RelayCommand]
    private async Task SendImageAsync()
    {
        if (SelectedMatch == null) return;

        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                
                form.Add(streamContent, "file", System.IO.Path.GetFileName(openFileDialog.FileName));
                form.Add(new StringContent(SelectedMatch.Partner.Id.ToString()), "receiverId");

                var response = await _httpClient.PostAsync("/api/Messages/image", form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    var msgDto = new MessageDto
                    {
                        Id = result.GetProperty("id").GetGuid(),
                        SenderId = result.GetProperty("senderId").GetGuid(),
                        ReceiverId = result.GetProperty("receiverId").GetGuid(),
                        Content = result.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != System.Text.Json.JsonValueKind.Null ? contentProp.GetString() ?? "" : "",
                        ImageUrl = result.TryGetProperty("imageUrl", out var imageProp) && imageProp.ValueKind != System.Text.Json.JsonValueKind.Null ? imageProp.GetString() ?? "" : "",
                        IsSeen = result.GetProperty("isSeen").GetBoolean(),
                        SentAt = result.GetProperty("sentAt").GetDateTime(),
                        IsMine = true
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentMessages.Add(msgDto);
                    });
                    
                    _ = LoadMatchesAsync();
                }
                else
                {
                    System.Windows.MessageBox.Show("Lỗi khi gửi ảnh.", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi gửi ảnh: " + ex.Message, "Lỗi");
            }
        }
    }

    [RelayCommand]
    private async Task UnmatchAsync()
    {
        if (SelectedMatch == null) return;
        
        var confirm = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn huỷ tương hợp với {SelectedMatch.Partner.FullName} không?", "Xác nhận", System.Windows.MessageBoxButton.YesNo);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Match/{SelectedMatch.Id}");
            if (response.IsSuccessStatusCode)
            {
                Matches.Remove(SelectedMatch);
                SelectedMatch = null;
                CurrentMessages.Clear();
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task BlockAsync()
    {
        if (SelectedMatch == null) return;
        
        var confirm = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn chặn {SelectedMatch.Partner.FullName} không? Bạn sẽ không thấy họ nữa.", "Cảnh báo", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Match/block/{SelectedMatch.Partner.Id}", null);
            if (response.IsSuccessStatusCode)
            {
                Matches.Remove(SelectedMatch);
                SelectedMatch = null;
                CurrentMessages.Clear();
            }
        }
        catch { }
    }

    private async Task LoadUnreadNotificationCountAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<NotificationResponse>("/api/Notification");
            if (response != null)
            {
                UnreadNotificationCount = response.UnreadCount;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task ToggleNotificationPanelAsync()
    {
        IsNotificationPanelOpen = !IsNotificationPanelOpen;
        if (IsNotificationPanelOpen)
        {
            await LoadNotificationsAsync();
        }
    }

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<NotificationResponse>("/api/Notification");
            if (response != null)
            {
                Notifications = new ObservableCollection<NotificationDto>(response.Notifications);
                UnreadNotificationCount = response.UnreadCount;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task MarkAllNotificationsAsReadAsync()
    {
        try
        {
            var response = await _httpClient.PutAsync("/api/Notification/read-all", null);
            if (response.IsSuccessStatusCode)
            {
                foreach (var notif in Notifications)
                {
                    notif.IsRead = true;
                }
                Notifications = new ObservableCollection<NotificationDto>(Notifications);
                UnreadNotificationCount = 0;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task ClickNotificationAsync(NotificationDto notif)
    {
        if (notif == null) return;

        try
        {
            if (!notif.IsRead)
            {
                var response = await _httpClient.PutAsync($"/api/Notification/{notif.Id}/read", null);
                if (response.IsSuccessStatusCode)
                {
                    notif.IsRead = true;
                    UnreadNotificationCount = Math.Max(0, UnreadNotificationCount - 1);
                }
            }

            IsNotificationPanelOpen = false;

            if (notif.Type == "NewMatch" && notif.RelatedUserId.HasValue)
            {
                ShowMessages();

                var partnerId = notif.RelatedUserId.Value;
                var match = Matches.FirstOrDefault(m => m.Partner.Id == partnerId);
                if (match != null)
                {
                    SelectedMatch = match;
                }
                else
                {
                    await LoadMatchesAsync();
                    match = Matches.FirstOrDefault(m => m.Partner.Id == partnerId);
                    if (match != null)
                    {
                        SelectedMatch = match;
                    }
                }
            }
        }
        catch {}
    }
}
