using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class MenuUserViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        // 1x1 transparent PNG to avoid broken image URLs when no photo exists.
        private const string TransparentPngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMB/axY9jUAAAAASUVORK5CYII=";

        public MenuUserViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            ClaimsPrincipal claimuser = HttpContext.User;


            string userName = "";
            string photoUser = "";
            string emailUser = "";

            if (claimuser.Identity.IsAuthenticated)
            {
                userName = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();

                var idUserString = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .SingleOrDefault();

                if (int.TryParse(idUserString, out var idUser))
                {
                    User user_found = await _userService.GetById(idUser);
                    if (user_found?.Photo != null && user_found.Photo.Length > 0)
                    {
                        photoUser = Convert.ToBase64String(user_found.Photo);
                    }
                    else
                    {
                        photoUser = TransparentPngBase64;
                    }
                }
                else
                {
                    photoUser = TransparentPngBase64;
                }

                emailUser = ((ClaimsIdentity)claimuser.Identity).FindFirst("Email")?.Value ?? string.Empty;
            }

            ViewData["userName"] = userName;
            ViewData["photoUser"] = photoUser;
            ViewData["emailUser"] = emailUser;

            return View();
        }
    }
}
