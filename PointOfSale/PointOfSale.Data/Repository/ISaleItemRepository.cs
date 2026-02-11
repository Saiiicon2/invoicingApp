using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Data.Repository
{
    public interface ISaleItemRepository
    {
        Task<IQueryable<SaleItem>> QueryAsync(System.Linq.Expressions.Expression<Func<SaleItem, bool>> predicate);
        Task<IQueryable<SaleItem>> Query(Expression<Func<SaleItem, bool>> predicate);
    }
}
