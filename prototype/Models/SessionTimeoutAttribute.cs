using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace prototype.Models
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        private readonly IHttpContextAccessor accessor;

        public SessionTimeoutAttribute(IHttpContextAccessor _accessor)
        {
            accessor = _accessor;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (accessor.HttpContext.Session.GetString("LoginTimeStamp") == null)
            {
                filterContext.Result = new RedirectResult("~/Home/LoginMsg");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
