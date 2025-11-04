public class CreateRedpayDto
{
    public string Redirect_Url { get; set; }
    public string User_Id { get; set; }
    public string User_Mdn { get; set; }
    public string Merchant_Transaction_Id { get; set; }
    public string Payment_Method { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Item_Id { get; set; }
    public string Item_Name { get; set; }
    public string Notification_Url { get; set; }
}

public class GetRedpayDto
{
    public string? IdOrder { get; set; }

}