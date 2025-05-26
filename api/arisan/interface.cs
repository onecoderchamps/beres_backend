public interface IArisanService
{
    Task<Object> Get();
    Task<Object> GetById(string id);
    Task<Object> Post(CreateArisanDto items, string id);
    Task<Object> AddMemberToArisan(CreateMemberArisan memberArisan);
    Task<Object> AddBannerToArisan(CreateBannerArisan memberArisan);


    Task<Object> Put(string id, CreateArisanDto items);
    Task<Object> Delete(string id);
}