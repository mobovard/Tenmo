using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserDAO userDAO;
        public UserController(IUserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        [HttpGet]
        public ActionResult<List<ShortUser>> GetUsers()
        {
            var users = userDAO.GetUsers();
            if (!(users is null))
            {
                return Ok(users);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
