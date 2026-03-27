using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;

namespace MaterialQ.Services.Interfaces;

public interface IItemService
{
    Task<IEnumerable<RetrieveItemViewModel>> GetAllItemsIndexAsync();

    Task<IEnumerable<ItemsModel>> GetAllAsync();
    Task<ItemsModel?> GetByIdAsync(int id);
    Task AddAsync(ItemsModel item);
    Task UpdateAsync(ItemsModel item);
    Task DeleteAsync(int id);

    Task<ItemsModel?> GetItemforUpdateByIdAsync(int id);
}
