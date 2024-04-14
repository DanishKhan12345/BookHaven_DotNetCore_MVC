using BookHaven.DataAccess.Data;
using BookHaven.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository categoryRepository { get; private set; }
        public IProductRepository productRepository { get; private set; }
        public ICompanyRepository companyRepository { get; private set; }
        public IShoppingCartRepository shoppingCartRepository { get; private set; }
        public IApplicationUserRepository applicationUserRepository { get; private set; }
        public IOrderHeaderRepository orderHeaderRepository { get; private set; }
        public IOrderDetailRepository orderDetailRepository { get; private set; }
        private readonly AppDbContext _appDbContext;
        public UnitOfWork(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
            categoryRepository = new CategoryRepository(_appDbContext);
            productRepository = new ProductRepository(_appDbContext);
            companyRepository = new CompanyRepository(_appDbContext);
            shoppingCartRepository = new ShoppingCartRepository(_appDbContext);
            applicationUserRepository = new ApplicationUserRepository(_appDbContext);
            orderHeaderRepository = new OrderHeaderRepository(_appDbContext);
            orderDetailRepository = new OrderDetailRepository(_appDbContext);
        }

        public void Save()
        {
            _appDbContext.SaveChanges();
        }

        public void Dispose() 
        {
            _appDbContext?.Dispose();
        }
    }
}
