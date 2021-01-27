using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AEVIWeb.Models
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Index(string msg)
        {

            ModelState.AddModelError("", msg);
            //ViewData["MSG"] = msg;
            return View();
        }

    }
}
