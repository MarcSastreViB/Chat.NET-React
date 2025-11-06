using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
namespace Chat.Infrastructure.Services
{
    internal class ChatService : IChatService
    {
        public Task AddUserAsync(Guid chatId, UsuarioDto usuario)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateChatRoomAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChatDto> GetChatRoomAsync(Guid chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Guid>> ListChatRoomsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MensajeDto> SendMessageAsync(Guid chatId, MensajeDto mensaje)
        {
            throw new NotImplementedException();
        }
    }
}
