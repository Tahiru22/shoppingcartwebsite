using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Controllers
{
    public class BaseController<T> where T : BaseEntity
    {
        protected readonly DbContext _context;

        public BaseController(DbContext context)
        {
            _context = context;
        }
        public async Task<T?> GetAsync(int id)
           => await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

        public async Task<T[]> GetAll()
            => await _context.Set<T>().ToArrayAsync();

        public async Task<T> Create(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(int id)
        {
            var entity = _context.Set<T>()
                .FirstOrDefault(x => x.Id == id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
