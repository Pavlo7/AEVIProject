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
    public class TransactController : Controller
    {
        //
        // GET: /Transact/
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
                    STTransactVP param;
                    if (Session["TRANSACTPARAM"] != null)
                        param = (STTransactVP)Session["TRANSACTPARAM"];
                    else param = new STTransactVP();

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
                        if (CheckerField.CheckField(arr, param.maskedpan, param.maskedpos))
                        {
                            ViewData["MSG"] = "One or more fields contain invalid characters.";
                            return View("Errors");
                        }
                        else
                        {
                            List<TransactModels> lst = TransactModelsRepository.Instance.GetListTransact(param);
                            ViewData["PageNum"] = pageNum;
                            ViewData["ItemsCount"] = lst.Count;
                            ViewData["PageSize"] = pageSize;
                            ViewData["STRPARAM"] = param.strdata;

                            return View(TransactModelsRepository.Instance.GetListTransact(pageSize, pageNum, param));
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
                    STTransactVP param;
                    if (Session["TRANSACTPARAM"] != null)
                        param = (STTransactVP)Session["TRANSACTPARAM"];
                    else
                    {
                        param = new STTransactVP();
                      //  param.dtbegin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
                      //  DateTime end = DateTime.Now;
                      //  end = end.AddMonths(1);
                      //  end = new DateTime(end.Year, end.Month, 1, 0, 0, 0, 0);
                      //  end = end.AddDays(-1);
                      //  param.dtend = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 0);

                        param.dtbegin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                        param.dtend = DateTime.Now;
                    }
                    TransactModelsViewParam prm = TransactModelsRepository.Instance.SetParam(param);
                    return View("Filter", prm);
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Filter(TransactModelsViewParam prm, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    STTransactVP param = TransactModelsRepository.Instance.GetParam(prm);
                    Session["TRANSACTPARAM"] = param;

                    return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
