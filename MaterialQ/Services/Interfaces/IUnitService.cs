using MaterialQ.Models.DataModels;

namespace MaterialQ.Services.Interfaces;

public interface IUnitService
{
    Task<IEnumerable<UnitModel>> GetAllAsync();
    Task<UnitModel> GetByIdAsync(int id);
    Task AddAsync(UnitModel unit);
    Task UpdateAsync(UnitModel color);
    Task DeleteAsync(int id);
}
