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
        private readonly IMongoCollection<Kurs> _kursCollection;
        private readonly IMongoCollection<Diskons> _diskonCollection;


        private readonly IMongoCollection<Setting> _settingCollection;

        public RedPayService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("ConnectionURI"));
            var database = mongoClient.GetDatabase("redpay");
            _RedPayCollection = database.GetCollection<RedPayModel>("RedPay");
            _userCollection = database.GetCollection<User>("User");
            _diskonCollection = database.GetCollection<Diskons>("Diskon");
            _kursCollection = database.GetCollection<Kurs>("Kurs");
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
                    redirect_url = "https://indonesiamineclosure.com",
                    user_id = dto.Company,
                    user_mdn = dto.PhoneNumber,
                    merchant_transaction_id = id,
                    payment_method = dto.PaymentMethod,
                    currency = dto.Currency ?? "IDR",
                    amount = amountBasic * dto.MemberOrder!.Count,
                    item_id = "1",
                    item_name = "PAYMENT",
                    customer_name = dto.Company,
                    notification_url = "https://apiberes.coderchamps.co.id/api/v1/redpay/approved",

                };
                string jsonBody = JsonSerializer.Serialize(requestBody);
                string bodySign = GenerateBodySign(requestBody, "ee9Kpp-tBUmRRFM");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("appkey", "c3X9c-_RLJ1AQkFX_1yUgg");
                httpClient.DefaultRequestHeaders.Add("appid", "0ChSmgeTWqy5M_n2vKWm0Q");
                httpClient.DefaultRequestHeaders.Add("bodysign", bodySign);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://sandbox-payment.redision.com/api/create", content);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;
                // string paymentUrl = "";

                // if (dto.PaymentMethod == "visa_master")
                // {
                //     paymentUrl = root.GetProperty("data").GetProperty("payment_url").GetString() ?? "";
                // } else if (dto.PaymentMethod == "qris")
                // {
                //     paymentUrl = root.GetProperty("qrisUrl").GetString() ?? "";
                // }
                // else
                // {
                //     paymentUrl = root.GetProperty("data").GetProperty("va").GetString() ?? "";
                // }


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
                    ReferenceId = "https://sandbox-payment.redision.com/api/order/0ChSmgeTWqy5M_n2vKWm0Q/" + root.GetProperty("data").GetProperty("token").GetString(),
                    Delegate = dto.Delegate,
                    Diskon = dto.Diskon
                };

                await _RedPayCollection.InsertOneAsync(transaction);

                return new
                {
                    code = 200,
                    data = responseContent,
                    paymentUrl = "https://sandbox-payment.redision.com/api/order/0ChSmgeTWqy5M_n2vKWm0Q/" + root.GetProperty("data").GetProperty("token").GetString()
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
                if (item.status != "success")
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

        public async Task<object> previewOrder(PreviewRedpayDto item)
        {
            try
            {
                if (item == null)
                    throw new CustomException(400, "Bad Request", "Input tidak boleh null.");

                // Tentukan jenis kurs yang ingin dicari di database
                string Kurs = "";
                if (item.Delegate == "International")
                {
                    Kurs = "IDR"; // jika International, kita mau konversi ke IDR
                }
                else
                {
                    Kurs = "USD"; // jika Domestic, konversi ke USD
                }

                // Ambil kurs dari MongoDB
                var campaign = await _kursCollection.Find(_ => _.To == Kurs).FirstOrDefaultAsync();

                // Tentukan mata uang asli & harga dasar
                string baseCurrency;
                double basePricePerPerson = 0;

                if (item.Delegate.Equals("International", StringComparison.OrdinalIgnoreCase))
                {
                    baseCurrency = "USD";

                    if (item.Participant == 1)
                        basePricePerPerson = 740;
                    else if (item.Participant == 2)
                        basePricePerPerson = 590;
                    else
                        basePricePerPerson = 510;
                }
                else // Domestic
                {
                    baseCurrency = "IDR";

                    if (item.Participant == 1)
                        basePricePerPerson = 9500000;
                    else if (item.Participant == 2)
                        basePricePerPerson = 7600000;
                    else
                        basePricePerPerson = 6650000;
                }

                // Hitung total harga
                double totalPrice = basePricePerPerson * (item.Participant ?? 0);

                // Jika campaign (kurs) ditemukan, lakukan konversi otomatis
                // Misalnya campaign.Currency = nilai tukar (contoh: 1 USD = 15500 IDR)
                double convertedPrice = totalPrice;
                if (campaign != null && campaign.Currency > 0)
                {
                    // Jika baseCurrency berbeda dari Kurs, lakukan konversi
                    convertedPrice = totalPrice * (campaign.Currency ?? 1.0);
                }

                var diskon = await _diskonCollection.Find(_ => _.Code == item.Diskon).FirstOrDefaultAsync();
                if (diskon != null && diskon.DiskonValue > 0)
                {
                    double diskonAmount = (diskon.DiskonValue ?? 0) / 100.0 * convertedPrice;
                    convertedPrice -= diskonAmount;
                }

                return new
                {
                    code = 200,
                    request = "Done",
                    data = new
                    {
                        delegateType = item.Delegate,
                        participant = item.Participant,
                        fromCurrency = baseCurrency,
                        toCurrency = Kurs,
                        rate = campaign?.Currency ?? 1,
                        pricePerParticipant = basePricePerPerson,
                        totalPrice = totalPrice,
                        convertedTotal = convertedPrice,
                        diskonApplied = diskon != null ? diskon.DiskonValue : 0
                    }
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
