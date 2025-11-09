using MaterialQ.Data.Repositories.Implementations;
using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;

namespace MaterialQ.Services.Implementations;

public class ColorService : IColorService
{
    private readonly IGenericRepository<ColorsModel> _colorRepository;

    public ColorService(IGenericRepository<ColorsModel> colorRepo)
    {
        _colorRepository = colorRepo;
    }

    public async Task<IEnumerable<ColorsModel>> GetAllAsync() => await _colorRepository.GetAllAsync();

    public async Task<ColorsModel> GetByIdAsync(int id)
    {
        if (id != 0)
            return null;
        var Color = await _colorRepository.GetByIdAsync(id);
        return Color;
    } 

    public async Task AddAsync(ColorsModel color) => await _colorRepository.AddAsync(color);

    public async Task UpdateAsync(ColorsModel color) => await _colorRepository.UpdateAsync(color);

    public async Task DeleteAsync(int id)
    {
        var entity = await _colorRepository.GetByIdAsync(id);
        if (entity != null)
            await _colorRepository.DeleteAsync(entity);
    }

}
