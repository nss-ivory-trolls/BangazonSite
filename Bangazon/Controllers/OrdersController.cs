using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Authorization;

namespace Bangazon.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);


        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }
            var userid = user.Id;
            var applicationDbContext = _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .Where(o => o.UserId == userid && o.PaymentTypeId != null);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Orders/cart
        public async Task<IActionResult> Cart()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            var userid = user.Id;
            var applicationDbContext = _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.UserId == userid && o.PaymentTypeId == null);
            var seeData = await applicationDbContext.ToListAsync();
            return View(seeData);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.OrderProducts)
                .ThenInclude(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

      

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null || order.PaymentTypeId != null)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType.Where(p => p.UserId == user.Id), "PaymentTypeId", "PaymentMethod");
            //ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("userId");
            var user = await GetCurrentUserAsync();
            order.UserId = user.Id;

            DateTime today = DateTime.UtcNow;
            order.DateCompleted = today;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        public async Task<IActionResult> RemoveProductFromOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.OrderProduct
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("RemoveProductFromOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }
            
            var orderProducts = _context.OrderProduct;
            foreach (OrderProduct item in orderProducts)
            {
                if (item.ProductId == id)
                {
                    orderProducts.Remove(item);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.OrderProducts)
                .ThenInclude(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }
            var userid = user.Id;
            var order = await _context.Order.FindAsync(id);
            var orderProducts = _context.OrderProduct;
            foreach (OrderProduct item in orderProducts)
            {
                if (item.OrderId == order.OrderId && userid == order.UserId)
                {
                    orderProducts.Remove(item);
                }
            }

            if (userid == order.UserId)
            {
            _context.Order.Remove(order);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> AddToCart([FromRoute] int id)
        {
            Product productToAdd = await _context.Product.SingleOrDefaultAsync(p => p.ProductId == id);

            var user = await GetCurrentUserAsync();

            var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);

            if (openOrder == null)
            {
                var order = new Order();
                order.UserId = user.Id;
                order.DateCreated = DateTime.UtcNow;
                _context.Add(order);

                var orderProduct = new OrderProduct();
                orderProduct.ProductId = productToAdd.ProductId;
                orderProduct.OrderId = order.OrderId;
                _context.Add(orderProduct);
                await _context.SaveChangesAsync();
            }
            else
            {
                var orderProduct = new OrderProduct();
                orderProduct.ProductId = productToAdd.ProductId;
                orderProduct.OrderId = openOrder.OrderId;
                _context.Add(orderProduct);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Cart));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}
