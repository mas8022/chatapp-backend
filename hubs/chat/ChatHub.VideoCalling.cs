using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace backend.hubs.chat
{
    public partial class ChatHub: Hub
    {
        public async Task CallUserVideo(int targetUserId, object offer)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("IncomingVideoCall", new
            {
                callerId = CurrentUserId,
                offer
            });
        }

        public async Task AnswerVideoCall(int targetUserId, object answer)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("VideoCallAccepted", new
            {
                receiverId = CurrentUserId,
                answer
            });
        }

        public async Task HangUpVideo(int targetUserId)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("VideoCallEnded", new
            {
                userId = CurrentUserId
            });
        }

        public async Task SendVideoIceCandidate(int targetUserId, object candidate)
        {
            await Clients.User(targetUserId.ToString()).SendAsync("ReceiveVideoIceCandidate", new
            {
                CurrentUserId,
                candidate
            });
        }
    }
}
