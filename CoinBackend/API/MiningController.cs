using DemCoinCommons;
using Microsoft.AspNetCore.Mvc;

namespace CoinBackend.API;

[ApiController]
[Route("/mining")]
public class MiningController : ControllerBase {

    [HttpGet("status")]
    public ActionResult<MiningStatus> RequestMiningStatus() {
        return Ok(new MiningStatus(Program.CoinNode.PrevBlockHash));
    }

    [HttpPost("submit")]
    public IActionResult SubmitNonce([FromBody] MiningSubmission submission) {
        if (!Program.CoinNode.IsNonceValid(submission.nonce)) {
            return BadRequest("Invalid nonce, please rerequest previous block hash");
        }

        try {
            Program.CoinNode.MineBlock(submission.nonce, submission.walletAddress);
        }
        catch (Exception) {
            return BadRequest("Invalid nonce, please rerequest previous block hash");
        }
        return Ok("Submission Accepted");
    }
}