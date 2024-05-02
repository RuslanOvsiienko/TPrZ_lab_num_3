using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using Xunit;

namespace ShoppingCart.Tests
{
    public class TestsForCC
    {
        /*---------------------------- GET ----------------------------*/

        //#1, Імітування IUnitOfWork та перевірка, чи метод Get повертає всі категорії
        [Fact]
        public void GetCategories_All_ReturnAllCategories()
        {
            // Arrange
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();
            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(() => CategoryDataset.Categories);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.Equal(CategoryDataset.Categories, result.Categories);
        }

        /*---------------------------- CREATE ----------------------------*/

        //#2, Перевірка чи метод CreateUpdate з дійсною моделлю створює нову категорію та зберігає зміни
        [Fact]
        public void CreateCategory_ValidModel_SavesCategory()
        {
            // Arrange
            var category = new Category { Name = "Test Category" };
            var categoryVM = new CategoryVM { Category = category };
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(categoryVM);

            // Assert
            repositoryMock.Verify(r => r.Add(It.Is<Category>(c => c.Name == category.Name)));
            mockUnitOfWork.Verify(uow => uow.Save());
        }

        //#3, Перевірка, чи CreateUpdate генерує виключення, коли модель є недійсною
        [Fact]
        public void CreateUpdate_InvalidModel_ThrowsException()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var controller = new CategoryController(mockUnitOfWork.Object);
            controller.ModelState.AddModelError("Name", "Name is required");
            var vm = new CategoryVM();

            // Act & Assert
            Assert.Throws<Exception>(() => controller.CreateUpdate(vm));
        }

        /*---------------------------- DELETE ----------------------------*/

        //#4, Перевірка того, що метод DeleteData з несуіснуючим ID кидає виняток
        [Fact]
        public void DeleteCategory_NonexistentId_ThrowsException()
        {
            // Arrange
            int nonExistentId = 12;
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();
            repositoryMock.Setup(r => r.Delete(null))
                .Throws<Exception>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            Assert.Throws<Exception>(() => controller.DeleteData(nonExistentId));

            // Assert
            // Перевірка викликів методів не потрібна, оскільки виняток кидається раніше
        }

        /*---------------------------- UPDATE ----------------------------*/

        //#5, Імітація `IUnitOfWork` і перевірка того, що `CreateUpdate` оновлює існуючу категорію для дійсної моделі з ідентифікатором.
        [Fact]
        public void UpdateCategory_ValidModel_UpdatesCategory()
        {
            // Arrange
            int categoryId = 2;
            var category = new Category { Id = categoryId, Name = "Updated Name" };
            var vm = new CategoryVM { Category = category };
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(vm);

            // Assert
            repositoryMock.Verify(r => r.Update(It.Is<Category>(c => c.Id == categoryId && c.Name == category.Name)));
            mockUnitOfWork.Verify(uow => uow.Save());
        }

    }
}
