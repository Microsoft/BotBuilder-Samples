using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsMessagingExtensionsAction.Controllers
{
    public class HomeController : Controller
    {
        [Route("/Home/RazorView")]
        public ActionResult RazorView()
        {
            return View("RazorView");
        }

        [Route("CustomForm")]
        public ActionResult CustomForm(int empId, string empName, string empEmail)
        {
            return View("CustomForm");
        }
    }
}