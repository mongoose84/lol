using Microsoft.EntityFrameworkCore;
using RiotProxy.Domain;

namespace RiotProxy.Infrastructure.Persistence
{
    public class PersonRepository
    {
        private readonly ApplicationDbContext _db;

        public PersonRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Person?> GetByIdAsync(int userId, CancellationToken ct = default)
        {
            return await _db.Set<Person>().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        }

        public async Task<Person?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userName)) return null;
            return await _db.Set<Person>().FirstOrDefaultAsync(p => p.UserName == userName, ct);
        }

        public async Task<List<Person>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Set<Person>().ToListAsync(ct);
        }

        public async Task AddAsync(Person person, CancellationToken ct = default)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));
            _db.Set<Person>().Add(person);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Person person, CancellationToken ct = default)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));
            _db.Set<Person>().Update(person);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int userId, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(userId, ct);
            if (entity == null) return;
            _db.Set<Person>().Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}