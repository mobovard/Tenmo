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
            if (newTransfer.TransferTypeId == TransferTypes.REQUEST && newTransfer.TransferStatusId != TransferStatus.PENDING)
            {
                return BadRequest("Requests must be created in Pending status");
            }
            // grabs ID from authorized JWT and tries to parse it into a userId
            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                if (newTransfer.TransferTypeId == TransferTypes.SEND)
                {
                    Account userAccount = accountDAO.GetAccount(userId);
                    Account toAccount = accountDAO.GetAccount(newTransfer.ToUserId);
                    if (toAccount != null)
                    {
                        if (userAccount.Balance >= newTransfer.Amount)
                        {
                            // transfer can be made
                            newTransfer.AccountFrom = userAccount.AccountId;
                            newTransfer.AccountTo = toAccount.AccountId;

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
                    Account userAccount = accountDAO.GetAccount(userId);
                    Account fromAccount = accountDAO.GetAccount(newTransfer.FromUserId);
                    if (fromAccount != null)
                    {
                        newTransfer.AccountFrom = fromAccount.AccountId;
                        newTransfer.AccountTo = userAccount.AccountId;

                        // post transfer to Sql
                        Transfer addedTransfer = transferDAO.AddTransfer(newTransfer);
                        return Created($"/user/transfer/{addedTransfer.TransferId}", addedTransfer);
                    }
                    else
                    {
                        //if toUser account doesn't exist (ie, bad user ID was entered)
                        return BadRequest("Invalid User ID");
                    }
                }
            }
            else
            {
                // if unable to parse subject line of JWT into an ID
                return NotFound("Invalid User ID");
            }
        }

        [HttpGet]
        public ActionResult<List<Transfer>> GetTransfersForUser(string status)
        {
            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {

                TransferStatus? ts = null;

                if (status != null)
                {
                    if (status.ToLower() == "pending")
                    {
                        ts = TransferStatus.PENDING;
                    }
                    else if (status.ToLower() == "approved")
                    {
                        ts = TransferStatus.APPROVED;
                    }
                    else if (status.ToLower() == "rejected")
                    {
                        ts = TransferStatus.REJECTED;
                    }
                }
                //get list transfers
                List<Transfer> transfers = transferDAO.GetTransfersForUser(userId, ts);
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

        [HttpPut("{transferId}")]
        public ActionResult<Transfer> UpdateTransferStatus(int transferId, Transfer transfer) // TODO: check balance
        {
            Transfer dbTransfer = transferDAO.GetTransferById(transferId);

            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                if (transfer.FromUserId == userId)
                {
                    if (dbTransfer.TransferStatusId == TransferStatus.PENDING && dbTransfer.TransferId == transfer.TransferId && transfer.ToUserId == dbTransfer.ToUserId && dbTransfer.Amount == transfer.Amount)
                    {
                        Transfer t = transferDAO.UpdateTransferStatus(transfer);

                        TransferFunds(t);
                        return Ok(t);
                    }
                    else
                    {
                        return BadRequest("Invalid request");
                    }
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
