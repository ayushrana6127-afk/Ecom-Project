using Ecom_Project_1145.DataAccess.Repository.IRepository;
using Ecom_Project_1145.model;
using Ecom_Project_1145.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_Project_1145.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CompanyController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitofwork.Company.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var CompanyInDb = _unitofwork.Company.Get(id);
            if (CompanyInDb == null)
                return Json(new { success = false, Message = "unable to delete Data!!!" });
            _unitofwork.Company.Remove(CompanyInDb);
            _unitofwork.save();
            return Json(new { success = true, Message = "Deleted Successfully" });
        }
        #endregion
        public IActionResult upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            company = _unitofwork.Company.Get(id.GetValueOrDefault());
            if (company == null) return BadRequest();
            return View(company);
        }
        [HttpPost]
        public IActionResult upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return View(company);
            if (company.Id == 0)
                _unitofwork.Company.Add(company);
            else
                _unitofwork.Company.Update(company);
            _unitofwork.save();
            return RedirectToAction(nameof(Index));

        }
    }
}