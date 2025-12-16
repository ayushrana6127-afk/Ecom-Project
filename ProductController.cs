    using Ecom_Project_1145.DataAccess.Repository.IRepository;
    using Ecom_Project_1145.model;
    using Ecom_Project_1145.model.ViewModel;
    using Ecom_Project_1145.Utility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    namespace Ecom_Project_1145.Areas.Admin.Controllers
    {
       
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public class ProductController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IWebHostEnvironment _webHostEnvironment;



            public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
            {
                _unitOfWork = unitOfWork;
                _webHostEnvironment = webHostEnvironment;
            }
            public IActionResult Index()
            {
                return View();
            }
            #region APIs
            [HttpGet]
            public IActionResult GetAll()
            {
                return Json(new { data = _unitOfWork.Product.GetAll() });
            }
            [HttpDelete]
            public IActionResult Delete(int id)
            {
                var ProductInDb = _unitOfWork.Product.Get(id);
                if (ProductInDb == null)
                    return Json(new { success = false, message = "Unable to delete data !!!" });
                //Image Delete
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, ProductInDb.ImageUrl.Trim('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _unitOfWork.Product.Remove(ProductInDb);
                _unitOfWork.save();
                return Json(new { success = true, message = "Data Deleted SuccessFully !!!" });
            }


            #endregion


            public IActionResult Upsert(int? id)
            {
                ProductVM productVM = new ProductVM()
                {
                    Product = new model.Product(),

                    CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })

                };
                if (id == null) return View(productVM);
                productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
                if (productVM.Product == null) return NotFound();
                return View(productVM);
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Upsert(ProductVM productVM)
            {
                if (ModelState.IsValid)
                {
                    var WebRootpath = _webHostEnvironment.WebRootPath;
                    var filesname = HttpContext.Request.Form.Files;

                    if (filesname.Count() > 0)
                    {
                        var fileName = Guid.NewGuid().ToString();
                        var extension = Path.GetExtension(filesname[0].FileName);
                        var Uploads = Path.Combine(WebRootpath, "images\\Products");
                        if (productVM.Product.id != 0)
                        {
                            var imageExists = _unitOfWork.Product.Get(productVM.Product.id).ImageUrl;
                            productVM.Product.ImageUrl = imageExists;
                        }
                        if (productVM.Product.ImageUrl != null)
                        {
                            var imagePath = Path.Combine(WebRootpath, productVM.Product.ImageUrl.Trim('\\'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                        using (var filestream = new FileStream(Path.Combine(Uploads, fileName + extension), FileMode.Create))
                        {

                            filesname[0].CopyTo(filestream);
                        }
                        productVM.Product.ImageUrl = @"\images\Products\" + fileName + extension;

                    }
                    else
                    {
                        if (productVM.Product.id != 0)
                        {
                            var imageExists = _unitOfWork.Product.Get(productVM.Product.id).ImageUrl;
                            productVM.Product.ImageUrl = imageExists;
                        }
                    }
                    if (productVM.Product.id == 0)
                        _unitOfWork.Product.Add(productVM.Product);
                    else
                        _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.save();
                    return RedirectToAction(nameof(Index));

                }
                else
                {
                    productVM = new ProductVM()
                    {
                        Product = new model.Product(),

                        CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                        {
                            Text = cl.Name,
                            Value = cl.Id.ToString()
                        }),
                        CoverTypeList = _unitOfWork.CoverType.GetAll().Select(ct => new SelectListItem()
                        {
                            Text = ct.Name,
                            Value = ct.Id.ToString()
                        })
                    };
                    if (productVM.Product.id != 0)
                    {
                        productVM.Product = _unitOfWork.Product.Get(productVM.Product.id);
                    }
                    return View(productVM);
                }
            }
        }
    }