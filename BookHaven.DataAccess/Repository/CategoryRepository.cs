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
    public class CategoryRepository : Repository<Category>,ICategoryRepository
    {
        private AppDbContext _appDbContext;
        public CategoryRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
       
        public void Update(Category category)
        {
            _appDbContext.Update(category);
        }
    }
}
