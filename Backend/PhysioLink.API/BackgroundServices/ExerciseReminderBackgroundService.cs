using PhysioLink.Application.Interfaces;

namespace PhysioLink.API.BackgroundServices
{
    // Wakes once a minute and runs one ReminderDispatcher pass. All the scheduling
    // rules (due, stale, once-per-day, multi-instance claim) live in the dispatcher;
    // this class is only the timer. Singleton, so each tick gets its own DI scope
    // for the scoped DbContext.
    public class ExerciseReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExerciseReminderBackgroundService> _logger;

        public ExerciseReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExerciseReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            try
            {
                do
                {
                    await RunTickAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host shutting down.
            }
        }

        private async Task RunTickAsync(CancellationToken stoppingToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IReminderDispatcher>();
                await dispatcher.RunOnceAsync(DateTime.UtcNow, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
            {
                // A bad tick (DB blip, ...) must never kill the loop; the next tick
                // picks up whatever was missed thanks to the "<= now" rule.
                _logger.LogError(ex, "Exercise reminder tick failed");
            }
        }
    }
}
