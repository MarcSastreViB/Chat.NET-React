using Chat.Domain.model;
using Chat.Domain.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Infrastructure.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private readonly ConcurrentDictionary<Guid, ChatRoom> _store = new();
        public InMemoryChatRepository() { }
        public Task AddMensajeAsync(Guid chatId, Mensaje mensaje)
        {
            throw new NotImplementedException();
        }

        public Task AddUsuarioAsync(Guid chatId, Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public Task<ChatRoom> CreateChatAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChatRoom?> GetChatAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Mensaje>> GetMensajesAsync(Guid chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Usuario>> GetUsuariosAsync(Guid chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatRoom>> ListChatsAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
