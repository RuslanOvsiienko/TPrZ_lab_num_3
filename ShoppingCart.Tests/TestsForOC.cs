using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Utility;
using ShoppingCart.Web.Areas.Admin.Controllers;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class TestsForOC
    {
        //#1, Перевірка на те чи корректно працює метод OrderDetails з урахуванням очікуваного результату
        [Fact]
        public void OrderDetails_ReturnsCorrectOrder()
        {
            var orderHeader = new OrderHeader { Id = 1 };
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = 1, ProductId = 1 },
                new OrderDetail { Id = 2, OrderHeaderId = 1, ProductId = 2 }
            };
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                .Returns(orderHeader);
            unitOfWorkMock.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<string>()))
                .Returns(orderDetails.AsQueryable());
            var controller = new OrderController(unitOfWorkMock.Object);

            var result = controller.OrderDetails(1);

            Assert.Equal(orderHeader.Id, result.OrderHeader.Id);
            Assert.Equal(orderDetails.Count, result.OrderDetails.Count());
        }

        //#2, Перевірка правильності роботи методу OrderDetails контролера OrderController для визначеного orderId, включаючи коректність завантаження даних з репозиторіїв і правильність створення об'єкта OrderVM
        [Fact]
        public void OrderDetails_ValidId_ReturnsOrderVM()
        {
            // Arrange
            int orderId = 1;
            var mockOrderHeaderRepository = new Mock<IOrderHeaderRepository>();

            // Замість іменованого аргументу використовуємо рядок для включення властивостей
            mockOrderHeaderRepository.Setup(repo => repo.GetT(x => x.Id == orderId, "ApplicationUser"))
                .Returns(new OrderHeader() { Id = orderId, ApplicationUserId = "123" });

            var mockOrderDetailRepository = new Mock<IOrderDetailRepository>();

            // Замінюємо іменований аргумент рядком
            mockOrderDetailRepository.Setup(repo => repo.GetAll("Product"))
                .Returns(new List<OrderDetail>() { new OrderDetail() { OrderHeaderId = orderId } });

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.OrderHeader).Returns(mockOrderHeaderRepository.Object);
            mockUnitOfWork.Setup(uow => uow.OrderDetail).Returns(mockOrderDetailRepository.Object);
            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.OrderDetails(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.OrderHeader.Id);
            Assert.True(result.OrderDetails.Any());
        }

        //#3, Перевірка того, що якщо статус платежу не підтверджений, метод SetToCancelOrder контролера OrderController скасовує замовлення без повернення коштів
        [Fact]
        public void SetToCancelOrder_WhenPaymentStatusIsNotApproved_CancelsOrderWithoutRefund()
        {
            // Arrange
            var orderHeaderId = 1;
            var orderHeader = new OrderHeader { Id = orderHeaderId, PaymentStatus = PaymentStatus.StatusRejected };
            var vm = new OrderVM { OrderHeader = orderHeader };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
            .Returns(orderHeader);

            var refundServiceMock = new Mock<Stripe.RefundService>();
            var controller = new OrderController(unitOfWorkMock.Object);

            controller.SetToCancelOrder(vm);

            unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
            refundServiceMock.Verify(x => x.Create(It.IsAny<RefundCreateOptions>(), null), Times.Never);
        }
    }
}
