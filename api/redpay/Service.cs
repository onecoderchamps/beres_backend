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
            var database = mongoClient.GetDatabase("redpay");
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
                string id = Guid.NewGuid().ToString();
                int amountBasic = 10000;

                var requestBody = new
                {
                    redirect_url = "https://merchant.com/return",
                    user_id = dto.Company,
                    user_mdn = dto.PhoneNumber,
                    merchant_transaction_id = id,
                    payment_method = dto.PaymentMethod,
                    currency = "IDR",
                    amount = amountBasic * dto.MemberOrder!.Count,
                    item_id = "1",
                    item_name = "PAYMENT",
                    customer_name = dto.Company,
                    notification_url = "https://apiimpact.coderchamps.co.id/api/v1/redpay/approved",
                };
                string jsonBody = JsonSerializer.Serialize(requestBody);
                string bodySign = GenerateBodySign(requestBody, "ee9Kpp-tBUmRRFM");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("appkey", "c3X9c-_RLJ1AQkFX_1yUgg");
                httpClient.DefaultRequestHeaders.Add("appid", "0ChSmgeTWqy5M_n2vKWm0Q");
                httpClient.DefaultRequestHeaders.Add("bodysign", bodySign);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://sandbox-payment.redision.com/api/transaction", content);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent); 
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;
                string paymentUrl = "";

                if (dto.PaymentMethod == "visa_master")
                {
                    paymentUrl = root.GetProperty("data").GetProperty("payment_url").GetString() ?? "";
                } else if (dto.PaymentMethod == "qris")
                {
                    paymentUrl = root.GetProperty("qrisUrl").GetString() ?? "";
                }
                else
                {
                    paymentUrl = root.GetProperty("data").GetProperty("va").GetString() ?? "";
                }


                var transaction = new RedPayModel
                {
                    Id = id,
                    Company = dto.Company,
                    Category = dto.Category,
                    Website = dto.Website,
                    PhoneNumber = dto.PhoneNumber,
                    PaymentMethod = dto.PaymentMethod,
                    Email = dto.Email,
                    Amount = amountBasic * dto.MemberOrder!.Count,
                    Qty = dto.MemberOrder!.Count,
                    MemberOrder = dto.MemberOrder,
                    IsActive = false,
                    IsVerification = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // ReferenceId = result.Data.Transaction_Id
                };

                await _RedPayCollection.InsertOneAsync(transaction);

                return new
                {
                    code = 200,
                    data = responseContent,
                    paymentUrl = paymentUrl,
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

        public async Task<object> ApprovedRedPay(ApprovedRedpayDto item)
        {
            try
            {
                string merchantOrderId = item.merchant_transaction_id;
                var campaign = await _RedPayCollection.Find(_ => _.Id == merchantOrderId).FirstOrDefaultAsync();
                if (campaign == null)
                {
                    throw new CustomException(400, "Error", "Data Not Found");
                }
                if(item.status != "success")
                {
                    throw new CustomException(400, "Error", "Payment Not Successful");
                }
                campaign.IsVerification = true;
                await _RedPayCollection.ReplaceOneAsync(x => x.Id == merchantOrderId, campaign);
                return new
                {
                    code = 200,
                    request = "Done",
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
    }
}
