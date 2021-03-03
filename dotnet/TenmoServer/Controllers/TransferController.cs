using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("user/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly ITransferDAO transferDAO;
        private readonly IAccountDAO accountDAO;
        public TransferController(ITransferDAO transferDAO, IAccountDAO accountDAO)
        {
            this.transferDAO = transferDAO;
            this.accountDAO = accountDAO;
        }

        [HttpPost]
        public ActionResult<Transfer> AddTransfer(Transfer newTransfer)
        {
            // grabs ID from authorized JWT and tries to parse it into a userId
            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                // make sure user has funds for transfer
                Account userAccount = accountDAO.GetAccount(userId);
                if (userAccount.Balance >= newTransfer.Amount)
                {
                    // transfer can be made
                    // create transfer with 'approved status'
                    newTransfer.TransferTypeId = (int)TransferTypes.SEND;
                    newTransfer.TransferStatusId = (int)TransferStatus.APPROVED;
                    newTransfer.AccountFrom = userAccount.AccountId;

                    // ? adjust balances ? TODO
                    TransferFunds(newTransfer);

                    // post transfer to Sql
                    Transfer addedTransfer = transferDAO.AddTransfer(newTransfer);
                    return Created($"/user/transfer/{addedTransfer.TransferId}", addedTransfer);
                }
                else
                {
                    // return code of insufficient funds
                    return BadRequest("Insufficient Funds");
                }
            }
            else
            {
                // if unable to parse subject line of JWT into an ID
                return NotFound("Invalid User ID");
            }
        }

        private void TransferFunds(Transfer newTransfer)
        {
            if (newTransfer.TransferStatusId == (int)TransferStatus.APPROVED)
            {
                accountDAO.UpdateBalance(newTransfer.AccountFrom, -(newTransfer.Amount));
                accountDAO.UpdateBalance(newTransfer.AccountTo, newTransfer.Amount);
            }
        }
    }
}
