using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;

namespace AEVIWeb.Helpers
{
    public static class Paging
    {
        public static MvcHtmlString PaginNavigator(this HtmlHelper helper, int pageNum, int itemCount, int pageSize)
        {
            StringBuilder sb = new StringBuilder();
            if (pageNum > 0)
                sb.Append(helper.ActionLink("<", "List", new { pageNum = pageNum - 1 }));
            else sb.Append(HttpUtility.HtmlEncode("<"));
            sb.Append("  ");
            
            int pagesCount = (int)Math.Ceiling((double)itemCount / pageSize);

            if (pagesCount > 0)
                sb.Append(string.Format("Page {0} of {1}", pageNum+1, pagesCount));
            
            sb.Append("  ");

            if (pageNum < pagesCount-1)
                sb.Append(helper.ActionLink(">", "List", new { pageNum = pageNum + 1 }));
            else sb.Append(HttpUtility.HtmlEncode(">"));
            
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}