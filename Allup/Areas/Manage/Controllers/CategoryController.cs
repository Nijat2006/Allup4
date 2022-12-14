using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();
            return View(categories);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();
            return View();
          
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if (await _context.Categories.AnyAsync(c=>c.IsDeleted == false &&c.Name.ToLower()== category.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name",$"This Name = {category.Name} Alreade Exists");
                return View(category);

            }


            if (category.IsMain)
            {
                category.ParentId = null;
                if (category.File==null)
                {
                    ModelState.AddModelError("File", "is required");
                    return View();
                }
                if(category.File.ContentType != "image/png")
                {
                    ModelState.AddModelError("File", "file type should be jpeg or jpg");
                    return View();
                }
                if (category.File.Length>20000)
                {
                    ModelState.AddModelError("File", "file length should be less than 20k");
                    return View();
                }
                string originalFileName = category.File.FileName.Substring(category.File.FileName.LastIndexOf("."), 4);
                string fileName = Guid.NewGuid().ToString() + "-" + DateTime.UtcNow.AddHours(4).ToString("yyyymmddhhmmss") + originalFileName;
                string path = @"C:\Users\irade\source\repos\new\AlluP_ASP.NetCoreWithMVC\Allup\wwwroot\assets\images\" + fileName;
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    await category.File.CopyToAsync(fileStream);
                }
                category.ParentId = null;
                category.Image = fileName;
               
            }
            else
            {
                if(category.ParentId == null)
                {
                    ModelState.AddModelError("ParenId", "parent should be choosen");
                    return View(category);
                }
                if (!await _context.Categories.AnyAsync(c=>c.IsDeleted==false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "choose the vorrect parent category");
                    return View(category);
                }
                category.Image = null;
            }

            category.Name = category.Name.Trim();
            category.IsDeleted = false;
            category.CreatedAt = DateTime.UtcNow.AddHours(4);
            category.CreatedBy = "System";
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
       
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id==null)
            {
                return BadRequest("id can not be null");
            }
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);
            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower() == category.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", $"This Name = {category.Name} Alreade Exists");
                return View(category);

            }


            if (category==null)
            {
                return NotFound("can not find category with this id");
            }
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            ViewBag.Categories = await _context.Categories
             .Where(c => c.IsDeleted == false && c.IsMain)
             .ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            if (id == null)
            {
                return BadRequest("id ola bilmer");

            }
            if(await _context.Categories.AnyAsync(c=> c.IsDeleted ==false && c.Name.ToLower()== category.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", $"This Name = {category.Name} Alreade Exists");
                return View(category);

            }
            if (id== null)
            {
                return BadRequest("id bos ola Bilmez");
            }
            Category existedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);
            if (existedCategory == null)
            {
                return NotFound("can not find category with this id");
            }
            if (category.Id != id)
            {
                return BadRequest("id can not be null");

            }
            if (category.IsMain)
            {
                category.ParentId = null;
                if (category.File == null)
                {
                    ModelState.AddModelError("File", "is required");
                    return View();
                }
                if (category.File.ContentType != "image/png")
                {
                    ModelState.AddModelError("File", "file type should be jpeg or jpg");
                    return View();
                }
                if (category.File.Length > 20000)
                {
                    ModelState.AddModelError("File", "file length should be less than 20k");
                    return View();
                }
                string originalFileName = category.File.FileName.Substring(category.File.FileName.LastIndexOf("."), 4);
                string fileName = Guid.NewGuid().ToString() + "-" + DateTime.UtcNow.AddHours(4).ToString("yyyymmddhhmmss") + originalFileName;
                string path = @"C:\Users\irade\source\repos\new\AlluP_ASP.NetCoreWithMVC\Allup\wwwroot\assets\images\" + fileName;
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    await category.File.CopyToAsync(fileStream);
                }
                existedCategory.ParentId = null;
                existedCategory.Image = fileName;

            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParenId", "parent should be choosen");
                    return View(category);
                }
                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "choose the vorrect parent category");
                    return View(category);
                }
                existedCategory.Image = null;
            }
            existedCategory.IsMain = category.IsMain;
            existedCategory.Name = category.Name;
            existedCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);
            existedCategory.UpdatedBy = "System";
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("this id could not be found");
            }
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);
            if (category==null)
            {
                return NotFound("can not find category with this id");
            }

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow.AddHours(4);
            category.DeletedBy = "System";
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> ShowDetail(int? id)
        {
            if (id == null)
            {
                return BadRequest("id can not be null");
            }
            Category category = await _context.Categories.FirstOrDefaultAsync(C => C.IsDeleted == false && C.Id == id);
            if (category == null)
            {
                return NotFound("can not find category with this id");
            }
            ViewBag.Categories = await _context.Categories
               .Where(c => c.IsDeleted == false && c.IsMain==false && c.ParentId == id)
               .ToListAsync();
            return View(category);
        }
    }
}
