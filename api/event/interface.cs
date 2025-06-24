public interface IEventService
{
    Task<Object> Get();
    Task<Object> GetById(string id, string idUser);
    Task<Object> Post(CreateEventDto items);
    Task<Object> Put(string id, CreateEventDto items);
    Task<Object> Delete(string id);
}