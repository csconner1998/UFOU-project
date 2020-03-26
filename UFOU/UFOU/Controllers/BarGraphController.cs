using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UFOU.Data;
using UFOU.Models;

namespace UFOU.Controllers
{
    public class BarGraphController : Controller
    {
        private readonly UFOContext _context;

        public BarGraphController(UFOContext context)
        {
            _context = context;
        }

        // GET: BarGraph
        public async Task<IActionResult> Index(string locationName)
        {
            return View(await _context.BarGraphs.Where(b => b.Location == locationName).ToListAsync());
        }

        // GET: BarGraph/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barGraph = await _context.BarGraphs
                .FirstOrDefaultAsync(m => m.ID == id);
            if (barGraph == null)
            {
                return NotFound();
            }

            return View(barGraph);
        }

        // GET: BarGraph/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BarGraph/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Shape,Quantity,Location")] BarGraph barGraph)
        {
            if (ModelState.IsValid)
            {
                _context.Add(barGraph);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(barGraph);
        }

        // GET: BarGraph/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barGraph = await _context.BarGraphs.FindAsync(id);
            if (barGraph == null)
            {
                return NotFound();
            }
            return View(barGraph);
        }

        // POST: BarGraph/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Shape,Quantity,Location")] BarGraph barGraph)
        {
            if (id != barGraph.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(barGraph);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BarGraphExists(barGraph.ID))
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
            return View(barGraph);
        }

        // GET: BarGraph/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barGraph = await _context.BarGraphs
                .FirstOrDefaultAsync(m => m.ID == id);
            if (barGraph == null)
            {
                return NotFound();
            }

            return View(barGraph);
        }

        // POST: BarGraph/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var barGraph = await _context.BarGraphs.FindAsync(id);
            _context.BarGraphs.Remove(barGraph);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BarGraphExists(int id)
        {
            return _context.BarGraphs.Any(e => e.ID == id);
        }
    }
}
