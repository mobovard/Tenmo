using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("user/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountDAO accountDAO;
        public AccountController(IAccountDAO accountDAO)
        {
            this.accountDAO = accountDAO;
        }


        [HttpGet]
        public ActionResult<Account> GetAccount()
        {
            // grabs ID from authorized JWT and tries to parse it into a userId
            if (int.TryParse(User.FindFirst("sub")?.Value, out int userId)) 
            {
                // creates user account object and passes it back
                Account userAccount = accountDAO.GetAccount(userId);
                return Ok(userAccount);
            }
            else
            {
                // if unable to parse subject line of JWT into an ID
                return NotFound("Invalid User ID");
            }
        }
    }
}
