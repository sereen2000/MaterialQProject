using MaterialQ.Models.DataModels;

namespace MaterialQ.Services.Interfaces;

public interface IItemColorService
{
    Task<List<ColorItemModel>> GetColorsByItemIdAsync(int itemId);
    Task AddOrUpdateColorAsync(ColorItemModel model);
    Task DeleteColorAsync(int itemId, int colorId);
}
