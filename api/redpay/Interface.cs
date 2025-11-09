public interface IRedPayService
{

    Task<object> SendRedPayWAAsync(CreateRedpayDto dto);
    Task<object> ApprovedRedPay(ApprovedRedpayDto idUser);
    Task<object> previewOrder(PreviewRedpayDto idUser);


}