using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("User/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUserDAO userDAO;
        private readonly IAccountDAO accountDAO;
        public AccountController(IUserDAO userDAO, IAccountDAO accountDAO)
        {
            this.userDAO = userDAO;
            this.accountDAO = accountDAO;
        }


        [HttpGet("{userId}")]
        public ActionResult<Account> GetAccount(int userId)
        {
            Account userAccount = accountDAO.GetAccount(userId);
            //string test = User.FindFirst("UserId").Value;
            return Ok(userAccount);
        }
    }
}
