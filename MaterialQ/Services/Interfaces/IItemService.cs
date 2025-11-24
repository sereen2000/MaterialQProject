using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;

namespace MaterialQ.Services.Interfaces;

public interface IItemService
{
    Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync();

    // ------------ Basic CRUD on Items ------------
    Task<IEnumerable<ItemsModel>> GetAllAsync();
    Task<ItemsModel?> GetByIdAsync(int id);
    Task AddAsync(ItemsModel item);
    Task UpdateAsync(ItemsModel item);
    Task DeleteAsync(int id);

    // ------------ Get item for update including ColorItems ------------
    Task<ItemsModel?> GetItemforUpdateByIdAsync(int id);
    Task<List<ColorItemModel>> GetColorforUpdateByIdAsync(int id);

    // ------------ Add/Update item with colors (from form or ViewModel) ------------
    Task AddItemWithColorsAsync(IFormCollection form);  // إضافة عنصر مع ألوانه
    Task UpdateItemWithColorsAsync(UpdateItemViewModel model); // تعديل عنصر وألوانه
}
