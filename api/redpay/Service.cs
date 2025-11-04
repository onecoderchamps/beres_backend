using MongoDB.Driver;
using Beres.Shared.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;


namespace RepositoryPattern.Services.RedPayService
{
    public class RedPayService : IRedPayService
    {
        private readonly IMongoCollection<RedPayModel> _RedPayCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Setting> _settingCollection;

        public RedPayService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            var database = mongoClient.GetDatabase("beres");
            _RedPayCollection = database.GetCollection<RedPayModel>("RedPay");
            _userCollection = database.GetCollection<User>("User");
            _settingCollection = database.GetCollection<Setting>("Setting");
        }

        public static string GenerateBodySign(object payload, string appSecret)
        {
            // Serialize JSON tanpa escape slash (pakai encoder unsafe)
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            };
            string payloadJson = JsonSerializer.Serialize(payload, options);

            // Buat HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));

            // Convert ke Base64
            string base64 = Convert.ToBase64String(hashBytes);

            // Ubah ke Base64 URL-safe (tanpa menghapus '=')
            string bodySign = base64.Replace('+', '-').Replace('/', '_');

            return bodySign;
        }

        public async Task<object> SendRedPayWAAsync(CreateRedpayDto dto)
        {
            try
            {
                var requestBody = new
                {
                    redirect_url = "https://merchant.com/return",
                    user_id = "20250209TEST3477000000",
                    user_mdn = "08123412451",
                    merchant_transaction_id = "TESTSH0000011",
                    payment_method = "indosat_airtime",
                    currency = "IDR",
                    amount = 10000,
                    item_id = "3322",
                    item_name = "PAYMENT",
                    notification_url = "https://apiimpact.coderchamps.co.id/api/v1/redpay/verifCampaign"
                };
                string jsonBody = JsonSerializer.Serialize(requestBody);

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("appkey", "c3X9c-_RLJ1AQkFX_1yUgg");
                httpClient.DefaultRequestHeaders.Add("appid", "0ChSmgeTWqy5M_n2vKWm0Q");
                string bodySign = GenerateBodySign(requestBody, "ee9Kpp-tBUmRRFM");
                httpClient.DefaultRequestHeaders.Add("bodysign", bodySign);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://sandbox-payment.redision.com/api/transaction", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                return new { code = 200, data = responseContent };
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

        public async Task<object> GetRedPayWAAsync(string idUser)
        {
            try
            {

                return new { code = 200, data = "result" };
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
    }
}
