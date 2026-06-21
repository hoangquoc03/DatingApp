namespace DatingApp.Desktop.Models;

public class DiscoverUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int? Age { get; set; }
    public bool IsVerified { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Zodiac { get; set; }
    public string? Mbti { get; set; }
    public System.Collections.Generic.List<string> Interests { get; set; } = new();
    public int CompatibilityScore { get; set; }
    public bool IsSuperLikedBy { get; set; }
    public System.Collections.Generic.List<PhotoDto> Photos { get; set; } = new();
    public int? Height { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Smoking { get; set; } = string.Empty;
    public string Drinking { get; set; } = string.Empty;
    public double? Distance { get; set; }
}

public class DiscoverResponse
{
    public System.Collections.Generic.List<DiscoverUserDto> Data { get; set; } = new();
    public PaginationInfo? Pagination { get; set; }
    public object? Filters { get; set; }
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrev { get; set; }
}

