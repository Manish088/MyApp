using MyApp.DataAccessLayer.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicatonDbContext _context;
        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }

        public ICartRepository Cart { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public UnitOfWork(ApplicatonDbContext context)
        {
            _context = context;
            Category=new CategoryRepository(context);
            Product=new ProductRepository(context);
            Cart=new CartRepository(context);
            OrderDetail=new OrderDetailRepository(context);
            OrderHeader=new OrderHeaderRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
        }
      

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
