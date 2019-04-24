using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Bangazon.Models.ProductViewModels;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;

namespace Bangazon.Controllers
{

    public class SharedController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public SharedController(ApplicationDbContext ctx,
                                  UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = ctx;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public ViewResult Layout(string searchString)
        {
            var Product = from p in _context.Product
                          select p;
            if (!string.IsNullOrEmpty(searchString))
            {
                Product = _context.Product.Where(p => p.City.Contains(searchString));
            }

            return View(Product.ToList());
        }
    }
}