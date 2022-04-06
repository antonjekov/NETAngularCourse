using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHubContext;
        private readonly PresenceTracker presenceTracker;

        public MessageHub(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHubContext,
            PresenceTracker presenceTracker
            )
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.presenceHubContext = presenceHubContext;
            this.presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = getGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);


            var messages = await this.unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            if (this.unitOfWork.HasChanges()) await this.unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send messages to yourself");
            }
            var sender = await this.unitOfWork.UserRepository.GetUsersByUsernameAsync(username);
            var recipient = await this.unitOfWork.UserRepository.GetUsersByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) throw new HubException("Not found user");
            var message = new Message()
            {
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Sender = sender,
                SenderUsername = sender.UserName,
                Content = createMessageDto.Content
            };

            var groupName = getGroupName(username, recipient.UserName);

            var group = await this.unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await this.presenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections !=null)
                {
                    await presenceHubContext.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new {username=sender.UserName, knownAs=sender.KnownAs});
                }
            }

            this.unitOfWork.MessageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete())
            {
                var createdMessage = mapper.Map<MessageDto>(message);
                await Clients.Group(groupName).SendAsync("NewMessage", createdMessage);
            }
        }

        private string getGroupName(string caller, string other)
        {
            //this ensure alfabetical order of groups
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller} - {other}" : $"{other} - {caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await this.unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                this.unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            if( await unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await this.unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x=>x.ConnectionId== Context.ConnectionId);
            this.unitOfWork.MessageRepository.RemoveConnection(connection);
            if(await unitOfWork.Complete()) return group;
            throw new HubException("Failed to remove from group");
        }
    }
}
