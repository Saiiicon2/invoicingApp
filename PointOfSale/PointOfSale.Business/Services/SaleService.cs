using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly ISaleRepository _repositorySale;
        private readonly ISaleItemRepository _repositorySaleItem;
        public SaleService(IGenericRepository<Product> repositoryProduct, ISaleRepository repositorySale, ISaleItemRepository repositorySaleItem)
        {
            _repositoryProduct = repositoryProduct;
            _repositorySale = repositorySale;
            _repositorySaleItem = repositorySaleItem;
        }

        public async Task<List<Product>> GetProducts(string search)
        {
            IQueryable<Product> query = await _repositoryProduct.Query(p =>
           p.IsActive == true &&
           p.Quantity > 0 &&
           string.Concat(p.BarCode, p.Brand, p.Description).Contains(search)
           );

            return query.Include(c => c.IdCategoryNavigation).ToList();
        }

        public async Task<List<Product>> GetTop3PopularProducts()
        {
            try
            {
                // Define StartDate as 7 days ago
                var startDate = DateTime.Now.Date.AddDays(-7);

                // Query SaleItems where sales occurred in the past week
                IQueryable<SaleItem> saleItemsQuery = await _repositorySaleItem.Query(si =>
                    si.Sale.RegistrationDate.Value.Date >= startDate);

                // Group by ProductId, order by the number of sales, and take top 3
                var topProductIds = saleItemsQuery
                    .GroupBy(si => si.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        SaleCount = g.Count()
                    })
                    .OrderByDescending(g => g.SaleCount)
                    .Take(3)
                    .Select(g => g.ProductId)
                    .ToList();

                // Fetch product details for the top 3 products
                var query = await _repositoryProduct.Query(p => topProductIds.Contains(p.IdProduct));

                var topProducts = await query
                    .Include(p => p.IdCategoryNavigation)  // Include related category data
                    .ToListAsync();

                return topProducts;
            }
            catch
            {
                throw;
            }
        }




        public async Task<int> GetProductCount()
        {
            // IQueryable<Product> query = await _repositoryProduct.Query(p =>
            //p.IsActive == true &&
            //p.Quantity > 0 &&
            //string.Concat(p.BarCode, p.Brand, p.Description).Contains(search)
            //);

            // return query.Include(c => c.IdCategoryNavigation).ToList();
            return 4;
        }

        public async Task<int> GetSalesCount()
        {
            // IQueryable<Product> query = await _repositoryProduct.Query(p =>
            //p.IsActive == true &&
            //p.Quantity > 0 &&
            //string.Concat(p.BarCode, p.Brand, p.Description).Contains(search)
            //);

            // return query.Include(c => c.IdCategoryNavigation).ToList();
            return 4;
        }

        public async Task<Sale> Register(Sale entity)
        {
            try
            {
                return await _repositorySale.Register(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Sale>> SaleHistory(string SaleNumber, string StarDate, string EndDate)
        {
            IQueryable<Sale> query = await _repositorySale.Query();
            StarDate = StarDate is null ? "" : StarDate;
            EndDate = EndDate is null ? "" : EndDate;

            if (StarDate != "" && EndDate != "")
            {

                DateTime start_date = DateTime.ParseExact(StarDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime end_date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

                return query.Where(v =>
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date
                )
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .ToList();
            }
            else
            {
                return query.Where(v => v.SaleNumber == SaleNumber)
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .ToList();
            }
        }

        public async Task<Sale> Detail(string SaleNumber)
        {
            IQueryable<Sale> query = await _repositorySale.Query(v => v.SaleNumber == SaleNumber);

            return query
               .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
               .Include(u => u.IdUsersNavigation)
               .Include(dv => dv.DetailSales)
               .First();
        }

        public async Task<List<DetailSale>> Report(string StartDate, string EndDate)
        {
            DateTime start_date = DateTime.ParseExact(StartDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
            DateTime end_date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

            List<DetailSale> lista = await _repositorySale.Report(start_date, end_date);

            return lista;
        }

        //Task<List<Product>> ISaleService.GetProducts(string search)
        //{
        //    throw new NotImplementedException();
        //}

       

        //Task<int> ISaleService.GetProductCount()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<int> ISaleService.GetSalesCount()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<Sale> ISaleService.Register(Sale entity)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<Sale>> ISaleService.SaleHistory(string SaleNumber, string StarDate, string EndDate)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<Sale> ISaleService.Detail(string SaleNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<List<DetailSale>> ISaleService.Report(string StarDate, string EndDate)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<List<Product>> GetPopularProducts()
        {
            throw new NotImplementedException();
        }
    }
}
