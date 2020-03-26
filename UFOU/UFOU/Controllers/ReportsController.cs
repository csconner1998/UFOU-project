using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UFOU.Data;
using UFOU.Models;
using static UFOU.Models.ShapeUtility;

namespace UFOU.Controllers
{
    public class ReportsController : Controller
    {
        private readonly UFOContext _context;

        public ReportsController(UFOContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reports.OrderBy(r => r.Location).OrderBy(r => r.DateOccurred).ToListAsync());
        }

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        public IActionResult Scraper()
        {
            return View();
        }

        public JsonResult RunScraper(int batchSize)
        {
            var scraper = UFOScraper.GetSingletonScraper();
            scraper.BatchSize = 100;

            // turn on the scraper if its not already on
            if (scraper.IsRunning)
                return new JsonResult("{ \"success\":false, msg:\"Scraper is already running!\" }");

            var currentIds = _context.Reports.Select(r => r.ReportId).ToHashSet();
            new TaskFactory()
                .StartNew(() =>
                {
                    try { return scraper.GetReports(currentIds); }
                    catch (Exception) { }
                    return new List<Report>();

                }).ContinueWith((result) =>
                {
                    if (result.IsCompletedSuccessfully)
                    {
                        var newReports = result.Result;
                        try
                        {
                            _context.Reports.AddRange(newReports);
                            _context.SaveChanges();
                        }
                        catch (Exception) { }
                    }
                });

            return new JsonResult(new { success = true });

        }

        public JsonResult HaltScraper()
        {
            UFOScraper.GetSingletonScraper().Halt = true;

            return new JsonResult("{ \"success\":true }");

        }

        public async Task<IActionResult> ReportApprovals()
        {
            return View(await _context.Reports.Where(r => r.Approved == false)
                .OrderBy(r => r.DateOccurred).OrderBy(r => r.Location).ToListAsync());
        }

        // GET: Reports/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Location,Shape,Duration,Description,DateOccurred,DateSubmitted")] Report report)
        {
            if (ModelState.IsValid)
            {
                report.DatePosted = DateTime.Today;
                _context.Add(report);

                // add to location database
                Location l = _context.Locations.Where(lo => lo.Name.Equals(report.Location)).FirstOrDefault();

                if (l == null)
                {
                    l = new Location
                    {
                        Name = report.Location,
                        MostCommonShape = report.Shape,
                        Sightings = 1
                    };
                    BarGraph b = new BarGraph
                    {
                        Shape = report.Shape,
                        Location = report.Location,
                        Quantity = 1
                    };

                    _context.Locations.Add(l);
                    _context.BarGraphs.Add(b);
                }
                else // location exists so update bargraph and most common shape
                {
                    // update location sightings
                    l.Sightings++;

                    BarGraph bargraph = _context.BarGraphs.Where(b => b.Location.Equals(l.Name) && b.Shape.Equals(report.Shape)).FirstOrDefault();

                    if (bargraph == null)
                    {
                        BarGraph b = new BarGraph
                        {
                            Shape = report.Shape,
                            Location = report.Location,
                            Quantity = 1
                        };

                        l.MostCommonShape = report.Shape;

                        _context.BarGraphs.Add(b);
                    }
                    else
                    {
                        // update the correct bargraphs quantity
                        bargraph.Quantity++;

                        // update the most common shape in location
                        var bargraphs = await _context.BarGraphs.ToListAsync();

                        int max = 0;
                        foreach (BarGraph b in bargraphs)
                        {
                            if (max < b.Quantity)
                            {
                                l.MostCommonShape = b.Shape;
                            }
                        }
                    }

                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }


        // GET: Reports/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ReportId,Location,Shape,Duration,Description,DateOccurred")] Report report)
        {
            if (id != report.ReportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(report);
                    //await _context.SaveChangesAsync();
                    // delete the report
                    await DeleteConfirmed(id);
                    _ = Create(report);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.ReportId))
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
            return View(report);
        }

        // GET: Reports/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            // remove from the map
            Location l = _context.Locations.Where(lo => lo.Name.Equals(report.Location)).FirstOrDefault();
            l.Sightings--;

            // remove from bar graph
            BarGraph bargraph = _context.BarGraphs.Where(b => b.Location.Equals(l.Name) && b.Shape.Equals(report.Shape)).FirstOrDefault();
            bargraph.Quantity--;

            if (bargraph.Quantity == 0)
            {
                _context.BarGraphs.Remove(bargraph);
            }

            // update the most common shape in location
            var bargraphs = await _context.BarGraphs.ToListAsync();

            int max = 0;
            foreach (BarGraph b in bargraphs)
            {
                if (max < b.Quantity)
                {
                    l.MostCommonShape = b.Shape;
                }
            }

            // remove report from favorites lists
            var deletefavs = await _context.Favorites.Where(f => f.ReportID == id).ToListAsync();
            foreach (Favorite favs in deletefavs)
            {
                _context.Favorites.Remove(favs);
                await _context.SaveChangesAsync();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.ReportId == id);
        }
    }
}
