public interface IRedPayService
{

    Task<object> SendRedPayWAAsync(CreateRedpayDto dto);
    Task<object> GetRedPayWAAsync(string idUser);

}