using Microsoft.AspNetCore.Mvc;

namespace CoinBackend.API;

[ApiController]
[Route("/")]
public class RootController : ControllerBase {

    [HttpGet("/")]
    public IActionResult Index() {
        return Ok("Democracy Coin API");
    }

    [HttpOptions]
    public IActionResult Options() {
        HttpContext.Response.Headers.Append("Allow", "GET");
        return Ok();
    }
}