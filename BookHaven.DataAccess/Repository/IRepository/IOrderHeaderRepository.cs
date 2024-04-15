using BookHaven.Models;
using BookHaven.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository :IRepository<OrderHeader>   
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, ShipmentStatus orderStatus, PaymentStatus? paymentStatus = PaymentStatus.Idle);
        void UpdateStripePaymentId(int id,string sessionId, string paymentIntentId);
    }
}
