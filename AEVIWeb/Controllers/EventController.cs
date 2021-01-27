using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AEVIWeb.Models;
using PaginationExample;
using AEVIDomain;

namespace AEVIWeb.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/
        int pageSize = 20;

        public ActionResult List(int pageNum = 0)
        {
            if (Request.IsAuthenticated)
            {
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;
                string msg;
                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);
                if (!stUser.oldpass)
                {
                    STEventVP param;
                    if (Session["EVENTPARAM"] != null)
                        param = (STEventVP)Session["EVENTPARAM"];
                    else param = new STEventVP();

                    if (!SharedModel.IsConnect(LocalData.CSDbTransacts1(), out msg)
                        && !SharedModel.IsConnect(LocalData.CSDbTransacts2(), out msg))
                    {
                        ViewData["ERROR"] = "No connection to DB";
                        ViewData["MSG"] = msg;

                        //return RedirectToAction("Index", "Error");
                        return View("Index");
                    }
                    else
                    {
                        string[] arr = new[] { "'", "\"", "--" };
                        if (CheckerField.CheckField(arr, param.maskedpan))
                        {
                            ViewData["MSG"] = "One or more fields contain invalid characters.";
                            return View("Errors");
                        }
                        else
                        {
                            List<EventModels> lst = EventModelsRepository.Instance.GetListEvent(param);
                            ViewData["PageNum"] = pageNum;
                            ViewData["ItemsCount"] = lst.Count;
                            ViewData["PageSize"] = pageSize;
                            ViewData["STRPARAM"] = param.strdata;

                            return View(EventModelsRepository.Instance.GetListEvent(pageSize, pageNum, param));
                        }
                    }
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }


        public ActionResult Filter()
        {
            if (Request.IsAuthenticated)
            {
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;
                string msg;
                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);
                if (!stUser.oldpass)
                {
                    //STCardVP param = LocaParam.cardparam;
                    STEventVP param;
                    if (Session["EVENTPARAM"] != null)
                        param = (STEventVP)Session["EVENTPARAM"];
                    else
                    {
                        param = new STEventVP();
                        //  param.dtbegin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
                        //  DateTime end = DateTime.Now;
                        //  end = end.AddMonths(1);
                        //  end = new DateTime(end.Year, end.Month, 1, 0, 0, 0, 0);
                        //  end = end.AddDays(-1);
                        //  param.dtend = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 0);

                        param.dtbegin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                        param.dtend = DateTime.Now;
                    }
                    EventModelsViewParam prm = EventModelsRepository.Instance.SetParam(param);
                    return View("Filter", prm);
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Filter(EventModelsViewParam prm, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    STEventVP param = EventModelsRepository.Instance.GetParam(prm);
                    Session["EVENTPARAM"] = param;

                    return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
