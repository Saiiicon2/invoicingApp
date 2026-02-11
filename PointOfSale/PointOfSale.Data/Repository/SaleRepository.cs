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
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        private readonly POINTOFSALEContext _dbcontext;
        public SaleRepository(POINTOFSALEContext context) : base(context)
        {
            _dbcontext = context;
        }

        public async Task<Sale> Register(Sale entity)
        {

            Sale SaleGenerated = new Sale();
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    foreach (DetailSale dv in entity.DetailSales)
                    {
                        Product product_found = _dbcontext.Products.Where(p => p.IdProduct == dv.IdProduct).First();

                        product_found.Quantity = product_found.Quantity - dv.Quantity;
                        _dbcontext.Products.Update(product_found);
                    }
                    await _dbcontext.SaveChangesAsync();

                    // Invoice numbering: start at INV400 on a fresh DB.
                    // Correlative.LastNumber stores the numeric part. We generate the final id as INV{number}.
                    var correlative = _dbcontext.CorrelativeNumbers.FirstOrDefault(n => n.Management == "Sale");
                    if (correlative == null)
                    {
                        correlative = new CorrelativeNumber
                        {
                            Management = "Sale",
                            LastNumber = 399,
                            QuantityDigits = 3,
                            DateUpdate = DateTime.UtcNow
                        };
                        _dbcontext.CorrelativeNumbers.Add(correlative);
                        await _dbcontext.SaveChangesAsync();
                    }

                    correlative.LastNumber = (correlative.LastNumber ?? 399) + 1;
                    correlative.DateUpdate = DateTime.UtcNow;

                    _dbcontext.CorrelativeNumbers.Update(correlative);
                    await _dbcontext.SaveChangesAsync();

                    entity.SaleNumber = $"INV{correlative.LastNumber}";

                    await _dbcontext.Sales.AddAsync(entity);
                    await _dbcontext.SaveChangesAsync();

                    SaleGenerated = entity;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return SaleGenerated;
        }

        public async Task<List<DetailSale>> Report(DateTime StarDate, DateTime EndDate)
        {
            List<DetailSale> listSummary = await _dbcontext.DetailSales
                .Include(v => v.IdSaleNavigation)
                .ThenInclude(u => u.IdUsersNavigation)
                .Include(v => v.IdSaleNavigation)
                .ThenInclude(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Where(dv => dv.IdSaleNavigation.RegistrationDate.Value.Date >= StarDate.Date && dv.IdSaleNavigation.RegistrationDate.Value.Date <= EndDate.Date)
                .ToListAsync();

            return listSummary;
        }
    }
}
