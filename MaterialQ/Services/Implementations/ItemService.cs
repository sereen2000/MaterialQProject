using MaterialQ.Data;
using MaterialQ.Data.Repositories;
using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;
using MaterialQ.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    // ----------------- Get items for Index -----------------
    public async Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync()
    {
        var items = await _context.Items
            .Include(i => i.Unit)
            .AsNoTracking()
            .ToListAsync();

        return items.Select(i => new RetrieveItemViewModel
        {
            Id = i.Id,
            ImageFile = i.Image,
            Code = i.Code,
            Description = i.Description,
            Price = i.Price,
            ActualPrice = i.ActualPrice,
            Vat = i.Vat,
            Qty = i.Qty,                 // 👈 الكمية مباشرة من Items
            UnitId = i.UnitId,
            UnitName = i.Unit?.Name
        });
    }

    // ----------------- Get single item -----------------
    public async Task<ItemsModel?> GetByIdAsync(int id)
    {
        return await _itemRepo.GetByIdAsync(id);
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

    // ----------------- Delete item -----------------
    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);

        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // ----------------- Add item from form (مبسّط) -----------------
    public async Task AddItemWithColorsAsync(IFormCollection form)
    {
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

        // Unit logic
        int unitId = int.TryParse(form["UnitId"], out var u) ? u : 0;

        if (unitId == 0 && !string.IsNullOrWhiteSpace(form["NewUnitName"]))
        {
            var newUnit = new UnitModel { Name = form["NewUnitName"] };
            await _context.Units.AddAsync(newUnit);
            await _context.SaveChangesAsync();
            unitId = newUnit.Id;
        }

        // Create item (بدون ألوان)
        var item = new ItemsModel
        {
            Code = form["Code"],
            Description = form["Description"],
            Price = float.TryParse(form["Price"], out var p) ? p : 0,
            ActualPrice = float.TryParse(form["ActualPrice"], out var ap) ? ap : 0,
            Vat = float.TryParse(form["Vat"], out var v) ? v : 0,
            Qty = float.TryParse(form["Qty"], out var q) ? q : 0,   // 👈 الكمية من الفورم
            UnitId = unitId,
            Image = imagePath
        };

        await _itemRepo.AddAsync(item);
        await _itemRepo.SaveAsync();
    }

    // ----------------- Update item (بدون ألوان) -----------------
    public async Task UpdateItemWithColorsAsync(UpdateItemViewModel model)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == model.Id);
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

        // Update basic fields
        item.Code = model.Code;
        item.Description = model.Description;
        item.Price = model.Price;
        item.ActualPrice = model.ActualPrice;
        item.Vat = model.Vat;
        item.Qty = model.Qty;          // 👈 الكمية مباشرة
        item.UnitId = model.UnitId;

        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    // ----------------- Get all items raw -----------------
    public async Task<IEnumerable<ItemsModel>> GetAllAsync()
    {
        return await _context.Items
            .AsNoTracking()
            .Include(i => i.Unit)
            .ToListAsync();
    }

    public async Task<ItemsModel?> GetItemforUpdateByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Unit)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}
