using MongoDB.Driver;
using Beres.Shared.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RepositoryPattern.Services.RekeningService
{
    public class RekeningService : IRekeningService
    {
        private readonly IMongoCollection<Setting> _settingCollection;


        public RekeningService(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            var database = client.GetDatabase("beres");
            _settingCollection = database.GetCollection<Setting>("Setting");
        }

        public async Task<object> getRekening()
        {
            try
            {
                var Bank = await _settingCollection.Find(d => d.Key == "Bank").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var Rekening = await _settingCollection.Find(d => d.Key == "Rekening").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                var Holder = await _settingCollection.Find(d => d.Key == "Holder").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                return new
                {
                    code = 200,
                    Message = "Berhasil",
                    Data = new
                    {
                        Bank = Bank.Value,
                        Rekening = Rekening.Value,
                        Holder = Holder.Value
                    }
                };
            }
            catch (Exception)
            {
                throw new CustomException(400, "Message", "Failed to send Rekening email");
            }
        }

        public async Task<object> SettingArisan()
        {
            try
            {
                var Bank = await _settingCollection.Find(d => d.Key == "SettingArisan").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                return new
                {
                    code = 200,
                    Message = "Berhasil",
                    Data = Bank.Value
                };
            }
            catch (Exception)
            {
                throw new CustomException(400, "Message", "Failed to send Rekening email");
            }
        }

        public async Task<object> SettingPatungan()
        {
            try
            {
                var Bank = await _settingCollection.Find(d => d.Key == "SettingPatungan").FirstOrDefaultAsync() ?? throw new CustomException(400, "Data", "Data not found");
                return new
                {
                    code = 200,
                    Message = "Berhasil",
                    Data = Bank.Value
                };
            }
            catch (Exception)
            {
                throw new CustomException(400, "Message", "Failed to send Rekening email");
            }
        }

        public class sendForm
        {
            public string? Id { get; set; }
            public string? Phone { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
            public string? Rekening { get; set; }
        }


    }
}
