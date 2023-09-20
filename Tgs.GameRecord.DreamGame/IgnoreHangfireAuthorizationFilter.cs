using Hangfire.Dashboard;

namespace Tgs.GameRecord.DreamGame;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
