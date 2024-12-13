using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;

            if (session != null && session.IsNewSession)
            {
                string sessionCookie = filterContext.HttpContext.Request.Headers["Cookie"];
                if ((sessionCookie != null) && (sessionCookie.IndexOf("ASP.NET_SessionId") >= 0))
                {
                    // Redirect to Dashboard controller and Dashboard action
                    filterContext.Result = new RedirectResult("~/Vl_operation_Head/SessionTimeout");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}