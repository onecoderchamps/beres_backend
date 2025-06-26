using MongoDB.Driver;
using Beres.Shared.Models;

namespace RepositoryPattern.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IMongoCollection<Order> dataUser;
        private readonly IMongoCollection<Transaksi> dataTransaksi;
        private readonly IMongoCollection<User> Users;
        private readonly IMongoCollection<Setting> _settingCollection;


        private readonly string key;

        public OrderService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<Order>("Order");
            Users = database.GetCollection<User>("User");
            dataTransaksi = database.GetCollection<Transaksi>("Transaksi");
            _settingCollection = database.GetCollection<Setting>("Setting");

            this.key = configuration.GetSection("AppSettings")["JwtKey"];
        }
        public async Task<Object> GetOrderSaldoUser(string idUser)
        {
            try
            {
                var items = await dataUser.Find(_ => _.IsActive == true && _.IdUser == idUser && _.Type == "Saldo" && _.Status == "Pending").FirstOrDefaultAsync();
                return new { code = 200, data = items, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<object> PostSaldo(CreateOrderDto item, string idUser)
        {
            try
            {
                if (item.Price == null || item.Price <= 0)
                {
                    throw new CustomException(400, "Error", "Price Tidak Boleh Kosong Atau Kurang Dari 0");
                }
                if (item.Price < 10000)
                {
                    throw new CustomException(400, "Error", "Minimal Top Up Saldo Adalah Rp. 10.000");
                }
                var user = await dataUser.Find(x => x.IdUser == idUser && x.Type == "Saldo" && x.Status == "Pending" && x.IsActive == true).FirstOrDefaultAsync();
                if (user != null)
                {
                    throw new CustomException(400, "Error", "Kamu Sudah Memiliki Order, Silahkan Selesaikan Pembayaran Sebelumnya");
                }
                var random = new Random();
                int randomDigits = random.Next(0, 1000);
                var OrderData = new Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "Saldo",
                    IdUser = idUser,
                    Status = "Pending",
                    Price = item.Price,
                    UniqueCode = randomDigits,
                    Image = item.Image,
                    IsActive = true,
                    IsVerification = false,
                    CreatedAt = DateTime.Now
                };
                await dataUser.InsertOneAsync(OrderData);
                return new { code = 200, id = OrderData.Id, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<object> UpdateStatus(UpdateOrderDto item)
        {
            try
            {
                var BannerData = await dataUser.Find(x => x.Id == item.Id).FirstOrDefaultAsync();
                if (BannerData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                var authConfig = await _settingCollection.Find(d => d.Key == "authKey").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var appConfig = await _settingCollection.Find(d => d.Key == "appKey").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var phoneCS = await _settingCollection.Find(d => d.Key == "CS").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var emailBody = BannerData.IdUser + $"#Saldo#" + BannerData.Price + $"#UniqueCode#" + BannerData.UniqueCode + $"#Bukti Transfer => " + item.Image;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var form = new MultipartFormDataContent();
                        form.Add(new StringContent(appConfig.Value ?? string.Empty), "appkey");
                        form.Add(new StringContent(authConfig.Value ?? string.Empty), "authkey");
                        form.Add(new StringContent(phoneCS.Value ?? string.Empty), "to");
                        form.Add(new StringContent(emailBody), "message");

                        var response = await httpClient.PostAsync("https://app.saungwa.com/api/create-message", form);
                        var result = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            BannerData.Status = item.Status;
                            BannerData.Image = item.Image ?? BannerData.Image;
                            await dataUser.ReplaceOneAsync(x => x.Id == item.Id, BannerData);
                            return new { code = 200, id = BannerData.Id.ToString(), message = "Data Updated" };
                        }
                        else
                        {
                            return $"Failed to send OTP. Response: {result}";
                        }
                    }
                }
                catch (CustomException)
                {
                    throw;
                }
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
                var OrderData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (OrderData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                OrderData.IsActive = false;
                await dataUser.ReplaceOneAsync(x => x.Id == id, OrderData);
                return new { code = 200, id = OrderData.Id.ToString(), message = "Data Deleted" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
    }
}