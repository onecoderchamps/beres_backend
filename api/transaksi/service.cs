using MongoDB.Driver;
using Beres.Shared.Models;

namespace RepositoryPattern.Services.TransaksiService
{
    public class TransaksiService : ITransaksiService
    {
        private readonly IMongoCollection<Transaksi> dataUser;
        private readonly IMongoCollection<User> Users;
        private readonly IMongoCollection<Setting3> Setting;
        private readonly string key;

        public TransaksiService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<Transaksi>("Transaksi");
            Users = database.GetCollection<User>("User");
            Setting = database.GetCollection<Setting3>("Setting");


            this.key = configuration.GetSection("AppSettings")["JwtKey"];
        }
        public async Task<object> Get(string idUser)
        {
            try
            {
                var items = await dataUser
                    .Find(_ => _.IdUser == idUser)
                    .SortByDescending(_ => _.CreatedAt) // Urutkan dari terbaru ke terlama
                    .ToListAsync();

                return new { code = 200, data = items, message = "Data Add Complete" };
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
        public async Task<object> Post(CreateTransaksiDto item)
        {
            try
            {
                var filter = Builders<Transaksi>.Filter.Eq(u => u.IdTransaksi, item.Name);
                var user = await dataUser.Find(filter).SingleOrDefaultAsync();
                if (user != null)
                {
                    throw new CustomException(400, "Error", "Nama sudah digunakan.");
                }
                var TransaksiData = new Transaksi()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    IsVerification = false,
                    CreatedAt = DateTime.Now
                };
                await dataUser.InsertOneAsync(TransaksiData);
                return new { code = 200, id = TransaksiData.Id, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<object> PayBulananKoperasi(string idUser)
        {
            try
            {
                var setting = await Setting.Find(d => d.Key == "IuranBulanan").FirstOrDefaultAsync()
                              ?? throw new CustomException(400, "Data", "Data not found");

                var dataUsers = await Users.Find(x => x.Phone == idUser).FirstOrDefaultAsync();
                if (dataUsers == null)
                    throw new CustomException(400, "Error", "Data User Not Found");

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var filter = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "KoperasiBulanan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, idUser),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfMonth),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfMonth)
                );

                var existingTransaction = await dataUser.Find(filter).FirstOrDefaultAsync();
                if (existingTransaction != null)
                    throw new CustomException(400, "Error", "Transaksi koperasi bulan ini sudah ada.");

                var nominal = setting.Value ?? 0;
                if (dataUsers.Balance < nominal)
                    throw new CustomException(400, "Error", "Saldo tidak mencukupi");

                dataUsers.Balance -= nominal;
                await Users.ReplaceOneAsync(x => x.Phone == idUser, dataUsers);

                var transaksiData = new Transaksi()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = idUser,
                    IdTransaksi = Guid.NewGuid().ToString(),
                    Type = "KoperasiBulanan",
                    Nominal = nominal,
                    Ket = "Iuran Bulanan Koperasi",
                    Status = "Expense",
                    CreatedAt = DateTime.Now
                };
                await dataUser.InsertOneAsync(transaksiData);

                return new { code = 200, id = transaksiData.Id, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }


        public async Task<object> Put(string id, CreateTransaksiDto item)
        {
            try
            {
                var TransaksiData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (TransaksiData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                // TransaksiData.Name = item.Name;
                await dataUser.ReplaceOneAsync(x => x.Id == id, TransaksiData);
                return new { code = 200, id = TransaksiData.Id.ToString(), message = "Data Updated" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
        public async Task<object> Delete(string id)
        {
            try
            {
                var TransaksiData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (TransaksiData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                TransaksiData.IsActive = false;
                await dataUser.ReplaceOneAsync(x => x.Id == id, TransaksiData);
                return new { code = 200, id = TransaksiData.Id.ToString(), message = "Data Deleted" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
    }
}