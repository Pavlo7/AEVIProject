using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AEVIWeb.Models;
using AEVIDomain;
using System.Threading;

namespace AEVIWeb.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            string msg;
            if (!SharedModel.IsConnect(LocalData.CSDbUsers(), out msg))
            {
                ViewData["ERROR"] = "No connection to DB";
                ViewData["MSG"] = msg;

                //return RedirectToAction("Index", "Error");
                return View("Index");
            }

            return View();
        }


        /*
         * Возвращаемые значения
         * 0 - ОК
         * 1 - срок действия пароля закончен
         * 2 - usera не существует
         * 3 - user временно заблокирован
         * 4 - неверный пароль
         */
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            string msg = null;

            if (ModelState.IsValid)
            {
                int retcode = MembershipService.LogON(model.Login, model.Password, out msg);
                switch (retcode)
                {
                    case 0:
                        {
                            FormsService.SignIn(model.Login, model.RememberMe);
                            if (Url.IsLocalUrl(returnUrl))
                                return Redirect(returnUrl);
                            else
                                return RedirectToAction("Index", "Home");
                        }

                    case 1:
                        FormsService.SignIn(model.Login, model.RememberMe);
                        return RedirectToAction("ChangePassword");

                    default:
                        ModelState.AddModelError("", msg);
                        return View(model);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

       /* [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            string msg = null;

            if (ModelState.IsValid)
            {
            //    if (!MembershipService.ValidateUser(model.UserName, model.Password, out msg))
            //    {
            //        ModelState.AddModelError("", msg);
            //        return View(model);
            //    }

                if (!MembershipService.ValidatePass(model.Login, model.Password, out msg))
                {
                    Thread.Sleep(2000);
                    ModelState.AddModelError("", msg);
                    return View(model);
                }
                
                UserModels UM = UserModelsRepository.Instance.GetUserByLogin(model.Login);
                if (UM.Condition == "Blocked") msg = "The user has been blocked";
                if (UM.Condition == "Deleted") msg = "The user has been removed";
                if (UM.Condition == "Active")
                {
                    CUser clUser = new CUser(null, LocalData.CSDbUsers(), LocalData.LogPath());
                    STUser stUser;
                    clUser.GetRecordByUserLogin(model.Login, out stUser, out msg);

                    FormsService.SignIn(model.Login, model.RememberMe);

                    if (stUser.passvaliddate <= DateTime.Now) return RedirectToAction("ChangePassword");

                    CAction clAction = new CAction(stUser.userid, LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.LogON, null, out msg);
                    if (Url.IsLocalUrl(returnUrl))
                    {

                        return Redirect(returnUrl);
                    }
                    else
                    {

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                    ModelState.AddModelError("", msg);
               
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }*/

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            string msg;
            CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            clAction.AddAction(ActionType.LogOFF, null, out msg);

            FormsService.SignOut();
            Session.Abandon(); 

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************
        /*
        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (Request.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsService.SignIn(model.UserName, false  );
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);
            }
            else return RedirectToAction("Index", "Home");
        }
        */
        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            string msg;
            if (Request.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Если смена пароля прошла успешно перенапрявляем на ChangePasswordSuccess
                    if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword, out msg))
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", msg);
                    }
               
                }

                // If we got this far, something failed, redisplay form
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);
            }
            else return RedirectToAction("Index", "Home");
        }

      
        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            if (Request.IsAuthenticated)
            {
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }


        // **************************************
        // URL: /Account/Profile
        // **************************************

        public ActionResult Profile()
        {
            if (Request.IsAuthenticated)
            {
                return View(UserModelsRepository.Instance.GetUser(LocalData.UserId()));
            }
            else return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Update/Activate/key
        // **************************************
        public ActionResult Activate(string key)
        {
            string msg;
            CUser clUser = new CUser(null, LocalData.CSDbUsers(), LocalData.LogPath());
            STUser stUser;
            int ret = clUser.GetRecordByUserKey(key, out stUser, out msg);

            if (ret != 0) return RedirectToAction("Index", "Home");
            if (stUser.newemailkey != key) return RedirectToAction("Index", "Home");
            
            ActivateModel model = new ActivateModel();
            model.Key = key;
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        [HttpPost]
        public ActionResult Activate(ActivateModel model)
        {
            if (UserModelsRepository.Instance.IsSimplePassword(model.NewPassword))
            {
                ModelState.AddModelError("", "The new password is too simple. " +
                           "Passwords must be at least 8 characters in length. " +
                           "Passwords must contain: " +
                           "a minimum of 1 lower case letter [a-z] and a minimum of 1 upper case letter [A-Z] and " +
                           "a minimum of 1 numeric character [0-9] and a minimum of 1 special character: " +
                           "~`!@#$%^&*()-_+={}[]|\\;:\"<>,./?");
                return View("Activate", model);
            }
            else
            {
                if (UserModelsRepository.Instance.ActivateUser(model))
                    return RedirectToAction("LogON");
            }
          
            return RedirectToAction("Index", "Home");
        }


        // **************************************
        // URL: /Account/EnterLoginFPS/
        // **************************************

        public ActionResult EnterLoginFPS()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EnterLoginFPS(LogOnModel model)
        {
            string msg;
            CUser clUser = new CUser(null, LocalData.CSDbUsers(), LocalData.LogPath());
            STUser stUser;
            int ret = clUser.GetRecordByUserLogin(model.Login, out stUser, out msg);
            if (ret == 0 && stUser.login != null)
            {
                if (stUser.islock)
                {
                    ViewData["Information"] = string.Format("The user \"{0}\" has temporarily blocked for 30 minutes", stUser.login);
                    return View("Information");
                }
                else 
                    UserModelsRepository.Instance.SentLink(stUser);
            }

            ViewData["Information"] = "We sent a link to change the password to your email.";
            return View("Information");
        }

        // **************************************
        // URL: /Account/Update/FPS/key
        // **************************************
        public ActionResult FPS(string key)
        {
            string msg;
            CUser clUser = new CUser(null, LocalData.CSDbUsers(), LocalData.LogPath());
            STUser stUser;
            int ret = clUser.GetRecordByUserKey(key, out stUser, out msg);

            if (ret != 0) return RedirectToAction("Index", "Home");
            if (stUser.newemailkey != key) return RedirectToAction("Index", "Home");

            ActivateModel model = new ActivateModel();
            model.Key = key;
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View("Activate", model);
        }

        [HttpPost]
        public ActionResult FPS(ActivateModel model)
        {
            
            if (UserModelsRepository.Instance.IsSimplePassword(model.NewPassword))
            {
                ModelState.AddModelError("", "The new password is too simple. " +
                            "Passwords must be at least 8 characters in length. " +
                            "Passwords must contain: " +
                            "a minimum of 1 lower case letter [a-z] and a minimum of 1 upper case letter [A-Z] and " +
                            "a minimum of 1 numeric character [0-9] and a minimum of 1 special character: " +
                            "~`!@#$%^&*()-_+={}[]|\\;:\"<>,./?");
                return View("Activate", model);
            }
            else
            {
                if (UserModelsRepository.Instance.FPS(model))
                    return RedirectToAction("LogON");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
