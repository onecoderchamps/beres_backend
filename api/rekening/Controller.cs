using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Trasgo.Server.Controllers
{
    [ApiController]
    [Route("api/v1/rekening")]

    public class RekeningController : ControllerBase
    {
        private readonly IRekeningService _RekeningService;

        public RekeningController(IRekeningService RekeningService)
        {
            _RekeningService = RekeningService;
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetRekening()
        {
            try
            {
                var result = await _RekeningService.getRekening();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("SettingArisan")]
        public async Task<IActionResult> SettingArisan()
        {
            try
            {
                var result = await _RekeningService.SettingArisan();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("SettingPatungan")]
        public async Task<IActionResult> SettingPatungan()
        {
            try
            {
                var result = await _RekeningService.SettingPatungan();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("SettingIuranBulanan")]
        public async Task<IActionResult> SettingIuranBulanan()
        {
            try
            {
                var result = await _RekeningService.SettingIuranBulanan();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("SettingIuranTahunan")]
        public async Task<IActionResult> SettingIuranTahunan()
        {
            try
            {
                var result = await _RekeningService.SettingIuranTahunan();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("Banner")]
        public async Task<IActionResult> Banner()
        {
            try
            {
                var result = await _RekeningService.Banner();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
