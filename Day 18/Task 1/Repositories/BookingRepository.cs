using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task_1.Core;
using Task_1.Models;
using Task_1.Services;

namespace Task_1.Repositories
{
    public class BookingRepository : IRepository<Booking>
    {
        private readonly string _filePath;
        private List<Booking> _cache;

        public BookingRepository(string filePath) => _filePath = filePath;

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            await LoadAsync();
            return _cache;
        }

        public Task<Booking> GetByIdAsync(int id) =>
            Task.FromResult(_cache?.FirstOrDefault(b => b.Id == id));

        public async Task AddAsync(Booking entity)
        {
            await LoadAsync();
            entity.Id = _cache.Max(b => b.Id) + 1;
            _cache.Add(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(Booking entity)
        {
            await LoadAsync();
            var existing = _cache.FirstOrDefault(b => b.Id == entity.Id);
            if (existing != null)
            {
                existing.GuestName = entity.GuestName;
                existing.CheckIn = entity.CheckIn;
                existing.CheckOut = entity.CheckOut;
                existing.Status = entity.Status;
                await SaveAsync();
            }
        }

        public async Task DeleteAsync(Booking entity)
        {
            await LoadAsync();
            _cache.Remove(entity);
            await SaveAsync();
        }

        public Task SaveAsync()
        {
            JsonService.Save(_filePath, _cache);
            return Task.CompletedTask;
        }

        public Task LoadAsync()
        {
            _cache = JsonService.Load<List<Booking>>(_filePath) ?? new List<Booking>();
            return Task.CompletedTask;
        }
    }
}