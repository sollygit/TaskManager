using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Controllers
{
    [Authorize]
    [Route("/api/customers")]
    public class CustomerController : Controller
    {
        public IActionResult Get()
        {
            return Ok(new[] { "One", "Two", "Three" });
        }
    }
}
