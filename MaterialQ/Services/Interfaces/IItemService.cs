using MaterialQ.Models.DataModels;

namespace MaterialQ.Services.Interfaces;

public interface IItemService
{
    Task<IEnumerable<ItemsModel>> GetAllAsync();
    Task<ItemsModel?> GetByIdAsync(int id);
    Task AddAsync(ItemsModel item);
    Task AddItemsAsync(List<ItemsModel> items);
    Task UpdateAsync(ItemsModel item);
    Task DeleteAsync(int id);
}
