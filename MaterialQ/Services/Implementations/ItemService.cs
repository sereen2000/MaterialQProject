using MaterialQ.Data;
using MaterialQ.Data.Repositories;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MaterialQ.Services.Implementations;

public class ItemService : IItemService
{
    private readonly IGenericRepository<ItemsModel> _itemRepo;
    private readonly IGenericRepository<ColorItemModel> _colorItemRepo;
    private readonly ApplicationDbContext _context;

    public ItemService(IGenericRepository<ItemsModel> itemRepo,IGenericRepository<ColorItemModel> colorItemRepo, ApplicationDbContext context)
    {
        _itemRepo = itemRepo;
        _colorItemRepo = colorItemRepo;
        _context = context;
    }

    public async Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync()
    {
        var items = await _context.Items
            .Include(i => i.Unit)
            .AsNoTracking()
            .ToListAsync();

        var colorItems = await _colorItemRepo.GetAllAsync();

        // Preload Colors into dictionary
        var colorsDict = await _context.Colors
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        // Attach Color Items to Items
        foreach (var item in items)
        {
            var colors = colorItems.Where(x => x.ItemId == item.Id);
            item.ColorItems.AddRange(colors);
        }

        // Map to ViewModel
        return items.Select(i => new RetrieveItemViewModel
        {
            Id = i.Id,
            ImageFile=i.Image,
            Code = i.Code,
            Description = i.Description,
            Price = i.Price,
            Vat = i.Vat,
            UnitName = i.Unit?.Name,
            TotalQty = i.ColorItems.Sum(ci => ci.Quantity),

            Colors = i.ColorItems.Select(ci => new ItemColor
            {
                ColorId = ci.ColorId,
                ColorName = colorsDict.ContainsKey(ci.ColorId) ? colorsDict[ci.ColorId] : "",
                Quantity = ci.Quantity
            }).ToList()
        });
    }


    // ----------------- Get single item -----------------
    public async Task<ItemsModel?> GetByIdAsync(int id)
    {
        return await _itemRepo.GetByIdAsync(id);
    }

    // ----------------- Get item with colors for edit -----------------
    public async Task<ItemsModel?> GetItemforUpdateByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Unit)
            .Include(i => i.ColorItems)
                .ThenInclude(ci => ci.Color)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    // ----------------- Get ColorItems for item -----------------
    public async Task<List<ColorItemModel>> GetColorforUpdateByIdAsync(int id)
    {
        return await _context.ColorItem
            .Where(ci => ci.ItemId == id)
            .Include(ci => ci.Color)
            .ToListAsync();
    }

    // ----------------- Add single item -----------------
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

    // ----------------- Update single item -----------------
    public async Task UpdateAsync(ItemsModel item)
    {
        await _itemRepo.UpdateAsync(item);
        await _itemRepo.SaveAsync();
    }

    // ----------------- Delete item + its ColorItems -----------------
    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items
            .Include(i => i.ColorItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item != null)
        {
            if (item.ColorItems.Any())
            {
                _context.ColorItem.RemoveRange(item.ColorItems);
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // ----------------- Add item with colors from form -----------------
    public async Task AddItemWithColorsAsync(IFormCollection form)
    {
        var colorsList = JsonSerializer.Deserialize<List<ItemColor>>(form["ColorsJson"]) ?? new List<ItemColor>();

        // Upload image
        string? imagePath = null;
        var imageFile = form.Files["ImageFile"];
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            imagePath = "/images/items/" + fileName;
        }

        // UnitId logic
        int unitId = int.TryParse(form["Unit"], out var u) ? u : 0;

        if (unitId == 0 && !string.IsNullOrWhiteSpace(form["NewUnitName"]))
        {
            var newUnit = new UnitModel { Name = form["NewUnitName"] };
            await _context.Units.AddAsync(newUnit);
            await _context.SaveChangesAsync();
            unitId = newUnit.Id;
        }

        // Create item
        var item = new ItemsModel
        {
            Code = form["ItemCode"],
            Description = form["Description"],
            Price = float.TryParse(form["Price"], out var p) ? p : 0,
            Vat = float.TryParse(form["VAT"], out var v) ? v : 0,
            UnitId = unitId,
            Image = imagePath
        };

        await _itemRepo.AddAsync(item);
        await _itemRepo.SaveAsync();

        // Add ColorItems
        foreach (var c in colorsList)
        {
            int colorId = c.ColorId ?? 0;
            if (colorId == 0 && !string.IsNullOrWhiteSpace(c.ColorName))
            {
                var newColor = new ColorsModel { Name = c.ColorName };
                await _context.Colors.AddAsync(newColor);
                await _context.SaveChangesAsync();
                colorId = newColor.Id;
            }

            var ci = new ColorItemModel
            {
                ItemId = item.Id,
                ColorId = colorId,
                Quantity = c.Quantity
            };
            await _context.ColorItem.AddAsync(ci);
        }

        // Update item quantity
        item.Qty = await _context.ColorItem
            .Where(ci => ci.ItemId == item.Id)
            .SumAsync(ci => ci.Quantity);

        await _itemRepo.UpdateAsync(item);
        await _itemRepo.SaveAsync();
    }

    // ----------------- Update item with colors -----------------
    public async Task UpdateItemWithColorsAsync(UpdateItemViewModel model)
    {
        var item = await _context.Items
            .Include(i => i.ColorItems)
            .FirstOrDefaultAsync(i => i.Id == model.Id);

        if (item == null) return;

        // Handle image
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ImageFile.CopyToAsync(stream);
            item.Image = "/images/items/" + fileName;
        }

        // Update basic item fields
        item.Code = model.Code;
        item.Description = model.Description;
        item.Price = (float)model.Price;
        item.Vat = model.Vat;
        item.UnitId = model.UnitId;

        item.ColorItems ??= new List<ColorItemModel>();

        // Update or add ColorItems
        foreach (var colorVm in model.Colors)
        {
            int colorId = colorVm.ColorId ?? 0;
            if (colorId == 0 && !string.IsNullOrWhiteSpace(colorVm.ColorName))
            {
                var newColor = new ColorsModel { Name = colorVm.ColorName };
                await _context.Colors.AddAsync(newColor);
                await _context.SaveChangesAsync();
                colorId = newColor.Id;
            }

            var existingCi = _context.ColorItem.Where(x=>x.ColorId==colorVm.ColorId&&x.ItemId==model.Id).FirstOrDefault();
            if (existingCi != null)
            {
                existingCi.Quantity = colorVm.Quantity;
                _context.ColorItem.Update(existingCi);
            }
            else
            {
                var newCi = new ColorItemModel
                {
                    ItemId = item.Id,
                    ColorId = colorId,
                    Quantity = colorVm.Quantity
                };
                await _context.ColorItem.AddAsync(newCi);
                item.ColorItems.Add(newCi);
            }
        }

        // Recalculate total quantity
        item.Qty = await _context.ColorItem
            .Where(ci => ci.ItemId == item.Id)
            .SumAsync(ci => ci.Quantity);

        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    // ----------------- Get all items raw -----------------
    public async Task<IEnumerable<ItemsModel>> GetAllAsync()
    {
        var items = await _context.Items
            .AsNoTracking()
            .Include(i => i.Unit)
            .Include(i => i.ColorItems)
                .ThenInclude(ci => ci.Color)
            .ToListAsync();

        return items;
    }
}
