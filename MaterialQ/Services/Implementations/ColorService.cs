using MaterialQ.Data;
using MaterialQ.Data.Repositories;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaterialQ.Services.Implementations;

public class ColorService : IColorService

{
    private readonly IGenericRepository<ColorsModel> _colorRepository;
    private readonly ApplicationDbContext _context;
    private readonly IGenericRepository<ColorItemModel> _itemColorRepository;

    public ColorService(IGenericRepository<ColorsModel> colorRepo,ApplicationDbContext context, IGenericRepository<ColorItemModel> itemColorRepository)
    {
        _colorRepository = colorRepo;
        _context = context;
        _itemColorRepository = itemColorRepository;
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

        if (entity == null)
            throw new Exception("Color not found.");

        bool hasLinkedItems = await _context.ColorItem.AnyAsync(x => x.ColorId == id);

        if (hasLinkedItems)
            throw new InvalidOperationException("Cannot delete this color because it is linked to items.");

        await _colorRepository.DeleteAsync(entity);
        await _colorRepository.SaveAsync();
    }

    public async ValueTask<List<ItemColor>> GetColorsrelatedtToItem(int itemId)
    {
        var items = (await _itemColorRepository.GetAllAsync())
                .Where(i => i.Id == itemId)
                .ToList();

        var itemColors = await Task.WhenAll(items.Select(async i => new ItemColor
        {
            ColorId = i.ColorId,
            ColorName = (await _colorRepository.GetByIdAsync(i.ColorId)).Name,
            Quantity = i.Quantity
        }));

        return itemColors.ToList();
    }
}
