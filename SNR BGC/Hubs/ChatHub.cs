using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SNR_BGC.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
       public async Task SendOrders(string name,int? count)
        {
            await Clients.All.RecieveOrders(name, count);
        }

    }
}
