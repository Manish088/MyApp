using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicatonDbContext _context;
        public CategoryRepository(ApplicatonDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            var categorDb=_context.categories.FirstOrDefault(m=>m.Id == category.Id);
            if(categorDb != null)
            {
                
                categorDb.Name = category.Name;
                categorDb.DisplayOrder = category.DisplayOrder;
                
            }
            
        }
    }
}
