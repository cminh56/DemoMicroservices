using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Order_API.Domain.Entities;

namespace Order_API.Domain.Interfaces
{
    public interface IOrderDetailRepository
    {
        Task<OrderDetail?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderDetail>> GetAllAsync();
        Task<OrderDetail> AddAsync(OrderDetail orderDetail);
        Task UpdateAsync(OrderDetail orderDetail);
        Task DeleteAsync(Guid id);
    }
} 