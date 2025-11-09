using MaterialQ.Data;
using MaterialQ.Data.Repositories.Implementations;
using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Services.Implementations;

public class ItemService: IItemService
{
    private readonly IGenericRepository<ItemsModel> _itemRepo;
    private readonly ApplicationDbContext _context;

    public ItemService(IGenericRepository<ItemsModel> itemRepo,ApplicationDbContext context)
    {
        _itemRepo = itemRepo;
        _context = context;
    }

    public async Task<IEnumerable<ItemsModel>> GetAllAsync()
    {
        return await _context.Items
            .AsNoTracking()
            .Include(i => i.Color)
            .Include(i => i.Unit)
            .ToListAsync();
    }

    public async Task<ItemsModel?> GetByIdAsync(int id)
    {
        return await _itemRepo.GetByIdAsync(id);
    }
    public async Task AddAsync(ItemsModel item)
    {
        await _itemRepo.AddAsync(item);
        await _itemRepo.SaveAsync();
    }

    public async Task AddItemsAsync(List<ItemsModel> items)
    {
        await _itemRepo.AddRangeAsync(items);
        await _itemRepo.SaveAsync();
    }

    public async Task UpdateAsync(ItemsModel item)
    {
        await _itemRepo.UpdateAsync(item);
        await _itemRepo.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _itemRepo.GetByIdAsync(id);
        if (item != null)
        {
            await _itemRepo.DeleteAsync(item);
            await _itemRepo.SaveAsync();
        }
    }

}
