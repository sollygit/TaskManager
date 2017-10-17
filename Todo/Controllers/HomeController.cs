using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using Microsoft.AspNetCore.Authorization;

namespace Todo.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "A simple TODO list example with user authentication in .NET core 2.0";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Solly's contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
