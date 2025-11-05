public interface IRedPayService
{

    Task<object> SendRedPayWAAsync(CreateRedpayDto dto);
    Task<object> ApprovedRedPay(ApprovedRedpayDto idUser);

}