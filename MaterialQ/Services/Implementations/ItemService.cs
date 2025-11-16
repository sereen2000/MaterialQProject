using MaterialQ.Data;
using MaterialQ.Data.Repositories.Implementations;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
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


    public async Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync()
    {
        var items = await _context.Items
        .AsNoTracking()
        .Include(i => i.Color)
        .Include(i => i.Unit)
        .ToListAsync();

        var result = items
       .GroupBy(i => i.Code)
       .Select(g => new RetrieveItemViewModel
       {
           Id = g.First().Id,
           Code = g.Key,
           Description = g.First().Description,
           Price = g.First().Price,
           Vat = g.First().Vat,
           UnitName = g.First().Unit.Name,

           // 👇 Total quantity for all colors of this item
           TotalQty = g.Sum(i => i.Qty),

           // 👇 List of colors with individual quantities
           Colors = g.Select(i => new ItemColor
           {
               ColorId = i.ColorId,
               ColorName = i.Color?.Name,
               Quantity = i.Qty
           }).ToList()
       })
       .ToList();

        return result;
    }


    public async Task<ItemsModel?> GetByIdAsync(int id)
    {
        return await _itemRepo.GetByIdAsync(id);
    }

    public async Task<ItemsModel?> GetItemforUpdateByIdAsync(int id)
    {
        var itemMain = await _context.Items
            .Include(i => i.Unit)
            .Include(i => i.Color)
            .FirstOrDefaultAsync(i => i.Id == id);

         return itemMain;

    }

    public async Task<List<ItemsModel?>> GetColorforUpdateByIdAsync(int id)
    {
        var Colors = await _context.Items
            .Where(i => i.ColorId == id)
            .Include(i => i.Color)
            .ToListAsync();
        return Colors;
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

        // Upload image once
        string imagePath = null;
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
            imagePath = "/images/items/" + fileName;
        }

        // Create items for each color
        foreach (var c in colorsList)
        {
            int colorId = c.ColorId ?? 0;
            if (colorId == 0 && !string.IsNullOrEmpty(c.ColorName))
            {
                var newColor = new ColorsModel { Name = c.ColorName };
                await _context.Colors.AddAsync(newColor);
                await _context.SaveChangesAsync(); // get new ID
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
                Qty = c.Quantity,
                Image = imagePath
            };

            await _context.Items.AddAsync(item);
        }

        await _context.SaveChangesAsync(); // save all at once
    }

    public async Task UpdateItemWithColorsAsync(UpdateItemViewModel model)
    {
        var itemFromDb = await _context.Items.FirstOrDefaultAsync(i => i.Id == model.Id);
        if (itemFromDb == null) return;

        // 1. Handle image
        string imagePath = itemFromDb.Image;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(model.ImageFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/items");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }
            imagePath = "/images/items/" + fileName;
        }

        // 2. Delete old color variants for this item code
        var oldItems = _context.Items.Where(i => i.Code == itemFromDb.Code).ToList();
        _context.Items.RemoveRange(oldItems);
        await _context.SaveChangesAsync();

        // 3. Recreate items for each color
        foreach (var color in model.Colors)
        {
            int colorId = color.ColorId ?? 0;

            // Add new color if needed
            if (colorId == 0 && !string.IsNullOrEmpty(color.ColorName))
            {
                var newColor = new ColorsModel { Name = color.ColorName };
                await _context.Colors.AddAsync(newColor);
                await _context.SaveChangesAsync();
                colorId = newColor.Id;
            }

            var newItem = new ItemsModel
            {
                Code = model.Code,
                Description = model.Description,
                Price = (float)model.Price,
                Vat = model.Vat,
                UnitId = model.UnitId,
                ColorId = colorId,
                Qty = color.Quantity,
                Image = imagePath
            };

            await _context.Items.AddAsync(newItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ItemsModel>> GetAllAsync()
    {
        var items = await _context.Items
                 .AsNoTracking()
                 .Include(i => i.Color)
                 .Include(i => i.Unit)
                 .ToListAsync();
        return items;
    }
}
