using BookHaven.DataAccess.Data;
using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext _appDbContext;
        public OrderHeaderRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
       
        public void Update(OrderHeader orderHeader)
        {
            _appDbContext.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, ShipmentStatus orderStatus, PaymentStatus? paymentStatus = PaymentStatus.Idle)
        {
            var orderFromDb = _appDbContext.OrderHeaders.FirstOrDefault(x=>x.Id == id);
            if (orderFromDb != null) 
            {
                orderFromDb.OrderStatus = orderStatus;
                if (paymentStatus != PaymentStatus.Idle)
                {
                    orderFromDb.PaymentStatus = (PaymentStatus)paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _appDbContext.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionId)) 
            {
                orderFromDb.SessionId = sessionId; //sesiionid only generated when user tries to make a payment
            }
            if (!string.IsNullOrEmpty(paymentIntentId)) 
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
