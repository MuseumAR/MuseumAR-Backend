using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HistoricalMuseumAudioGuide.Service.Services
{
    public static class ResponseParser
    {
        public static IActionResult Result(ResponseModel responseModel)
        {
            return responseModel.StatusCode switch
            {
                StatusCodes.Status200OK => new OkObjectResult(responseModel),
                StatusCodes.Status400BadRequest => new BadRequestObjectResult(responseModel),
                StatusCodes.Status401Unauthorized => new ObjectResult(responseModel)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                },
                StatusCodes.Status403Forbidden => new ObjectResult(responseModel)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                },
                StatusCodes.Status404NotFound => new NotFoundObjectResult(responseModel),
                StatusCodes.Status500InternalServerError => new ObjectResult(responseModel)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                },
                _ => new BadRequestObjectResult(responseModel)
            };
        }
    }
}
