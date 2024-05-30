using System.Threading.Tasks;

namespace SNR_BGC.Hubs
{
    public interface IChatHub
    {
        Task RecieveOrders(string name,int? count);

    }
}
