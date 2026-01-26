public interface IOrderService
{
    Task<Object> GetOrderSaldoUser(string idUser);
    Task<Object> GetOrderWidrawalUser(string idUser);
    Task<Object> GetOrder();
    Task<Object> PostSaldo(CreateOrderDto items, string idUser);
    Task<Object> PostWidrawal(CreateOrderWidrawDto items, string idUser);
    Task<Object> UpdateStatus(UpdateOrderDto items);
    Task<Object> Delete(string id);
}