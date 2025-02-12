using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Data;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Controllers
{
    public class TKMKsController : Controller
    {
        private readonly QLSieuThiWebContext _context;

        public TKMKsController(QLSieuThiWebContext context)
        {
            _context = context;
        }

        // GET: TKMKs
        public async Task<IActionResult> Index()
        {
            return View(await _context.TKMK.ToListAsync());
        }

        // GET: TKMKs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tKMK = await _context.TKMK
                .FirstOrDefaultAsync(m => m.TK == id);
            if (tKMK == null)
            {
                return NotFound();
            }

            return View(tKMK);
        }

        // GET: TKMKs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TKMKs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TK,MK")] TKMK tKMK)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tKMK);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tKMK);
        }

        // GET: TKMKs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tKMK = await _context.TKMK.FindAsync(id);
            if (tKMK == null)
            {
                return NotFound();
            }
            return View(tKMK);
        }

        // POST: TKMKs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TK,MK")] TKMK tKMK)
        {
            if (id != tKMK.TK)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tKMK);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TKMKExists(tKMK.TK))
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
            return View(tKMK);
        }

        // GET: TKMKs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tKMK = await _context.TKMK
                .FirstOrDefaultAsync(m => m.TK == id);
            if (tKMK == null)
            {
                return NotFound();
            }

            return View(tKMK);
        }

        // POST: TKMKs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tKMK = await _context.TKMK.FindAsync(id);
            if (tKMK != null)
            {
                _context.TKMK.Remove(tKMK);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TKMKExists(string id)
        {
            return _context.TKMK.Any(e => e.TK == id);
        }
    }
}
