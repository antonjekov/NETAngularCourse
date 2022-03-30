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
        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages
                .Include(u=>u.Sender)
                .Include(u=>u.Recipient)
                .SingleOrDefaultAsync(m=>m.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
            "Inbox" => query
                .Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted==false),

            "Outbox" => query
                .Where(x=>x.Sender.UserName == messageParams.Username && x.SenderDeleted==false),

            _ => query
                .Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false && x.DateRead==null)
            };

            var messages = query.ProjectTo<MessageDto>(this.mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages,messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await this.context.Messages
                 .Include(x=>x.Sender).ThenInclude(p=>p.Photos)
                 .Include(x=>x.Recipient).ThenInclude(p=>p.Photos)
                 .Where(x => 
                 x.Recipient.UserName == currentUsername && x.Sender.UserName== recipientUsername && x.RecipientDeleted == false ||
                 x.Recipient.UserName == recipientUsername&& x.Sender.UserName == currentUsername && x.SenderDeleted == false)
                 .OrderBy(x=>x.MessageSent)
                 .ToListAsync();

            var unreadMessages = messages
                .Where(m=>m.DateRead==null && m.Recipient.UserName==currentUsername)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }
                await this.context.SaveChangesAsync();
            }

            return this.mapper.Map<IEnumerable<MessageDto>>(messages);

        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync()> 0;
        }
    }
}
