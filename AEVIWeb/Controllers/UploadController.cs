using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AEVIWeb.Models;
using AEVIDomain;
using System.IO;


namespace AEVIWeb.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return View();
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
                   
                    string msg;
                    List<STCard> list = new List<STCard>();

                    Stream stream = upload.InputStream;
               //     StreamReader sr = new StreamReader(stream);
               //     while (!sr.EndOfStream)
                //    {
                //        string line = sr.ReadLine();
               //     }
                    
                    
                    if (CardModelsRepository.Instance.ValidMassUpload(Path.GetFileName(upload.FileName), stream, out list, out msg))
                    {
                        CardModelsRepository.Instance.CreateCardMassUpload(upload.FileName,list,out msg);
                    }

                    ViewData["Information"] = "We had sent the report on your e_mail.";
                   
                }
                else ViewData["Information"] = "No files have been choosen";

                return View("Information");
            }
            else return RedirectToAction("Index", "Home");
        }
    }
}
