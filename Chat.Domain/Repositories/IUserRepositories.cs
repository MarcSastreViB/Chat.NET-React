using Chat.Domain.model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chat.Domain.Repositories
{
    public interface IUserRepositories
    {
        Task<Usuario> CreateUserAsync(Usuario usuario);
        Task<Usuario?> GetUserAsync(string userName);
        Task<IEnumerable<Usuario>> ListUsuariosAsync();
        Task<byte[]> GetUserPhoto(string userName);
        Task<bool> UserExistsAsync(string userName, CancellationToken ct = default);
        Task<bool> DeleteUserAsync(string userName);
        Task<ChatRoom> GetUserChats(string userName);
        Task SaveChangesAsync();
    }
}
