

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Trasgo.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/Arisan")]
    public class ArisanController : ControllerBase
    {
        private readonly IArisanService _IArisanService;
        private readonly ErrorHandlingUtility _errorUtility;
        private readonly ConvertJWT _ConvertJwt;
        private readonly ValidationMasterDto _masterValidationService;
        public ArisanController(IArisanService ArisanService, ConvertJWT convert)
        {
            _IArisanService = ArisanService;
             _ConvertJwt = convert;
            _errorUtility = new ErrorHandlingUtility();
            _masterValidationService = new ValidationMasterDto();
        }

        [Authorize]
        [HttpGet]
        public async Task<object> Get()
        {
            try
            {
                var data = await _IArisanService.Get();
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpGet("ByUser")]
        public async Task<object> GetActivity()
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return new CustomException(400, "Error", "Unauthorized");
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var data = await _IArisanService.GetUser(idUser);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpGet("Pay/{idArisan}/{idUser}")]
        public async Task<object> GetById([FromRoute] string idArisan, string idUser)
        {
            try
            {
                var data = await _IArisanService.GetById(idArisan, idUser);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<object> Post([FromBody] CreateArisanDto item)
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return new CustomException(400, "Error", "Unauthorized");
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var data = await _IArisanService.Post(item, idUser);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpPost("PayArisan")]
        public async Task<object> PayArisan([FromBody] CreatePaymentArisan item)
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return new CustomException(400, "Error", "Unauthorized");
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var data = await _IArisanService.PayArisan(item, idUser);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpPost("AddNewArisanMember")]
        public async Task<object> AddNewMember([FromBody] CreateMemberArisan item)
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return new CustomException(400, "Error", "Unauthorized");
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var data = await _IArisanService.AddMemberToArisan(item);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpPost("AddNewBannerArisanMember")]
        public async Task<object> AddBannerNewMember([FromBody] CreateBannerArisan item)
        {
            try
            {
                var claims = User.Claims;
                if (claims == null)
                {
                    return new CustomException(400, "Error", "Unauthorized");
                }
                string accessToken = HttpContext.Request.Headers["Authorization"];
                string idUser = await _ConvertJwt.ConvertString(accessToken);
                var data = await _IArisanService.AddBannerToArisan(item);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<object> Put([FromRoute] string id, [FromBody] CreateArisanDto item)
        {
            try
            {
                var data = await _IArisanService.Put(id, item);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<object> Delete([FromRoute] string id)
        {
            try
            {
                var data = await _IArisanService.Delete(id);
                return Ok(data);
            }
            catch (CustomException ex)
            {
                int errorCode = ex.ErrorCode;
                var errorResponse = new ErrorResponse(errorCode, ex.ErrorHeader, ex.Message);
                return _errorUtility.HandleError(errorCode, errorResponse);
            }
        }


    }
}