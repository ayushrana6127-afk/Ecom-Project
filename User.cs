using Ecom_Project_1145.DataAccess.Data;
using Ecom_Project_1145.DataAccess.Repository.IRepository;
using Ecom_Project_1145.model;
using Ecom_Project_1145.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_Project_1145.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class User: Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public User(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var UserList = _context.ApplicationUsers.ToList();//AspNetUser
            var RoleList = _context.Roles.ToList();//AspNetRole
            var UserRole = _context.UserRoles.ToList();//AspNetUserRole
            foreach (var user in UserList)
            {
                var roleId = UserRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = RoleList.FirstOrDefault(r => r.Id == roleId).Name;
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
                if (user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            //Remove Admin User From List
            var adminUser = UserList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            if (adminUser != null) UserList.Remove(adminUser);
            //***
            return Json(new { data = UserList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            bool islocked = false;
            var userInDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userInDb == null)
            {
                return Json(new { success = false, message = "Something Went wrong while Lock Unlock User!!!" });
            }
            if (userInDb != null && userInDb.LockoutEnd > DateTime.Now)
            {
                userInDb.LockoutEnd = DateTime.Now;
                islocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(100);
                islocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = islocked == true ? "User Successfully Locked" : "User Successfully Unlocked" });
        }
    }
}
#endregion