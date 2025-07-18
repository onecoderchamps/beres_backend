using MongoDB.Driver;
using Beres.Shared.Models;

namespace RepositoryPattern.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> dataUser;
        private readonly IMongoCollection<Transaksi> dataTransaksi;
        private readonly string key;

        public UserService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<User>("User");
            dataTransaksi = database.GetCollection<Transaksi>("Transaksi");
            this.key = configuration.GetSection("AppSettings")["JwtKey"];
        }
        public async Task<Object> Get()
        {
            try
            {
                var items = await dataUser.Find(_ => _.IsActive == true).ToListAsync();
                return new { code = 200, data = items, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<Object> TransferBalance(CreateTransferDto item, string idUser)
        {
            try
            {
                var from = await dataUser.Find(_ => _.Phone == idUser).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data User Not Found");
                var destination = await dataUser.Find(_ => _.Phone == item.Phone).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data User Not Found");
                if (from.Balance == null)
                {
                    throw new CustomException(400, "Error", "Saldo anda tidak cukup");
                }
                if (from.Balance < item.Balance)
                {
                    throw new CustomException(400, "Error", "Saldo anda tidak cukup");
                }
                if (from.Phone == destination.Phone)
                {
                    throw new CustomException(400, "Error", "Tidak boleh kirim ke nomor yang sama");
                }
                if (item.Balance < 10000)
                {
                    throw new CustomException(400, "Error", "Minimal Transfer adalah Rp 10.000");
                }
                ///update from balance
                from.Balance -= item.Balance;
                await dataUser.ReplaceOneAsync(x => x.Phone == idUser, from);

                var transaksi = new Transaksi
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = idUser,
                    IdTransaksi = Guid.NewGuid().ToString(),
                    Type = "Transfer",
                    Nominal = item.Balance,
                    Ket = "Transfer Kepada " + item.Phone,
                    Status = "Expense",
                    CreatedAt = DateTime.Now
                };
                await dataTransaksi.InsertOneAsync(transaksi);

                ///update destination balance
                destination.Balance += item.Balance;
                await dataUser.ReplaceOneAsync(x => x.Phone == item.Phone, destination);
                var transaksi2 = new Transaksi
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = item.Phone,
                    IdTransaksi = Guid.NewGuid().ToString(),
                    Type = "Transfer",
                    Nominal = item.Balance,
                    Ket = "Transfer Dari " + idUser,
                    Status = "Income",
                    CreatedAt = DateTime.Now
                };
                await dataTransaksi.InsertOneAsync(transaksi2);

                return new { code = 200, message = "Transfer Berhasil" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<Object> GetById(string id)
        {
            try
            {
                var items = await dataUser.Find(_ => _.Id == id).FirstOrDefaultAsync();
                return new { code = 200, data = items, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
        public async Task<object> AddUser(CreateUserDto item)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Phone, item.Phone);
                var user = await dataUser.Find(filter).SingleOrDefaultAsync();
                if (user != null)
                {
                    throw new CustomException(400, "Error", "Phone sudah digunakan.");
                }
                var uuid = Guid.NewGuid().ToString();
                var UserData = new User
                {
                    Id = uuid,
                    Phone = item.Phone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IdRole = "1",
                    FullName = item.FullName,
                    Email = "",
                    Image = "https://beres-backend-609517395039.asia-southeast2.run.app/api/v1/file/review/68333845226d836a9b5eb15c",
                    Pin = "",
                    Balance = 0,
                    Point = 0,
                    Fcm = "",
                    NoNIK = "",
                    Address = "",
                    IsActive = true,
                    IsVerification = false
                };
                await dataUser.InsertOneAsync(UserData);
                return new { code = 200, id = UserData.Id, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        // public async Task<object> Put(string id, CreateUserDto item)
        // {
        //     try
        //     {
        //         var UserData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
        //         if (UserData == null)
        //         {
        //             throw new CustomException(400, "Error", "Data Not Found");
        //         }
        //         UserData.FullName = item.Name;
        //         await dataUser.ReplaceOneAsync(x => x.Id == id, UserData);
        //         return new { code = 200, id = UserData.Id.ToString(), message = "Data Updated" };
        //     }
        //     catch (CustomException)
        //     {
        //         throw;
        //     }
        // }
        public async Task<object> Delete(string id)
        {
            try
            {
                var UserData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (UserData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                UserData.IsActive = false;
                await dataUser.ReplaceOneAsync(x => x.Id == id, UserData);
                return new { code = 200, id = UserData.Id.ToString(), message = "Data Deleted" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
    }
}