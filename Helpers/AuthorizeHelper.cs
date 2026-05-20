using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace restauranteswebsbasededatos.Helpers
{
    public class AuthorizeHelper : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeHelper(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UsuarioRol");
            
            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }
            
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}