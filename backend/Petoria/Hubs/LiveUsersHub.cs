using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Petoria.Hubs
{
    /// <summary>
    /// SignalR хъб за брояч на онлайн потребители в реално време.
    /// Увеличава/намалява брояча при свързване/разкачване и уведомява всички клиенти.
    /// </summary>
    public class LiveUsersHub : Hub
    {
        /// <summary>Атомарен брояч на свързани потребители</summary>
        private static int _userCount = 0;

        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref _userCount);
            await Clients.All.SendAsync("UpdateUserCount", _userCount);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Interlocked.Decrement(ref _userCount);
            await Clients.All.SendAsync("UpdateUserCount", _userCount);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
