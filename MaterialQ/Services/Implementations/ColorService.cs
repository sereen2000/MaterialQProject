using MaterialQ.Data;
using MaterialQ.Data.Repositories.Implementations;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;

namespace MaterialQ.Services.Implementations;

public class ColorService : IColorService

{
    private readonly IGenericRepository<ColorsModel> _colorRepository;
    private readonly ApplicationDbContext _context;
    private readonly IGenericRepository<ItemsModel> _itemRepository;

    public ColorService(IGenericRepository<ColorsModel> colorRepo,ApplicationDbContext context, IGenericRepository<ItemsModel> itemRepository)
    {
        _colorRepository = colorRepo;
        _context = context;
        _itemRepository = itemRepository;
    }

    public async Task<IEnumerable<ColorsModel>> GetAllAsync() => await _colorRepository.GetAllAsync();

    public async Task<ColorsModel> GetByIdAsync(int id)
    {
        if (id == 0)
            return null;
        var Color = await _colorRepository.GetByIdAsync(id);
        return Color;
    }

    public async Task AddAsync(ColorsModel color) 
    {
        await _colorRepository.AddAsync(color);
        await _colorRepository.SaveAsync();

    }

    public async Task UpdateAsync(ColorsModel color) 
    {
        await _colorRepository.UpdateAsync(color);
        await _colorRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _colorRepository.GetByIdAsync(id);
        if (entity != null)
            await _colorRepository.DeleteAsync(entity);
            await _colorRepository.SaveAsync();
    }

    public async ValueTask<List<ItemColor>> GetColorsrelatedtToItem(int itemId)
    {
        var items = (await _itemRepository.GetAllAsync())
                .Where(i => i.Id == itemId)
                .ToList();

        var itemColors = await Task.WhenAll(items.Select(async i => new ItemColor
        {
            ColorId = i.ColorId,
            ColorName = (await _colorRepository.GetByIdAsync(i.ColorId)).Name,
            Quantity = i.Qty
        }));

        return itemColors.ToList();
    }
}
