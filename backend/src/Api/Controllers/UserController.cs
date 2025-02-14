using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models;

namespace Api.Controllers
{
    [Route("api/users")]
    public class UserController : BaseController<User, IUserService>
    {
        public UserController(IUserService service) : base(service) { }
    }
}