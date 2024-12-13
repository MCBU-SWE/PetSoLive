// /PetSoLive.Data/Repositories/AssistanceRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Interfaces;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data.Repositories
{
    public class AssistanceRepository : IRepository<Assistance>
    {
        private readonly ApplicationDbContext _context;

        public AssistanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Assistance entity)
        {
            await _context.Assistances.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Assistances.FindAsync(id);
            if (entity != null)
            {
                _context.Assistances.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Assistance>> GetAllAsync()
        {
            return await _context.Assistances.ToListAsync();
        }

        public async Task<Assistance> GetByIdAsync(int id)
        {
            return await _context.Assistances.FindAsync(id);
        }

        public async Task UpdateAsync(Assistance entity)
        {
            _context.Assistances.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}