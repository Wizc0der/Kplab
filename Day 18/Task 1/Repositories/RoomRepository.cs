using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task_1.Core;
using Task_1.Models;
using Task_1.Services;

namespace Task_1.Repositories
{
    public class RoomRepository : IRepository<Room>
    {
        private readonly string _filePath;
        private List<Room> _cache;

        public RoomRepository(string filePath) => _filePath = filePath;

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            await LoadAsync();
            return _cache;
        }

        public Task<Room> GetByIdAsync(int id) =>
            Task.FromResult(_cache?.FirstOrDefault(r => r.Id == id));

        public async Task AddAsync(Room entity)
        {
            await LoadAsync();
            entity.Id = _cache.Max(r => r.Id) + 1;
            _cache.Add(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(Room entity)
        {
            await LoadAsync();
            var existing = _cache.FirstOrDefault(r => r.Id == entity.Id);
            if (existing != null)
            {
                existing.Number = entity.Number;
                existing.PricePerNight = entity.PricePerNight;
                existing.Status = entity.Status;
                await SaveAsync();
            }
        }

        public async Task DeleteAsync(Room entity)
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
            _cache = JsonService.Load<List<Room>>(_filePath) ?? new List<Room>();
            return Task.CompletedTask;
        }
    }
}