using MaterialQ.Data;
using MaterialQ.Data.Repositories.Interfaces;
using MaterialQ.Models.DataModels;
using MaterialQ.Services.Interfaces;

namespace MaterialQ.Services.Implementations;

public class UnitService : IUnitService
{
    private readonly IGenericRepository<UnitModel> _unitRepo;
    private readonly ApplicationDbContext _context;


    public UnitService(IGenericRepository<UnitModel> unitRepo,ApplicationDbContext context)
    {
        _unitRepo = unitRepo;
        _context = context;
    }

    public async Task<IEnumerable<UnitModel>> GetAllAsync()
    {
        return await _unitRepo.GetAllAsync();
    }

    public async Task<UnitModel?> GetByIdAsync(int id)
    {
        return await _unitRepo.GetByIdAsync(id);
    }

    public async Task AddAsync(UnitModel unit)
    {
        await _unitRepo.AddAsync(unit);
        await _unitRepo.SaveAsync();
    }

    public async Task UpdateAsync(UnitModel unit)
    {
        await _unitRepo.UpdateAsync(unit);
        await _unitRepo.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var unit = await _unitRepo.GetByIdAsync(id);
        if (unit != null)
        {
            var item= await _context.Items.FindAsync(id);
            if (item == null) 
            {
                await _unitRepo.DeleteAsync(unit);
                await _unitRepo.SaveAsync();
            }
            else
            {
                throw new InvalidOperationException("Cannot delete this unit because it is linked to items.");
            }

        }
    }

 
}
