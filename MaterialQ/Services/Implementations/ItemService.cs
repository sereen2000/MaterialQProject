using MaterialQ.Data;
using MaterialQ.Data.Repositories.Implementations;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Imaging;
using System.Text.Json;

namespace MaterialQ.Services.Implementations;

public class ItemService : IItemService
{
    private readonly IGenericRepository<ItemsModel> _itemRepo;
    private readonly ApplicationDbContext _context;

    public ItemService(IGenericRepository<ItemsModel> itemRepo, ApplicationDbContext context)
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

    public async Task AddItemWithColorsAsync(IFormCollection form)
    {
        var colorsList = JsonSerializer.Deserialize<List<ItemColor>>(form["ColorsJson"]);

        foreach (var c in colorsList)
        {
            int colorId = c.ColorId ?? 0;
            if (colorId == 0 && !string.IsNullOrEmpty(c.ColorName))
            {
                var newColor = new ColorsModel
                {
                    Name = c.ColorName
                };

                await _context.Colors.AddAsync(newColor);
                await _context.SaveChangesAsync();

                colorId = newColor.Id;
            }

            var item = new ItemsModel
            {
                Code = form["ItemCode"],
                Description = form["Description"],
                Price = float.Parse(form["Price"]),
                UnitId = int.Parse(form["Unit"]),
                Vat = float.Parse(form["VAT"]),
                ColorId = colorId,
                Qty = c.Quantity
            };

            var imageFile = form.Files["ImageFile"];
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var ext = Path.GetExtension(imageFile.FileName);
                fileName = $"{fileName}_{Guid.NewGuid()}{ext}";

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/items");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                item.Image = "/images/items/" + fileName;
            }
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }
    }
}
