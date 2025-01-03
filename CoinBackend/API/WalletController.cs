using DemCoinCommons;
using Microsoft.AspNetCore.Mvc;

namespace CoinBackend.API;

[ApiController]
[Route("wallet")]
public class WalletController : ControllerBase {

    [HttpPost("balance")]
    public ActionResult<WalletBalance> RequestBalance([FromBody] WalletInfo wallet) {
        double bal = Program.CoinNode.GetBalance(wallet.publicKey);
        ulong nextTransNum = Program.CoinNode.GetNextTransactionNumber(wallet.publicKey);
        return Ok(new WalletBalance(wallet.publicKey, bal, nextTransNum));
    }

    [HttpPost("transfer")]
    public IActionResult SendMoney([FromBody] Transaction transaction) {
        if (!Program.CoinNode.ValidateTransaction(transaction)) {
            return BadRequest("Invalid signature.");
        }

        if (transaction.Sender.SequenceEqual(new byte[32])) {  // Coinbase
            return BadRequest("Sender cannot be coinbase.");
        }
        
        Program.CoinNode.PublishTransaction(transaction);
        return Ok();
    }
}