public class CreateOrderDto
{
    public float? Price { get; set; }
    public string? Image { get; set; }

}

public class UpdateOrderDto
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? Image { get; set; }

}