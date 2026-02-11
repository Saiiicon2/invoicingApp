using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// REFERENCIAS
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using PointOfSale.Model;

namespace PointOfSale.Data.Repository
{
    public class SaleItemRepository : ISaleItemRepository
    {
        private readonly POINTOFSALEContext _context;

        public SaleItemRepository(POINTOFSALEContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<SaleItem>> QueryAsync(Expression<Func<SaleItem, bool>> predicate)
        {
            return await Task.FromResult(_context.SaleItems.Where(predicate).AsQueryable());
        }

        public async Task<IQueryable<SaleItem>> Query(Expression<Func<SaleItem, bool>> predicate)
        {
            return await Task.FromResult(_context.SaleItems
                .Include(si => si.Sale)    // Include related Sale for RegistrationDate
                .Where(predicate)
                .AsQueryable());
        }
    }
}
