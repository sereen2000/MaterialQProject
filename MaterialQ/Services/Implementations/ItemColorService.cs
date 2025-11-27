using MaterialQ.Data;
using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace MaterialQ.Services.Implementations;

public class ItemColorService : IItemColorService
{
    private readonly ApplicationDbContext _context;

    public ItemColorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ColorItemModel>> GetColorsByItemIdAsync(int itemId)
    {
        return await _context.ColorItem
            .Include(ci => ci.Color)
            .Where(ci => ci.ItemId == itemId)
            .ToListAsync();
    }

    public async Task AddOrUpdateColorAsync(ColorItemModel model)
    {
        var existing = await _context.ColorItem
            .FirstOrDefaultAsync(ci => ci.ItemId == model.ItemId && ci.ColorId == model.ColorId);

        if (existing == null)
        {
            await _context.ColorItem.AddAsync(model);
        }
        else
        {
            existing.Quantity = model.Quantity;
            _context.ColorItem.Update(existing);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteColorAsync(int itemId, int colorId)
    {
        var entity = await _context.ColorItem
            .FirstOrDefaultAsync(ci => ci.ItemId == itemId && ci.ColorId == colorId);

        if (entity != null)
        {
            _context.ColorItem.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

