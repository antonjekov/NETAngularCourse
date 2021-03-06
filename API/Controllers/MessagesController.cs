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
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower()) return this.BadRequest("You cannot send messages to yourself");
            var sender = await this.unitOfWork.UserRepository.GetUsersByUsernameAsync(username);
            var recipient = await this.unitOfWork.UserRepository.GetUsersByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) return this.NotFound();
            var message = new Message()
            {
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Sender = sender,
                SenderUsername = sender.UserName,
                Content=createMessageDto.Content
            };
            this.unitOfWork.MessageRepository.AddMessage(message);
            if (await this.unitOfWork.Complete()) return this.Ok(mapper.Map<MessageDto>(message));
            
            return this.BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            var username = User.GetUsername();
            messageParams.Username = username;
            var messages =  await this.unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }

        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage([FromRoute]int id)
        {
            var username = User.GetUsername();
            var message = await this.unitOfWork.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return this.Unauthorized();
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted) this.unitOfWork.MessageRepository.DeleteMessage(message);
            if(await this.unitOfWork.Complete()) return this.NoContent();
            return this.BadRequest("Problem deleting the message");
        }
    }
}
