using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicatonDbContext _context;
        public OrderDetailRepository(ApplicatonDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetail orderDetail)
        {

            _context.orderdetails.Update(orderDetail);
            /*var categorDb=_context.categories.FirstOrDefault(m=>m.Id == category.Id);
            if(categorDb != null)
            {
                
                categorDb.Name = category.Name;
                categorDb.DisplayOrder = category.DisplayOrder;
                
            }*/
            
        }
    }
}
