using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UFOU.Data;
using UFOU.Models;

namespace UFOU.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly UFOContext _context;
        private readonly UsersRolesContext _userContext;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoriteController(UFOContext context, UsersRolesContext userContext, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userContext = userContext;
            _userManager = userManager;
        }

        // GET: Favorite
        public async Task<IActionResult> Index()
        {
            dynamic models = new ExpandoObject();
            _userManager.GetUserId(this.User);
            var favorites = await _context.Favorites.Where(user => user.UserID.Equals(_userManager.GetUserId(User))).ToListAsync();


            //List<Report> reports = new List<Report>();
            List<Tuple<Favorite, Report>> fr = new List<Tuple<Favorite, Report>>();
            foreach (var f in favorites)
            {
                var report = _context.Reports.Where(r => r.ReportId == f.ReportID).FirstOrDefault();
                fr.Add(new Tuple<Favorite, Report>(f, report));
                //reports.Add(report);
            }


            models.FR = fr;
            models.Favorites = favorites;
            //models.Reports = reports;

            return View(models);
        }

        // GET: Favorite/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(m => m.ID == id);
            if (favorite == null)
            {
                return NotFound();
            }

            return View(favorite);
        }

        // GET: Favorite/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Favorite/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,UserID,ReportID")] Favorite favorite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(favorite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(favorite);
        }

        [HttpPost]
        public JsonResult AddFavorite(int reportID)
        {
            // see if user already has report favorited
            var userfavs = _context.Favorites.Where(f => f.UserID == _userManager.GetUserId(User) && f.ReportID == reportID).FirstOrDefault();

            if (userfavs == null)
            {
                Favorite fav = new Favorite
                {
                    UserID = _userManager.GetUserId(User),
                    ReportID = reportID
                };

                _context.Favorites.Add(fav);
                _context.SaveChanges();
            }
            

            return Json(new { success = true });
        }

        // GET: Favorite/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }
            return View(favorite);
        }

        // POST: Favorite/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,UserID,ReportID")] Favorite favorite)
        {
            if (id != favorite.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favorite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavoriteExists(favorite.ID))
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
            return View(favorite);
        }

        // GET: Favorite/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(m => m.ID == id);
            if (favorite == null)
            {
                return NotFound();
            }

            return View(favorite);
        }

        // POST: Favorite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FavoriteExists(int id)
        {
            return _context.Favorites.Any(e => e.ID == id);
        }
    }
}
