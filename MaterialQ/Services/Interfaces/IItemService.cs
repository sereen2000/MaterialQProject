using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;

namespace MaterialQ.Services.Interfaces;

public interface IItemService
{
    Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync();
    Task<IEnumerable<ItemsModel>> GetAllAsync();
    Task<ItemsModel?> GetByIdAsync(int id);
    Task<ItemsModel?> GetItemforUpdateByIdAsync(int id);
    Task<List<ItemsModel?>> GetColorforUpdateByIdAsync(int id);
    Task AddItemWithColorsAsync(IFormCollection form);
    Task UpdateItemWithColorsAsync(UpdateItemViewModel model);
    Task AddAsync(ItemsModel item);
    Task AddItemsAsync(List<ItemsModel> items);
    Task UpdateAsync(ItemsModel item);
    Task DeleteAsync(int id);
}
