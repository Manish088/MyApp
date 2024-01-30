using Microsoft.EntityFrameworkCore;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicatonDbContext _context;
        private DbSet<T> _dbset;
        public Repository(ApplicatonDbContext context)
        {
            _context= context;
            _dbset=_context.Set<T>();

        }
        public void Add(T entity)
        {
            _dbset.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbset.RemoveRange(entities);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? Predicate,string? includeProperties = null)
        {
            IQueryable<T> Query = _dbset;
            if(Predicate!=null)
            {
                Query = Query.Where(Predicate);
            }
           
            if (includeProperties!=null)
            {
                foreach (var item in includeProperties.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    Query= Query.Include(item);
                }
            }
            return Query.ToList();
        }

        public T GetT(Expression<Func<T, bool>> Predicate, string? includeProperties = null)
        {
            IQueryable<T> Query = _dbset;
            Query=Query.Where(Predicate);
            if (includeProperties != null)
            {
                foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Query = Query.Include(item);
                }
            }
           
            return Query.FirstOrDefault();
        }
    }
}
