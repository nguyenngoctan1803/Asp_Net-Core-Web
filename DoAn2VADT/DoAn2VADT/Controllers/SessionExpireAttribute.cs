
using DoAn2VADT.Shared;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DoAn2VADT.Controllers
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            
            // check  sessions here
            if (String.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(Const.CARTSESSION)))
            {
                httpContextAccessor.HttpContext.Session.SetString(Const.CARTSESSION, Guid.NewGuid().ToString());
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
