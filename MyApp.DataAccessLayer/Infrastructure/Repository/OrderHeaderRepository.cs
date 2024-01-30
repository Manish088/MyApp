using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicatonDbContext _context;
        public OrderHeaderRepository(ApplicatonDbContext context) : base(context)
        {
            _context = context;
        }

        public void PaymentStatus(int id, string SessionId, string PaymentIntentId)
        {
            var orderHeader = _context.orderheaders.FirstOrDefault(x => x.Id == id);
            orderHeader.DateOfPayment=DateTime.Now;
            orderHeader.PaymentIntentId = PaymentIntentId;
            orderHeader.SessionId = SessionId;
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.orderheaders.Update(orderHeader);
           /* var categorDb=_context.categories.FirstOrDefault(m=>m.Id == category.Id);
            if(categorDb != null)
            {
                
                categorDb.Name = category.Name;
                categorDb.DisplayOrder = category.DisplayOrder;
                
            }*/
            
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var order=_context.orderheaders.FirstOrDefault(m=>m.Id==id);    
            if(order!=null)
            {
                order.OrderStatus = orderStatus;
            }
            if(paymentStatus!=null)
            {
                order.PaymentStatus=paymentStatus;
            }
        }
    }
}
