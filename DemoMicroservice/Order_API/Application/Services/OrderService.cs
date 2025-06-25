using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order_API.Domain.Entities;
using Order_API.Domain.Interfaces;

namespace Order_API.Application.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IInventoryClientService _inventoryClientService;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IInventoryClientService inventoryClientService)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _inventoryClientService = inventoryClientService;
        }

        // Lấy tất cả Order
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        // Lấy Order theo Id
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        // Thêm Order (không nhận OrderDetail)
        public async Task<Order> AddAsync(Order order)
        {
            if (order.UserID == Guid.Empty)
                throw new ArgumentException("UserID is required");
            if (string.IsNullOrWhiteSpace(order.PaymentMethod))
                throw new ArgumentException("PaymentMethod is required");

            var createdOrder = await _orderRepository.AddAsync(order);
            return createdOrder;
        }

        // Cập nhật Order
        public async Task<Order> UpdateAsync(Guid id, Order order)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
                throw new KeyNotFoundException($"Order {id} not found");

            // Chỉ cập nhật các trường được cung cấp
            if (order.UserID != Guid.Empty)
                existingOrder.UserID = order.UserID;
                
            if (order.OrderDate != default)
                existingOrder.OrderDate = order.OrderDate;
                
            if (!string.IsNullOrEmpty(order.PaymentMethod))
                existingOrder.PaymentMethod = order.PaymentMethod;
                
            if (!string.IsNullOrEmpty(order.Status))
                existingOrder.Status = order.Status;

            await _orderRepository.UpdateAsync(existingOrder);
            return existingOrder;
        }

        // Xóa Order và OrderDetail liên quan
        public async Task RemoveAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order {id} not found");

            var orderDetails = await _orderDetailRepository.GetAllAsync();
            var detailsToDelete = orderDetails.Where(od => od.OrderId == id).ToList();
            foreach (var detail in detailsToDelete)
                await _orderDetailRepository.DeleteAsync(detail.Id);

            await _orderRepository.DeleteAsync(id);
        }

        // CRUD cho OrderDetail
        public async Task<OrderDetail?> GetOrderDetailByIdAsync(Guid id)
        {
            return await _orderDetailRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync()
        {
            return await _orderDetailRepository.GetAllAsync();
        }

        public async Task<OrderDetail> AddOrderDetailAsync(OrderDetail orderDetail)
        {
            if (orderDetail.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");
            if (orderDetail.Price < 0)
                throw new ArgumentException("Price must be >= 0");

            // Kiểm tra tồn kho
            var available = await _inventoryClientService.GetProductQuantityAsync(orderDetail.ProductId);
            if (available < orderDetail.Quantity)
                throw new InvalidOperationException($"Insufficient inventory for product {orderDetail.ProductId}. Available: {available}, Requested: {orderDetail.Quantity}");

            // Thêm OrderDetail
            var addedDetail = await _orderDetailRepository.AddAsync(orderDetail);

            // Cập nhật tồn kho
            var newQuantity = available - orderDetail.Quantity;
            var success = await _inventoryClientService.UpdateProductQuantityAsync(orderDetail.ProductId, newQuantity);
            if (!success)
                throw new InvalidOperationException($"Failed to update inventory for product {orderDetail.ProductId}");

            return addedDetail;
        }

        public async Task<OrderDetail> UpdateOrderDetailAsync(Guid id, OrderDetail orderDetail)
        {
            var existing = await _orderDetailRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"OrderDetail {id} not found");

            // Tính chênh lệch số lượng
            int quantityDiff = orderDetail.Quantity - existing.Quantity;

            // Kiểm tra tồn kho nếu tăng số lượng
            if (quantityDiff > 0)
            {
                var available = await _inventoryClientService.GetProductQuantityAsync(orderDetail.ProductId);
                if (available < quantityDiff)
                    throw new InvalidOperationException($"Insufficient inventory for product {orderDetail.ProductId}. Available: {available}, Requested: {quantityDiff}");
            }

            // Cập nhật OrderDetail
            existing.ProductId = orderDetail.ProductId;
            existing.Quantity = orderDetail.Quantity;
            existing.Price = orderDetail.Price;
            await _orderDetailRepository.UpdateAsync(existing);

            // Cập nhật tồn kho
            var currentInventory = await _inventoryClientService.GetProductQuantityAsync(orderDetail.ProductId);
            var newInventory = currentInventory - quantityDiff;
            var success = await _inventoryClientService.UpdateProductQuantityAsync(orderDetail.ProductId, newInventory);
            if (!success)
                throw new InvalidOperationException($"Failed to update inventory for product {orderDetail.ProductId}");

            return existing;
        }

        public async Task DeleteOrderDetailAsync(Guid id)
        {
            await _orderDetailRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            return await _orderDetailRepository.GetByOrderIdAsync(orderId);
        }
    }
} 