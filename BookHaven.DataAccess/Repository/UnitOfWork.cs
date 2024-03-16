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
        private AppDbContext _appDbContext;
        public UnitOfWork(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
            categoryRepository = new CategoryRepository(appDbContext);
        }

        public void Save()
        {
            _appDbContext.SaveChanges();
        }
    }
}
