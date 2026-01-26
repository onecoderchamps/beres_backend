public class CreateOrderDto
{
    public float? Price { get; set; }
    public string? Image { get; set; }

}

public class CreateOrderWidrawDto
{
    public float? Price { get; set; }
    public string? BankName { get; set; }
    public string? BankNumber { get; set; }
    public string? BankAccount { get; set; }

}

public class UpdateOrderDto
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? Image { get; set; }

}