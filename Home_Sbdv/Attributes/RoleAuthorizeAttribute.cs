using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace Home_Sbdv.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Check if user has required role
            bool authorized = false;
            var userRole = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

            foreach (var role in _allowedRoles)
            {
                if (string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase))
                {
                    authorized = true;
                    break;
                }
            }

            if (!authorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}