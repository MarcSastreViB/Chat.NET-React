using Chat.Domain.model;
using Chat.Domain.Repositories;
using System.Collections.Concurrent;

namespace Chat.Infrastructure.Repositories
{
    public class InMemoryChatRepository : IChatRepository
    {
        private readonly ConcurrentDictionary<Guid, ChatRoom> _store = new();
        private readonly InMemoryUserRepository _userRepo;

        public InMemoryChatRepository(InMemoryUserRepository userRepo)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task AddMensajeAsync(Guid chatId, Mensaje mensaje)
        {
            if (mensaje == null) throw new ArgumentNullException(nameof(mensaje));

            if (!_store.TryGetValue(chatId, out ChatRoom chatRoom))
                throw new KeyNotFoundException($"Chat {chatId} no encontrado");

            // Verificar que el usuario del mensaje existe globalmente
            if (mensaje.Usuario == null || !await _userRepo.UserExistsAsync(mensaje.Usuario.userName))
                throw new InvalidOperationException($"Usuario '{mensaje.Usuario?.userName}' no existe en el repositorio de usuarios.");

            if (mensaje.Id == Guid.Empty)
                mensaje.Id = Guid.NewGuid();

            lock (chatRoom)
            {
                if (!chatRoom.AddMessage(mensaje))
                    throw new InvalidOperationException("No se puede añadir el mensaje, el usuario no pertenece a la sala");
            }
        }

        public async Task AddUsuarioAsync(Guid chatId, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("userName vacío", nameof(userName));

            // Obtener usuario del repositorio global
            var usuario = await _userRepo.GetUserAsync(userName);
            if (usuario == null) throw new KeyNotFoundException($"Usuario '{userName}' no existe en el repositorio de usuarios.");

            if (!_store.TryGetValue(chatId, out var chat))
                throw new KeyNotFoundException($"Chat {chatId} no encontrado.");

            lock (chat)
            {
                chat.AddUser(usuario); // AddUser internamente evita duplicados
            }
        }

        public Task<ChatRoom> CreateChatAsync()
        {
            var chat = new ChatRoom
            {
                Id = Guid.NewGuid()
            };
            _store[chat.Id] = chat;
            return Task.FromResult(chat);
        }

        public Task<ChatRoom?> GetChatAsync(Guid id)
        {
            _store.TryGetValue(id, out var chat);
            return Task.FromResult(chat);
        }

        public Task<IEnumerable<Mensaje>> GetMensajesAsync(Guid chatId)
        {
            if (!_store.TryGetValue(chatId, out var chat))
                throw new KeyNotFoundException($"Chat {chatId} no encontrado");
            var mensajes = chat.Mensajes.ToList().AsEnumerable();
            return Task.FromResult(mensajes);
        }

        public Task<IEnumerable<Usuario>> GetUsuariosAsync(Guid chatId)
        {
            if (!_store.TryGetValue(chatId, out var chat))
                throw new KeyNotFoundException($"Chat {chatId} no encontrado");
            var usuarios = chat.Usuarios.ToList().AsEnumerable();
            return Task.FromResult(usuarios);
        }

        public Task<IEnumerable<ChatRoom>> ListChatsAsync()
        {
            var list = _store.Values.ToList().AsEnumerable();
            return Task.FromResult(list);
        }

        public Task SaveChangesAsync()
        {
            // In-memory: no-op. En una implementación real con EF aquí iría SaveChangesAsync().
            return Task.CompletedTask;
        }
    }
}
