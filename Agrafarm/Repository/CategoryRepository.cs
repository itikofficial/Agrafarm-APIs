using Agrafarm.Data;
using Agrafarm.Interfaces;
using Agrafarm.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agrafarm.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all categories
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Category.ToListAsync();
        }

        // Get category by Id
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Category.FindAsync(id);
        }

        // Add new category
        public async Task AddAsync(Category category)
        {
            await _context.Category.AddAsync(category);
        }

        // Update existing category
        public void Update(Category category)
        {
            _context.Category.Update(category);
        }

        // Delete category
        public void Delete(Category category)
        {
            _context.Category.Remove(category);
        }

        // Save changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
