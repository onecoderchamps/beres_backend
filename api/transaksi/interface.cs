public interface ITransaksiService
{
    Task<Object> Get(string idUser);
    Task<Object> GetById(string id);
    Task<Object> Post(CreateTransaksiDto items);
    Task<Object> Put(string id, CreateTransaksiDto items);
    Task<Object> Delete(string id);
}