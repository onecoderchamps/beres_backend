public interface ITransaksiService
{
    Task<Object> Get(string idUser);
    Task<Object> GetSedekah();
    Task<Object> GetKoperasi();
    Task<Object> GetById(string id);
    Task<Object> Sedekah(string idUser, CreateTransaksiDto items);
    Task<Object> Event(string idUser, CreateEventPayDto items);
    Task<Object> PayBulananKoperasi(string items);
    Task<Object> Put(string id, CreateTransaksiDto items);
    Task<Object> Delete(string id);
}