using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitofWork;

        public CategoryController(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        [HttpGet]
        public CategoryVM Get()
        {
            CategoryVM categoryVM = new CategoryVM();
            categoryVM.Categories = _unitofWork.Category.GetAll();
            return categoryVM;
        }

        [HttpGet]
        public CategoryVM Get(int id)
        {
            CategoryVM vm = new CategoryVM();

            vm.Category = _unitofWork.Category.GetT(x => x.Id == id);

            return vm;
        }

        [HttpPost]
        public void CreateUpdate(CategoryVM vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.Category.Id == 0)
                {
                    _unitofWork.Category.Add(vm.Category);
                }
                else
                {
                    _unitofWork.Category.Update(vm.Category);
                }
                _unitofWork.Save();;
            }
            else
            {
                throw new Exception("Model is invalid");
            }
        }

        [HttpPost, ActionName("Delete")]
        public void DeleteData(int? id)
        {
            var category = _unitofWork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            _unitofWork.Category.Delete(category);
            _unitofWork.Save();
        }
    }
}
