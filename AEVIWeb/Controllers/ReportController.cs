using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AEVIWeb.Models;
using AEVIDomain;

namespace AEVIWeb.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/

        public ActionResult Indexd()
        {
            if (Request.IsAuthenticated)
            {
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;
                string msg;
                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);

                if (!stUser.oldpass)
                {
                    ReportParamModels param = new ReportParamModels();
                    param.BeginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
                    DateTime end = DateTime.Now;
                    end = end.AddMonths(1);
                    end = new DateTime(end.Year, end.Month, 1, 0, 0, 0, 0);
                    end = end.AddDays(-1);
                    param.EndDate = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 0);
                    return View(param);
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public ActionResult Indexd(ReportParamModels model)
        {
            if (Request.IsAuthenticated)
            {
                List<ReportModels> lst = ReportRepository.Instance.GetReport(model);

                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;
                string msg;
                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);

                ViewData["USERNAME"] = stUser.username;
                return View("List", lst);
            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
