using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace backend.hubs.chat
{
    public partial class ChatHub
    {
        public async Task CallUser(int targetUserId, object offer)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("IncomingCall", new
            {
                callerId = CurrentUserId,
                offer
            });
        }

        public async Task AnswerCall(int targetUserId, object answer)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("CallAccepted", new
            {
                receiverId = CurrentUserId,
                answer
            });
        }

        public async Task HangUp(int targetUserId)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("CallEnded", new
            {
                userId = CurrentUserId
            });
        }

        public async Task SendIceCandidate(int targetUserId, object candidate)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("ReceiveIceCandidate", new
            {
                CurrentUserId,
                candidate
            });
        }
    }
}
