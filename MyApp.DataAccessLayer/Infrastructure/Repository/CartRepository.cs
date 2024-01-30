using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private ApplicatonDbContext _context;
        public CartRepository(ApplicatonDbContext context) : base(context)
        {
            _context = context;
        }

        public int DecrementItemCart(Cart cart, int count)
        {
            cart.Count-=count;
            return cart.Count;
        }

        public int IncrementItemCart(Cart cart, int count)
        {
            cart.Count+= count;
            return cart.Count;
        }
    }
}
