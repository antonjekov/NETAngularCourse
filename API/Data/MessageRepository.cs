using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            this.context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await this.context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            var group = await this.context.Groups
                .Include(g => g.Connections)
                .Where(g=>g.Connections.Any(c=>c.ConnectionId==connectionId))
                .FirstOrDefaultAsync();
            return group;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages
                .Include(u=>u.Sender)
                .Include(u=>u.Recipient)
                .SingleOrDefaultAsync(m=>m.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this.context.Groups
                .Include(x=>x.Connections)
                .FirstOrDefaultAsync(x=>x.Name==groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages
                .OrderByDescending(x => x.MessageSent)
                .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
            "Inbox" => query
                .Where(x => x.RecipientUsername == messageParams.Username && x.RecipientDeleted==false),

            "Outbox" => query
                .Where(x=>x.SenderUsername == messageParams.Username && x.SenderDeleted==false),

            _ => query
                .Where(x => x.RecipientUsername == messageParams.Username && x.RecipientDeleted == false && x.DateRead==null)
            };

            return await PagedList<MessageDto>.CreateAsync(query,messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await this.context.Messages
                 .Where(x => 
                 x.Recipient.UserName == currentUsername && x.Sender.UserName== recipientUsername && x.RecipientDeleted == false ||
                 x.Recipient.UserName == recipientUsername&& x.Sender.UserName == currentUsername && x.SenderDeleted == false)
                 .OrderBy(x=>x.MessageSent)
                 .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                 .ToListAsync();

            var unreadMessages = messages
                .Where(m=>m.DateRead==null && m.RecipientUsername==currentUsername)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return messages;

        }

        public void RemoveConnection(Connection connection)
        {
            this.context.Connections.Remove(connection);
        }

    }
}
