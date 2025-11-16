using MaterialQ.Models.DataModels;
using MaterialQ.Models.ViewModels;

namespace MaterialQ.Services.Interfaces;

public interface IColorService
{
    Task<IEnumerable<ColorsModel>> GetAllAsync();
    ValueTask<List<ItemColor>> GetColorsrelatedtToItem(int itemId);
    Task<ColorsModel> GetByIdAsync(int id);
    Task AddAsync(ColorsModel color);
    Task UpdateAsync(ColorsModel color);
    Task DeleteAsync(int id);
}
