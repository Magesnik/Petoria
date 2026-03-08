using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Petoria.Hubs
{
    public class LiveUsersHub : Hub
    {
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
