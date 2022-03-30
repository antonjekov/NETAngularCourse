using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController: BaseAPIController
    {
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper)
        {
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower()) return this.BadRequest("You cannot send messages to yourself");
            var sender = await this.userRepository.GetUsersByUsernameAsync(username);
            var recipient = await this.userRepository.GetUsersByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) return this.NotFound();
            var message = new Message()
            {
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Sender = sender,
                SenderUsername = sender.UserName,
                Content=createMessageDto.Content
            };
            this.messageRepository.AddMessage(message);
            if (await this.messageRepository.SaveAllAsync()) return this.Ok(mapper.Map<MessageDto>(message));
            
            return this.BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            var username = User.GetUsername();
            messageParams.Username = username;
            var messages =  await this.messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task< ActionResult<IEnumerable<MessageDto>> > GetMessageThread(string username)
        {
            var currentUser = User.GetUsername();
            var messages = await this.messageRepository.GetMessageThread(currentUser, username);
            return this.Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage([FromRoute]int id)
        {
            var username = User.GetUsername();
            var message = await this.messageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return this.Unauthorized();
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted) this.messageRepository.DeleteMessage(message);
            if(await this.messageRepository.SaveAllAsync()) return this.NoContent();
            return this.BadRequest("Problem deleting the message");
        }
    }
}
