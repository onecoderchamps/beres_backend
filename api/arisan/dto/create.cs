public class CreateArisanDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Keterangan { get; set; }
    public string[]? Banner { get; set; }
    public string[]? Document { get; set; }
    public string? Location { get; set; }
    public float? TargetLot { get; set; }
    public float? TargetAmount { get; set; }
    public string? PenagihanDate { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class CreateMemberArisan
{
    public string? IdUser { get; set; }
    public string? IdArisan { get; set; }
    public string? PhoneNumber { get; set; }
    public float? JumlahLot { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPayed { get; set; } = false;
}

public class CreateBannerArisan
{
    public string? IdArisan { get; set; }
    public List<string>? UrlBanner { get; set; }
    public List<string>? UrlDocument { get; set; }
}