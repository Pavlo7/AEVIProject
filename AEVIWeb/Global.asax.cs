using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AEVIDomain;

namespace AEVIWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute("Activate",
                "Account/Update/Activate/{key}",
                new { controller = "Account", action = "Activate", key = UrlParameter.Optional }
            );

            routes.MapRoute("FPS",
               "Account/Update/FPS/{key}",
               new { controller = "Account", action = "FPS", key = UrlParameter.Optional }
           );

         //   routes.MapRoute("Images",   "Images/{key}");
          
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            string channelsarray =  LocalData.ChannelsArray();
            string channels = LocalData.ChannelsName();

            string[] channelsArray = channelsarray.Split('/');
            MPChannels.Init(channelsArray);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            HttpContext ctx = HttpContext.Current;
            Exception ex = ctx.Server.GetLastError();
            ctx.Response.Clear();

            RequestContext rc = ((MvcHandler)ctx.CurrentHandler).RequestContext;
            

            var httpException = ex as HttpException;
            if (httpException != null)
            {
                int ec = httpException.GetHttpCode();
                switch (ec)
                {
                    case 404:
                        string smsg = string.Format("Invalid url ({0})", ec, Request.Url);
                    CUdpSender clUDp = new CUdpSender(LocalData.Host(), LocalData.Port(), LocalData.LogPath());
                    clUDp.Send(LocalData.Facility(), LocalData.TagId(), "UWA103", smsg);
                        break;
                

                    default:
          
                        break;
                }
            }
          //  else
          //  {
          //      string smsg = string.Format("{0}", httpException.Message);
          //      CUdpSender clUDp = new CUdpSender(LocalData.UPDEndPoint(), LocalData.LogPath());
          //      clUDp.Send(LocalData.UDPFacility(), LocalData.UDPSeverity(), LocalData.UDPHost(), LocalData.UDPTag(),
          //          LocalData.UDPIdMsg(), smsg);
          //  }

    
        } 

    }
}