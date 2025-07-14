using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Beres.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/Chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _ChatService;
        private readonly ConvertJWT _ConvertJwt;

        public ChatController(ConvertJWT convert,IChatService ChatService)
        {
            _ChatService = ChatService;
            _ConvertJwt = convert;
        }

        [HttpPost("sendChatCS")]
        public async Task<IActionResult> SendChatWA([FromBody] CreateChatDto dto)
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return Unauthorized(new { code = 400, error = "Error", message = "Unauthorized" });
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var result = await _ChatService.SendChatWAAsync(idUser,dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getChatCS")]
        public async Task<IActionResult> GetChatWA()
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return Unauthorized(new { code = 400, error = "Error", message = "Unauthorized" });
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var result = await _ChatService.GetChatWAAsync(idUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
