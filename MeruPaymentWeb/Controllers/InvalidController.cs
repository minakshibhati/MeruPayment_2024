using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MeruPaymentWeb.Controllers
{
    public class InvalidController : Controller
    {
        // GET: Invalid
        public ActionResult Index()
        {
            ViewBag.Message = "400-Invalid request.";
            return View();
        }
    }
}