using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bangazon.Models;
using Bangazon.Data;
using Microsoft.EntityFrameworkCore;

namespace Bangazon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public ViewResult IndexSearch(string searchString)
        {
            var Product = from p in _context.Product
                          select p;
            if (!string.IsNullOrEmpty(searchString))
            {
                Product = _context.Product.Where(p => p.Title.Contains(searchString));
            }

            return View(Product.ToList());
        }

        public ViewResult IndexSearchCity(string searchString)
        {
            var Product = from p in _context.Product
                          select p;
            if (!string.IsNullOrEmpty(searchString))
            {
                Product = _context.Product.Where(p => p.City.Contains(searchString));
            }

            return View(Product.ToList());
        }




        public async Task<IActionResult> Index()
        {
            var list = (from p in _context.Product
                        orderby p.DateCreated descending
                        select p).Take(20);
            return View(await list.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
