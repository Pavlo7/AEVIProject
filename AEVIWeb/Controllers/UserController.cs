using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AEVIWeb.Models;
using AEVIDomain;

namespace AEVIWeb.Controllers
{
    public class UserController : Controller
    {
        public IMembershipService MembershipService { get; set; }

        int pageSize = 20;
      //  public static STUserVP param;
        //
        // GET: /User/


        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                STUser user = UserModelsRepository.Instance.GetLocalUser();
                if (!user.oldpass)
                {
                    if (user.permission != 2)
                        return RedirectToAction("List");
                    else
                    {
                        ViewData["MSG"] = "The user with the permission \"STANDART\" can't operate the list's users";
                        return View("Permission1");
                    }
                }
                else return RedirectToAction("ChangePassword", "Account");
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult List(int pageNum = 0)
        {
            if (Request.IsAuthenticated)
            {
                STUserVP param;
                if (Session["USERPARAM"] != null)
                    param = (STUserVP)Session["USERPARAM"];
                else param = new STUserVP();

                string[] arr = new[] { "'", "\"", "--" };
                if (CheckerField.CheckField(arr, param.masklogin, param.maskusername, param.maskemail))
                {
                    ViewData["MSG"] = "One or more fields contain invalid characters.";
                    return View("Errors");
                }
                else
                {
                    List<UserModels> lst = UserModelsRepository.Instance.GetListUser(param);

                    ViewData["PageNum"] = pageNum;
                    ViewData["ItemsCount"] = lst.Count;
                    ViewData["PageSize"] = pageSize;
                    ViewData["STRPARAM"] = param.strdata;

                    return View(UserModelsRepository.Instance.GetListUser(pageSize, pageNum, param));
                }

            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /User/Details/5

        public ActionResult Details(string id)
        {
            if (Request.IsAuthenticated)
            {
                return View(UserModelsRepository.Instance.GetUser(id));
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            if (Request.IsAuthenticated)
            {
                STUser user = UserModelsRepository.Instance.GetLocalUser();
                if (user.permission == 0)
                    ViewData["PM"] = "0";
                return View(new UserModels());
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(UserModels model)
        {
            if (Request.IsAuthenticated)
            {
                string msg;

                // if (ModelState.IsValid)
                //  {
                int ret = UserModelsRepository.Instance.AddUser(model, out msg);

                if (ret == 0)
                {
                    return View("Details", model);
                }
                else
                {
                    ModelState.AddModelError("", msg);
                }

                //   }

                return View();
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(string id)
        {
            if (Request.IsAuthenticated)
            {
                UserModels UM = UserModelsRepository.Instance.GetUser(id);
                STUser user = UserModelsRepository.Instance.GetLocalUser();
                if (UM.OwnerUserId == user.userid)
                {
                    switch (UM.Condition)
                    {
                        case "Deleted":
                            {
                                ViewData["MSG"] = "The user is removed";
                                return View("Permission");
                            }
                            break;

                    }

                    if (user.permission == 0)
                        ViewData["PM"] = "0";
                    return View(UM);
                }
                else
                {
                    ViewData["MSG"] = "You can't edit the user because he doesn't belong to you";
                    return View("Permission");
                }
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        public ActionResult Edit(string id, UserModels model)
        {
            if (Request.IsAuthenticated)
            {
                string msg;

                int ret = UserModelsRepository.Instance.UpdateUser(model, out msg);

                if (ret == 0) return View("Details", model);
                else
                {
                    ModelState.AddModelError("", msg);
                }

                //   }
                UserModels UM = UserModelsRepository.Instance.GetUser(id);
                return View(UM);
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // GET: /User/Delete/5

        public ActionResult Delete(string id)
        {
            if (Request.IsAuthenticated)
            {
                UserModels UM = UserModelsRepository.Instance.GetUser(id);
                STUser user = UserModelsRepository.Instance.GetLocalUser();

                if (UM.OwnerUserId == user.userid)
                {
                    switch (UM.Condition)
                    {
                        case "Deleted":
                            {
                                ViewData["MSG"] = "The user is removed";
                                return View("Permission");
                            }
                    }
                    return View(UM);
                }
                else
                {
                    ViewData["MSG"] = "You can't remove the user because he doesn't belong to you";
                    return View("Permission");
                }
            }
            else return RedirectToAction("Index", "Home");
        }

        //
        // POST: /User/Delete/5

        [HttpPost]
        public ActionResult Delete(string id, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                UserModelsRepository.Instance.DeleteUser(id);
                return RedirectToAction("List");
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult Filter()
        {
            if (Request.IsAuthenticated)
            {
                STUserVP param;
                if (Session["USERPARAM"] != null)
                    param = (STUserVP)Session["USERPARAM"];
                else param = new STUserVP();

                UserModelsViewParam prm = UserModelsRepository.Instance.SetParam(param);
                STUser user = UserModelsRepository.Instance.GetLocalUser();
                if (user.permission == 0)
                    ViewData["PM"] = "0";
                return View(prm);
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Filter(UserModelsViewParam prm, FormCollection collection)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    STUserVP param = UserModelsRepository.Instance.GetParam(prm);
                    Session["USERPARAM"] = param;
                    return RedirectToAction("List");
                }
                catch { return View(); }
            }
            else return RedirectToAction("Index", "Home");
        }

        public ActionResult Clearfilter()
        {
            STUserVP param = new STUserVP();
            Session["USERPARAM"] = param;

            if (Request.IsAuthenticated)
            {
                try
                {
                   
                    return RedirectToAction("List");
                }
                catch { return View(param); }

            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
