using MongoDB.Driver;
using Beres.Shared.Models;

namespace RepositoryPattern.Services.ArisanService
{
    public class ArisanService : IArisanService
    {
        private readonly IMongoCollection<Arisan> dataUser;
        private readonly IMongoCollection<Transaksi> dataTransaksi;
        private readonly IMongoCollection<User> User;


        private readonly string key;

        public ArisanService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            IMongoDatabase database = client.GetDatabase("beres");
            dataUser = database.GetCollection<Arisan>("Arisan");
            dataTransaksi = database.GetCollection<Transaksi>("Transaksi");
            User = database.GetCollection<User>("User");
            this.key = configuration.GetSection("AppSettings")["JwtKey"];
        }
        public async Task<Object> Get()
        {
            try
            {
                var items = await dataUser.Find(_ => _.IsActive == true).ToListAsync();
                var result = items.Select(arisan =>
                {
                    var totalLotTerpakai = arisan.MemberArisans?.Sum(m => m.JumlahLot) ?? 0;
                    var sisaSlot = arisan.TargetLot - totalLotTerpakai;
                    return new
                    {
                        Id = arisan.Id,
                        Title = arisan.Title,
                        Desc = arisan.Description,
                        Keterangan = arisan.Keterangan,
                        Banner = arisan.Banner,
                        Doc = arisan.Document,
                        TotalPrice = arisan.TargetAmount * arisan.TargetLot,
                        TotalSlot = arisan.TargetLot,
                        SisaSlot = sisaSlot,
                        TargetPay = arisan.TargetAmount,
                        JumlahMember = arisan.MemberArisans?.Count ?? 0,
                        MemberArisan = arisan.MemberArisans,
                        Status = arisan.IsAvailable
                    };
                });
                return new { code = 200, data = result, message = "Data Add Complete" };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<Object> GetUser(string idUser)
        {
            try
            {
                var items = await dataUser.Find(_ => _.IsActive == true).ToListAsync();

                // Filter hanya arisan yang memiliki member dengan IdUser yang sesuai
                var filtered = items.Where(arisan =>
                    arisan.MemberArisans != null &&
                    arisan.MemberArisans.Any(m => m.IdUser == idUser)
                );

                var result = filtered.Select(arisan =>
                {
                    var totalLotTerpakai = arisan.MemberArisans?.Sum(m => m.JumlahLot) ?? 0;
                    var sisaSlot = arisan.TargetLot - totalLotTerpakai;

                    return new
                    {
                        Id = arisan.Id,
                        Title = arisan.Title,
                        Desc = arisan.Description,
                        Keterangan = arisan.Keterangan,
                        Banner = arisan.Banner,
                        Doc = arisan.Document,
                        TotalPrice = arisan.TargetAmount * arisan.TargetLot,
                        TotalSlot = arisan.TargetLot,
                        SisaSlot = sisaSlot,
                        TargetPay = arisan.TargetAmount,
                        JumlahMember = arisan.MemberArisans?.Count ?? 0,
                        Status = arisan.IsAvailable,
                        MemberArisan = arisan.MemberArisans,
                        Type = "Arisan",
                        CreatedAt = arisan.CreatedAt,
                    };
                });

                return new { code = 200, data = result, message = "Data berhasil diambil." };
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public async Task<object> GetById(string id, string idUser)
        {
            try
            {
                var cekDbArisan = await dataUser.Find(_ => _.Id == id).FirstOrDefaultAsync();
                if (cekDbArisan == null)
                {
                    throw new CustomException(404, "Not Found", "Data arisan tidak ditemukan.");
                }

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Filter transaksi berdasarkan: ID Arisan, ID User, Type, dan tanggal
                var filter = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.IdTransaksi, id),
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "Arisan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, idUser),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfMonth),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfMonth)
                );

                var cekDbPayment = await dataTransaksi.Find(filter).FirstOrDefaultAsync();
                var sudahBayar = cekDbPayment != null;

                return new
                {
                    code = 200,
                    IsPay = sudahBayar,
                    message = "Cek pembayaran berhasil."
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(500, "Internal Server Error", ex.Message);
            }
        }


        public async Task<object> PayArisan(CreatePaymentArisan id, string idUser)
        {
            try
            {
                var cekDbArisan = await dataUser.Find(_ => _.Id == id.IdTransaksi).FirstOrDefaultAsync();
                if (cekDbArisan == null)
                {
                    throw new CustomException(404, "Not Found", "Data arisan tidak ditemukan.");
                }
                var roleData = await User.Find(x => x.Phone == idUser).FirstOrDefaultAsync() ?? throw new CustomException(400, "Error", "Data not found");
                var member = cekDbArisan.MemberArisans?.FirstOrDefault(m => m.IdUser == idUser && m.IsActive);
                if (member == null)
                {
                    throw new CustomException(404, "Not Found", "Data member arisan tidak ditemukan.");
                }
                if (roleData.Balance < cekDbArisan.TargetAmount * member.JumlahLot)
                {
                    throw new CustomException(400, "Error", "Saldo tidak cukup untuk melakukan pembayaran.");
                }
                // Cek apakah transaksi arisan bulan ini sudah ada
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                var filter = Builders<Transaksi>.Filter.And(
                    Builders<Transaksi>.Filter.Eq(_ => _.IdTransaksi, cekDbArisan.Id),
                    Builders<Transaksi>.Filter.Eq(_ => _.Type, "Arisan"),
                    Builders<Transaksi>.Filter.Eq(_ => _.IdUser, idUser),
                    Builders<Transaksi>.Filter.Gte(_ => _.CreatedAt, startOfMonth),
                    Builders<Transaksi>.Filter.Lte(_ => _.CreatedAt, endOfMonth)
                );
                var existingTransaction = await dataTransaksi.Find(filter).FirstOrDefaultAsync();
                if (existingTransaction != null)
                {
                    throw new CustomException(400, "Error", "Transaksi arisan bulan ini sudah ada.");
                }
                // Kurangi saldo user
                roleData.Balance -= cekDbArisan.TargetAmount * member.JumlahLot ?? 0;
                await User.ReplaceOneAsync(x => x.Phone == idUser, roleData);
                // Buat transaksi baru  
                var transaksi = new Transaksi
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = idUser,
                    IdTransaksi = cekDbArisan.Id,
                    Type = "Arisan",
                    Nominal = cekDbArisan.TargetAmount * member.JumlahLot,
                    Ket = "Pembayaran Arisan",
                    Status = "Expense",
                    CreatedAt = DateTime.Now
                };
                await dataTransaksi.InsertOneAsync(transaksi);

                return new
                {
                    code = 200,
                    message = "Cek pembayaran berhasil."
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(500, "Internal Server Error", ex.Message);
            }
        }

        public async Task<object> Post(CreateArisanDto item, string id)
        {
            try
            {
                // Cek apakah judul arisan sudah digunakan
                var filter = Builders<Arisan>.Filter.Eq(u => u.Title, item.Title);
                var existingArisan = await dataUser.Find(filter).SingleOrDefaultAsync();

                if (existingArisan != null)
                {
                    throw new CustomException(400, "Error", "Nama arisan sudah digunakan.");
                }

                // Mapping DTO ke model
                var arisanData = new Arisan
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = id,
                    Title = item.Title,
                    Description = item.Description,
                    Keterangan = item.Keterangan,
                    Banner = item.Banner?.ToList(),
                    Document = item.Document?.ToList(),
                    Location = item.Location,
                    TargetLot = item.TargetLot,
                    MemberArisans = [],
                    TargetAmount = item.TargetAmount,
                    PenagihanDate = item.PenagihanDate,
                    IsAvailable = item.IsAvailable,
                    IsActive = true,                     // properti dari BaseModel
                    IsVerification = false,              // properti dari BaseModel
                    IsPromo = false,
                    CreatedAt = DateTime.Now,
                };

                // Simpan ke database
                await dataUser.InsertOneAsync(arisanData);

                // Response sukses
                return new
                {
                    code = 200,
                    id = arisanData.Id,
                    message = "Data arisan berhasil ditambahkan."
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Tangani error umum
                throw new CustomException(500, "Internal Server Error", ex.Message);
            }
        }

        public async Task<object> AddMemberToArisan(CreateMemberArisan newMember)
        {
            try
            {
                // Cek apakah Arisan dengan ID yang diberikan ada
                var filter = Builders<Arisan>.Filter.Eq(a => a.Id, newMember.IdArisan);
                var arisan = await dataUser.Find(filter).FirstOrDefaultAsync();

                if (arisan == null)
                {
                    throw new CustomException(404, "Not Found", "Data arisan tidak ditemukan.");
                }

                // Cek apakah member dengan ID atau nomor telepon sudah ada
                var isExistingMember = arisan.MemberArisans != null && arisan.MemberArisans.Any(m =>
                    m.IdUser == newMember.IdUser || m.PhoneNumber == newMember.PhoneNumber);

                if (isExistingMember)
                {
                    throw new CustomException(400, "Duplicate", "Member sudah terdaftar dalam arisan ini.");
                }

                var dataArisan = new MemberArisan
                {
                    IdUser = newMember.IdUser,
                    PhoneNumber = newMember.PhoneNumber,
                    JumlahLot = newMember.JumlahLot,
                    IsActive = newMember.IsActive,
                    IsPayed = newMember.IsPayed
                };

                // Tambahkan member baru ke list
                var update = Builders<Arisan>.Update.Push("MemberArisans", dataArisan);
                await dataUser.UpdateOneAsync(filter, update);

                return new
                {
                    code = 200,
                    message = "Member berhasil ditambahkan ke arisan."
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(500, "Internal Server Error", ex.Message);
            }
        }

        public async Task<object> AddBannerToArisan(CreateBannerArisan newMember)
        {
            try
            {
                // Cek apakah Arisan dengan ID yang diberikan ada
                var filter = Builders<Arisan>.Filter.Eq(a => a.Id, newMember.IdArisan);
                var arisan = await dataUser.Find(filter).FirstOrDefaultAsync();

                if (arisan == null)
                {
                    throw new CustomException(404, "Not Found", "Data arisan tidak ditemukan.");
                }

                var update = Builders<Arisan>.Update
                    .Set(a => a.Banner, newMember.UrlBanner ?? new List<string>())
                    .Set(a => a.Document, newMember.UrlDocument ?? new List<string>());

                await dataUser.UpdateOneAsync(filter, update);

                return new
                {
                    code = 200,
                    message = "Banner dan document berhasil ditambahkan ke arisan."
                };
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(500, "Internal Server Error", ex.Message);
            }
        }



        public async Task<object> Put(string id, CreateArisanDto item)
        {
            try
            {
                var ArisanData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (ArisanData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                ArisanData.Title = item.Title;
                await dataUser.ReplaceOneAsync(x => x.Id == id, ArisanData);
                return new { code = 200, id = ArisanData.Id.ToString(), message = "Data Updated" };
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
                var ArisanData = await dataUser.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (ArisanData == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                ArisanData.IsActive = false;
                await dataUser.ReplaceOneAsync(x => x.Id == id, ArisanData);
                return new { code = 200, id = ArisanData.Id.ToString(), message = "Data Deleted" };
            }
            catch (CustomException)
            {
                throw;
            }
        }
    }
}