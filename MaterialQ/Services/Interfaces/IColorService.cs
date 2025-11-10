using MaterialQ.Models.DataModels;

namespace MaterialQ.Services.Interfaces;

public interface IColorService
{
    Task<IEnumerable<ColorsModel>> GetAllAsync();
    Task<ColorsModel> GetByIdAsync(int id);
    Task AddAsync(ColorsModel color);
    Task UpdateAsync(ColorsModel color);
    Task DeleteAsync(int id);
}
