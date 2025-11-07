using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chat.Domain.model;
using Chat.Domain.Repositories;

namespace Chat.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserRepositories
    {
        private readonly ConcurrentDictionary<string, Usuario> _users = new(StringComparer.OrdinalIgnoreCase);

        public Task<Usuario> CreateUserAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (string.IsNullOrWhiteSpace(usuario.userName)) throw new ArgumentException("UserName vacío", nameof(usuario.userName));

            _users.AddOrUpdate(usuario.userName, usuario, (k, existing) => usuario);
            return Task.FromResult(usuario);
        }

        public Task<bool> DeleteUserAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("UserName vacío", nameof(userName));
            var removed = _users.TryRemove(userName, out _);
            return Task.FromResult(removed);
        }

        public Task<Usuario?> GetUserAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("UserName vacío", nameof(userName));
            _users.TryGetValue(userName, out var user);
            return Task.FromResult(user);
        }

        public Task<ChatRoom> GetUserChats(string userName)
        {
            // Como los usuarios son independientes del chat, y no tenemos relación aquí,
            // devolvemos un ChatRoom vacío o lanzar NotSupported si no gestionamos relaciones.
            throw new NotSupportedException("GetUserChats no está soportado en el repositorio de usuarios en memoria.");
        }

        public Task<byte[]> GetUserPhoto(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("UserName vacío", nameof(userName));
            _users.TryGetValue(userName, out var user);
            return Task.FromResult(user?.fotoPerfil ?? Array.Empty<byte>());
        }

        public Task<IEnumerable<Usuario>> ListUsuariosAsync()
        {
            var list = _users.Values.ToList().AsEnumerable();
            return Task.FromResult(list);
        }

        public Task SaveChangesAsync()
        {
            // In-memory: no-op
            return Task.CompletedTask;
        }

        public Task<bool> UserExistsAsync(string userName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("UserName vacío", nameof(userName));
            var exists = _users.ContainsKey(userName);
            return Task.FromResult(exists);
        }
    }
}
