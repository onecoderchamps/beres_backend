using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryPattern.Services.RedPayService;
using System.Threading.Tasks;

namespace Beres.Server.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/v1/RedPay")]
    public class RedPayController : ControllerBase
    {
        private readonly IRedPayService _RedPayService;
        private readonly ConvertJWT _ConvertJwt;

        public RedPayController(ConvertJWT convert,IRedPayService RedPayService)
        {
            _RedPayService = RedPayService;
            _ConvertJwt = convert;
        }

        // [HttpPost("generate-bodysign")]
        // public async Task<IActionResult> GenerateBodysign([FromBody] CreateRedpayDto dto)
        // {
        //     try
        //     {
        //         var payload = new
        //         {
        //             redirect_url = "https://merchant.com/return",
        //             user_id = "20250209TEST3477000000",
        //             user_mdn = "08123412451",
        //             merchant_transaction_id = "TESTSH0000011",
        //             payment_method = "indosat_airtime",
        //             currency = "IDR",
        //             amount = 5000,
        //             item_id = "1",
        //             item_name = "PAYMENT",
        //             notification_url = "https://merchant/callback-payment"
        //         };

        //         string appSecret = "ee9Kppp-tBUmRRFM";
        //         string bodysign = RedPayService.GenerateBodySign(payload, appSecret);
        //         return Ok(new { message = bodysign });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpPost("createOrder")]
        public async Task<IActionResult> GetRedPayWA([FromBody] CreateRedpayDto dto)
        {
            try
            {
                var result = await _RedPayService.SendRedPayWAAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("approved")]
        public async Task<IActionResult> Approved([FromBody] ApprovedRedpayDto dto)
        {
            try
            {
                var result = await _RedPayService.ApprovedRedPay(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("previewOrder")]
        public async Task<IActionResult> previewOrder([FromBody] PreviewRedpayDto dto)
        {
            try
            {
                var result = await _RedPayService.previewOrder(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
