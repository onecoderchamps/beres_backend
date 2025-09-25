

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CheckId;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Beres.Shared.Models;
using MongoDB.Bson;

namespace RepositoryPattern.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> dataUser;
        private readonly IMongoCollection<Transaksi> Transaksi;
        private readonly IMongoCollection<ChatModel> dataChat;
        private readonly IMongoCollection<Order> dataOrder;
        private readonly IMongoCollection<Patungan> Patungans;

        private readonly IMongoCollection<Setting3> Setting;

        private readonly string key;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<User>("User");
            dataChat = database.GetCollection<ChatModel>("Chat");
            dataOrder = database.GetCollection<Order>("Order");
            Transaksi = database.GetCollection<Transaksi>("Transaksi");
            Patungans = database.GetCollection<Patungan>("Patungan");
            Setting = database.GetCollection<Setting3>("Setting");
            this.key = configuration.GetSection("AppSettings")["JwtKey"];
            _logger = logger;
        }

        public async Task<object> DeleteAccount(string idUser)
        {
            try
            {
                // Buat dua kemungkinan format nomor
                string idUser62 = idUser;
                string idUser0 = idUser;

                if (idUser.StartsWith("+62"))
                {
                    idUser0 = "0" + idUser.Substring(3); // +62... -> 0...
                }
                else if (idUser.StartsWith("0"))
                {
                    idUser62 = "+62" + idUser.Substring(1); // 0... -> +62...
                }

                // Cari user dengan salah satu format
                var filter = Builders<User>.Filter.Or(
                    Builders<User>.Filter.Eq(u => u.Phone, idUser62),
                    Builders<User>.Filter.Eq(u => u.Phone, idUser0)
                );

                var user = await dataUser.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new CustomException(400, "Data", "User not found");
                }

                // Hapus user (pakai filter yang sama)
                await dataUser.DeleteOneAsync(filter);

                // Update data terkait (pakai kedua kemungkinan IdUser)
                var filterChat = Builders<ChatModel>.Filter.Or(
                    Builders<ChatModel>.Filter.Eq(c => c.IdUser, idUser62),
                    Builders<ChatModel>.Filter.Eq(c => c.IdUser, idUser0)
                );
                await dataChat.UpdateManyAsync(filterChat, Builders<ChatModel>.Update.Set(c => c.IdUser, null));

                var filterOrder = Builders<Order>.Filter.Or(
                    Builders<Order>.Filter.Eq(o => o.IdUser, idUser62),
                    Builders<Order>.Filter.Eq(o => o.IdUser, idUser0)
                );
                await dataOrder.UpdateManyAsync(filterOrder, Builders<Order>.Update.Set(o => o.IdUser, null));

                var filterTrans = Builders<Transaksi>.Filter.Or(
                    Builders<Transaksi>.Filter.Eq(t => t.IdUser, idUser62),
                    Builders<Transaksi>.Filter.Eq(t => t.IdUser, idUser0)
                );
                await Transaksi.UpdateManyAsync(filterTrans, Builders<Transaksi>.Update.Set(t => t.IdUser, null));

                var filterPatungan = Builders<Patungan>.Filter.ElemMatch(
                    s => s.MemberPatungans,
                    mp => mp.IdUser == idUser
                );
                var update = Builders<Patungan>.Update
                .Set("MemberPatungans.$[elem].IdUser", (string?)null)       // atau "deleted"
                .Set("MemberPatungans.$[elem].PhoneNumber", (string?)null); 
                var options = new UpdateOptions
                {
                    ArrayFilters = new List<ArrayFilterDefinition>
                    {
                        new JsonArrayFilterDefinition<BsonDocument>("{ 'elem.IdUser': '" + idUser + "' }")
                    }
                };

                await Patungans.UpdateManyAsync(filterPatungan, update, options);

                return new { code = 200, message = "Account deleted successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account {IdUser}", idUser);
                throw new CustomException(400, "Message", "Failed to delete account");
            }
        }

        public async Task<object> UpdatePin(string id, UpdatePinDto item)
        {
            try
            {
                var roleData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (roleData == null)
                {
                    throw new CustomException(400, "Error", "Data tidak ada");
                }
                if (item.Pin.Length < 6)
                {
                    throw new CustomException(400, "Password", "Pin harus 6 karakter");
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(item.Pin);
                roleData.Pin = hashedPassword;
                await dataUser.ReplaceOneAsync(x => x.Id == id, roleData);
                return new { code = 200, Message = "Update Pin Berhasil" };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }

        public async Task<Object> CheckPin(string id, PinDto pin)
        {
            try
            {
                var user = await dataUser.Find(u => u.Phone == id).FirstOrDefaultAsync() ?? throw new CustomException(400, "Email", "Email tidak ditemukan");
                bool isPinCorrect = BCrypt.Net.BCrypt.Verify(pin.Pin, user.Pin);
                if (!isPinCorrect)
                {
                    throw new CustomException(400, "Pin", "Pin Salah");
                }
                string idAsString = user.Id.ToString();
                return new { code = 200, Message = "Berhasil" };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }

        public async Task<object> UpdateProfile(string id, UpdateProfileDto item)
        {
            try
            {
                var Bank = await Setting.Find(d => d.Key == "IuranTahunan").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var roleData = await dataUser.Find(x => x.Phone == id).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data tidak ada");
                if (roleData.Balance < Bank.Value)
                {
                    throw new CustomException(400, "Error", "Saldo tidak cukup untuk melakukan pembayaran.");
                }
                // Cek apakah transaksi koperasi tahunan tahun ini sudah ada
                var now = DateTime.Now;
                var startOfYear = new DateTime(now.Year, 1, 1);
                var endOfYear = new DateTime(now.Year, 12, 31, 23, 59, 59);

                var filter = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "KoperasiTahunan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, roleData.Phone),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfYear),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfYear)
                );

                var existingTransaction = await Transaksi.Find(filter).FirstOrDefaultAsync();
                if (existingTransaction != null)
                {
                    throw new CustomException(400, "Error", "Transaksi koperasi tahunan tahun ini sudah ada.");
                }

                var transaksi = new Transaksi
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = roleData.Phone,
                    IdTransaksi = Guid.NewGuid().ToString(),
                    Type = "KoperasiTahunan",
                    Nominal = Bank.Value,
                    Ket = "Iuran Tahunan Koperasi",
                    Status = "Expense",
                    CreatedAt = DateTime.Now
                };
                await Transaksi.InsertOneAsync(transaksi);

                roleData.Balance -= Bank.Value;
                roleData.FullName = item.FullName;
                roleData.Email = item.Email;
                roleData.Address = item.Address;
                roleData.NoNIK = item.NoNIK;
                await dataUser.ReplaceOneAsync(x => x.Phone == id, roleData);
                return new { code = 200, Message = "Update Berhasil" };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }

        public async Task<object> UpdateUserProfile(string id, UpdateFCMProfileDto item)
        {
            try
            {
                var roleData = await dataUser.Find(x => x.Phone == id).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data tidak ada");
                roleData.Fcm = item.FCM;
                await dataUser.ReplaceOneAsync(x => x.Phone == id, roleData);
                return new { code = 200, Message = "Update Berhasil" };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }

        public async Task<object> SendNotif(PayloadNotifSend item)
        {
            try
            {
                string ServerKey = "AIzaSyDUKDzEbHSUxeI_d0Y8pAkEi8SydSz-TvQ";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={ServerKey}");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                var payload = new
                {
                    to = item.FCM,
                    notification = new
                    {
                        title = item.Title,
                        body = item.Body
                    },
                    data = new
                    {
                        forceOpen = "true" // Bisa digunakan untuk membuka app otomatis di Android
                    }
                };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://fcm.googleapis.com/fcm/send", content);
                string responseString = await response.Content.ReadAsStringAsync();
                return new { code = 200, Message = "Notification sent successfully", Response = responseString };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }

        public async Task<object> Aktifasi(string id)
        {
            try
            {
                var roleData = await dataUser.Find(x => x.Phone == id).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data not found");
                // Cek apakah transaksi koperasi tahunan tahun ini sudah ada
                var now = DateTime.Now;
                var startOfYear = new DateTime(now.Year, 1, 1);
                var endOfYear = new DateTime(now.Year, 12, 31, 23, 59, 59);

                var filter = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "KoperasiTahunan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, roleData.Phone),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfYear),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfYear)
                );

                var startOfMonthPayed = new DateTime(now.Year, now.Month, 1);
                var endOfMonthPayed = startOfMonthPayed.AddMonths(1).AddTicks(-1);
                var existingTransaction = await Transaksi.Find(filter).FirstOrDefaultAsync();

                var filterBulanan = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "KoperasiBulanan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, roleData.Phone),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfMonthPayed),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfMonthPayed)
                );

                var existingTransactionBulanan = await Transaksi.Find(filterBulanan).FirstOrDefaultAsync();

                var user = new ModelViewUser
                {
                    Phone = roleData.Phone,
                    FullName = roleData.FullName,
                    Balance = roleData.Balance,
                    Point = roleData.Point,
                    Fcm = roleData.Fcm,
                    Image = roleData.Image,
                    Email = roleData.Email,
                    IsMember = existingTransaction != null,
                    IsPayMonthly = existingTransactionBulanan != null,
                    Role = roleData.IdRole,
                };
                return new { code = 200, Id = roleData.Id, Data = user };
            }
            catch (CustomException ex)
            {
                throw;
            }
        }
    }

    public class ModelViewUser
    {
        public string? Id { get; set; }
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public float? Balance { get; set; }
        public float? Point { get; set; }
        public string? Fcm { get; set; }
        public string? Image { get; set; }
        public string? Email { get; set; }
        public bool? IsMember { get; set; }
        public bool? IsPayMonthly { get; set; }

        public string? Role { get; set; }


    }
}