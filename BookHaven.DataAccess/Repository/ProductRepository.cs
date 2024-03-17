using BookHaven.DataAccess.Data;
using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>,IProductRepository
    {
        private readonly AppDbContext _appDbContext;
        public ProductRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public void update(Product product)
        {
            _appDbContext.Products.Update(product);
        }
    }
}
