using System;
using System.Threading.Tasks;

namespace TypedClientInterfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(string user, string message);
    }
}
