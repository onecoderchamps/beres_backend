public interface IUserService
{
    Task<Object> Get();
    Task<Object> AddUser(CreateUserDto createUserDto);

    Task<Object> TransferBalance(CreateTransferDto item, string idUser);

}