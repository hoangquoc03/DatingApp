using System;

namespace DatingApp.Desktop.Models;

public class ReportUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    private string? _avatarUrl;
    public string? AvatarUrl 
    { 
        get => _avatarUrl; 
        set => _avatarUrl = string.IsNullOrWhiteSpace(value) ? null : value; 
    }
    public bool IsOnline { get; set; }
}

public class ReportDto
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public ReportUserDto Reporter { get; set; } = new();
    public ReportUserDto ReportedUser { get; set; } = new();

    public string ReasonDisplay => Reason switch
    {
        "fake_profile" => "Hồ sơ giả",
        "harassment" => "Quấy rối / Đe dọa",
        "inappropriate" => "Nội dung phản cảm",
        "spam" => "Spam / Quảng cáo",
        _ => "Lý do khác"
    };

    public string StatusDisplay => Status switch
    {
        "pending" => "Chờ xử lý",
        "reviewed" => "Đã xem xét",
        "resolved" => "Đã giải quyết",
        _ => Status
    };

    public bool IsPending => Status == "pending";
}
