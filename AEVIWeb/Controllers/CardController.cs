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
    public class CardController : Controller
    {
        int pageSize = 20;
       // public static STCardVP param;
        
        //
        // GET: /Card/


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
                    STCardVP param;
                    if (Session["CARDPARAM"] != null)
                        param = (STCardVP)Session["CARDPARAM"];
                    else param = new STCardVP();
                    
                    if (!SharedModel.IsConnect(LocalData.CSDbCards1(), out msg) && !SharedModel.IsConnect(LocalData.CSDbCards2(), out msg))
                    {
                        ViewData["ERROR"] = "No connection to DB";
                        ViewData["MSG"] = msg;

                        //return RedirectToAction("Index", "Error");
                        return View("Index");
                    }
                    else
                    {
                        string[] arr = new[] { "'", "\"", "--" };
                        if (CheckerField.CheckField(arr, param.maskaccount, param.maskedcompanyname, param.maskeddrivername, param.maskedemail, param.maskedpan, param.maskedvrn,
                            param.masksubaccount, param.expdate))
                        {
                            ViewData["MSG"] = "One or more fields contain invalid characters.";
                            return View("Errors1");
                        }
                        else
                        {
                            List<CardModels> lst = CardModelsRepository.Instance.GetListCard(param);
                            ViewData["PageNum"] = pageNum;
                            ViewData["ItemsCount"] = lst.Count;
                            ViewData["PageSize"] = pageSize;
                            ViewData["STRPARAM"] = param.strdata;

                            return View(CardModelsRepository.Instance.GetListCard(pageSize, pageNum, param));
                        }
                    }
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Card/Details/5

        public ActionResult Details(string pan)
        {
            if (Request.IsAuthenticated)
            {
                return View(CardModelsRepository.Instance.GetCard(pan));
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Card/Create

        public ActionResult Create()
        {
            if (Request.IsAuthenticated)
            {
                return View(new CardModels());
            }
            else return RedirectToAction("Index", "Home");
        } 

        //
        // POST: /Card/Create

        [HttpPost]
        public ActionResult Create(CardModels model)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string msg;
                    if (!CardModelsRepository.Instance.CreateCard(model, out msg))
                    {
                        ViewData["MSG"] = msg;
                        return View("Errors");
                    }
                    else

                        // CardModelsRepository.Instance.CreateFakeCard();
                        return View("Details", CardModelsRepository.Instance.GetCard(model.Pan));

                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }
        
        //
        // GET: /Card/Edit/5

        public ActionResult Edit(string pan)
        {
            if (Request.IsAuthenticated)
            {
                CardModels card = CardModelsRepository.Instance.GetCard(pan);
                return View(card);
                
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Card/Edit/5

        [HttpPost]
        public ActionResult Edit(string pan, CardModels model)
        {

            if (Request.IsAuthenticated)
            {
                try
                {
                    string msg;
                    if (!CardModelsRepository.Instance.UpdateCard(model, out msg))
                    {
                        ViewData["MSG"] = msg;
                        return View("Errors");
                    }
                    else
                    {
                        CardModels newcard = CardModelsRepository.Instance.GetCard(pan);
                        return View("Details", newcard);
                    }
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Card/Delete/5
 
        public ActionResult Delete(string pan)
        {
            if (Request.IsAuthenticated)
            {
                CardModels card = CardModelsRepository.Instance.GetCard(pan);
                return View(card);
            }
            else return RedirectToAction("Index", "Home");
        }

     //
        // POST: /Card/Delete/5

        [HttpPost]
        public ActionResult Delete(string pan, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string msg;
                    if (!CardModelsRepository.Instance.DeleteCard(pan, out msg))
                    {
                        ViewData["MSG"] = msg;
                        return View("Errors");
                    }
                    else
                        return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult Filter()
        {
            if (Request.IsAuthenticated)
            {
                //STCardVP param = LocaParam.cardparam;
                STCardVP param;
                if (Session["CARDPARAM"] != null)
                    param = (STCardVP)Session["CARDPARAM"];
                else param = new STCardVP();
                CardModelsViewParam prm = CardModelsRepository.Instance.SetParam(param);
                return View("Filter", prm);
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Filter(CardModelsViewParam prm, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    STCardVP param = CardModelsRepository.Instance.GetParam(prm);
                    Session["CARDPARAM"] = param;
                  
                    return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult Clearfilter()
        {
            if (Request.IsAuthenticated)
            {
                STCardVP param = new STCardVP();
                Session["CARDPARAM"] = param;
                try
                {
                 //   STUser user = UserModelsRepository.Instance.GetLocalUser();
                  
                   // LocaParam.SetParam(null);
                    return RedirectToAction("List");
                }
                catch { return View(param); }
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (Request.IsAuthenticated)
            {
                if (upload != null)
                {
                    // получаем имя файла
                    string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                }
                return RedirectToAction("Index");
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult Unregistred(string pan)
        {
            if (Request.IsAuthenticated)
            {
                CardModels card = CardModelsRepository.Instance.GetCard(pan);
                return View(card);
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Unregistred(string pan, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string msg;
                    if (!CardModelsRepository.Instance.UnRegistredCard(pan, out msg))
                    {
                        ViewData["MSG"] = msg;
                        return View("Errors");
                    }
                    else
                        return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult BlockListCard(FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string msg;
                    STCardVP param;
                    if (Session["CARDPARAM"] != null)
                        param = (STCardVP)Session["CARDPARAM"];
                    else param = new STCardVP();
                  //  Session["CARDPARAM"] = param;
                    if (!CardModelsRepository.Instance.BlockListCard(param, out msg))
                    {
                        ViewData["MSG"] = msg;
                    }
                    else
                    {
                        ViewData["MSG"] = "Cards is successfully blocked";
                    }

                    return View();
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
