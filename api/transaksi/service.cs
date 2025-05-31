using MongoDB.Driver;
using Beres.Shared.Models;

namespace RepositoryPattern.Services.TransaksiService
{
    public class TransaksiService : ITransaksiService
    {
        private readonly IMongoCollection<Transaksi> dataUser;
        private readonly string key;

        public TransaksiService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<Transaksi>("Transaksi");
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