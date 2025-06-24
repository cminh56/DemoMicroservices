using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order_API.Domain.Entities;
using Order_API.Domain.Interfaces;
using Order_API.Infastructure.DataContext;

namespace Order_API.Infastructure.Repositories
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly OrderDbContext _context;

        public OrderDetailRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetail?> GetByIdAsync(Guid id)
        {
            return await _context.OrderDetails.FindAsync(id);
        }

        public async Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            return await _context.OrderDetails.ToListAsync();
        }

        public async Task<OrderDetail> AddAsync(OrderDetail orderDetail)
        {
            await _context.OrderDetails.AddAsync(orderDetail);
            await _context.SaveChangesAsync();
            return orderDetail;
        }

        public async Task UpdateAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Update(orderDetail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
            }
        }
    }
} 