
using Ecom_Project_1145.DataAccess.Repository.IRepository;
using Ecom_Project_1145.model;
using Ecom_Project_1145.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_Project_1145.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public class CategoryController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;
            public CategoryController(IUnitOfWork UnitOfWork)
            {
                _unitOfWork = UnitOfWork;
            }

            public IActionResult Index()
            {
                return View();
            }
            #region APIs
            [HttpGet]
            public IActionResult GetAll()
            {
                var categoryList = _unitOfWork.Category.GetAll();
                return Json(new { data = categoryList });
            }
            [HttpDelete]
            public IActionResult Delete(int id)
            {
                var CategoryInDb = _unitOfWork.Category.Get(id);
                if (CategoryInDb == null) return Json(new { success = false, Message = "Something Went Wrong!!!" });
                _unitOfWork.Category.Remove(CategoryInDb);
                _unitOfWork.save();
                return Json(new { success = true, Message = "Data Deleted Successfully" });
            }
            #endregion
            public IActionResult Upsert(int? id)
            {
                Category category = new Category();
                if (id == null) return View(category);
               category = _unitOfWork.Category.Get(id.GetValueOrDefault());
                if (category == null) return NotFound();
                return View(category);
            }    
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Upsert(Category category)
            {
                if (category == null) return BadRequest();
                if (!ModelState.IsValid) return View(category);
                if (category.Id== 0)
                    _unitOfWork.Category.Add(category);
                else
                    _unitOfWork.Category.Update(category);
                _unitOfWork.save();
                return RedirectToAction(nameof(Index));
            }
        }
    }
