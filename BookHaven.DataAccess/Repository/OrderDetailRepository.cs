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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly AppDbContext _appDbContext;
        public OrderDetailRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
       
        public void Update(OrderDetail orderDetail)
        {
            _appDbContext.OrderDetails.Update(orderDetail);
        }
    }
}
