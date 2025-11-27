using MaterialQ.Data.Repositories;
using MaterialQ.Models.DataModels;

public class CheckStockService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public CheckStockService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoLowStockCheck();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CheckStockService: " + ex.Message);
            }

            // ALWAYS valid: 30 seconds
            await Task.Delay(30000, stoppingToken);
        }
    }

    private async Task DoLowStockCheck()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var itemRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<ItemsModel>>();
            var items = await itemRepo.GetAllAsync();

            foreach (var item in items)
            {
                if (item.Qty <= 5)
                {
                    Console.WriteLine($"⚠ LOW STOCK: {item.Code} - Qty={item.Qty}");
                }
            }
        }
    }
}
