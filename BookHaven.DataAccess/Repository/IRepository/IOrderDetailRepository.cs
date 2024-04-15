using BookHaven.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository :IRepository<OrderDetail>   
    {
        void Update(OrderDetail orderDetail);
    }
}
