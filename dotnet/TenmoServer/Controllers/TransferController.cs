using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
                Account toAccount = accountDAO.GetAccount(newTransfer.ToUserId);
                if (toAccount != null)
                {
                    if (userAccount.Balance >= newTransfer.Amount)
                    {
                        // transfer can be made
                        // create transfer with 'approved status'
                        newTransfer.TransferTypeId = TransferTypes.SEND;
                        newTransfer.TransferStatusId = TransferStatus.APPROVED;
                        newTransfer.AccountFrom = userAccount.AccountId;
                        newTransfer.AccountTo = toAccount.AccountId;

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
                    //if toUser account doesn't exist (ie, bad user ID was entered)
                    return BadRequest("Invalid User ID");
                }
            }
            else
            {
                // if unable to parse subject line of JWT into an ID
                return NotFound("Invalid User ID");
            }
        }

        [HttpGet]
        public ActionResult<List<Transfer>> GetTransfersForUser()
        {
            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                //get list transfers
                List<Transfer> transfers = transferDAO.GetTransfersForUser(userId);
                if (transfers != null)
                {
                    return Ok(transfers);
                }
                else
                {
                    return BadRequest("Something went wrong");
                }
            }
            else
            {
                // if unable to parse subject line of JWT into an ID
                return NotFound("Invalid User ID");
            }
        }

        [HttpGet("{transferId}")]
        public ActionResult<Transfer> GetTransferById(int transferId)
        {
            Transfer transfer = transferDAO.GetTransferById(transferId);
            if (transfer != null)
            {
                if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
                {
                    Account userAccount = accountDAO.GetAccount(userId);

                    if (userAccount.AccountId == transfer.AccountFrom || userAccount.AccountId == transfer.AccountTo)
                    {
                        return Ok(transfer);
                    }
                    else
                    {
                        return StatusCode(403, "Access denied");
                    }
                }
                else
                {
                    return NotFound("Invalid User ID");
                }
            }
            else
            {
                return BadRequest("Invalid transfer ID");
            }
        }

        private void TransferFunds(Transfer newTransfer)
        {
            if (newTransfer.TransferStatusId == TransferStatus.APPROVED)
            {
                accountDAO.UpdateBalance(newTransfer.AccountFrom, -(newTransfer.Amount));
                accountDAO.UpdateBalance(newTransfer.AccountTo, newTransfer.Amount);
            }
        }
    }
}
