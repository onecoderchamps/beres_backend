public interface IPatunganService
{
    Task<Object> Get();
    Task<Object> GetbyID(string id, string idUser);
    Task<Object> GetById(string idPatungan, string idUser);
    Task<Object> GetUser(string idUser);
    Task<Object> Post(CreatePatunganDto items, string id);
    Task<Object> AddMemberToPatungan(CreateMemberPatungan memberPatungan);
    Task<Object> AddBannerToPatungan(CreateBannerPatungan memberPatungan);
    Task<Object> PayPatungan(CreatePaymentPatungan memberPatungan, string idPatungan);
    Task<Object> PayCompletePatungan(CreatePaymentPatungan2 memberPatungan, string idPatungan);
    Task<Object> AddMemberToPatunganByAdmin(CreateMemberPatungan memberPatungan);
    Task<Object> DeleteMemberPatungan(DeleteMemberPatungan memberPatungan);



    Task<Object> Put(string id, CreatePatunganDto items);
    Task<Object> Delete(string id);
}