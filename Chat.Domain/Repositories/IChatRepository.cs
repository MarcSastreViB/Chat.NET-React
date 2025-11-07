using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Domain.model;

namespace Chat.Domain.Repositories
{
    /// <summary>
    /// Contrato para operaciones de persistencia relacionadas con Chat.
    /// Los métodos son asincrónicos y trabajan con entidades del dominio.
    /// </summary>

    public interface IChatRepository
    {
        /// <summary>Crea un nuevo chat y devuelve su referencia.</summary>
        Task<ChatRoom> CreateChatAsync();

        /// <summary>Devuelve el `Chat` por id o null si no existe.</summary>
        Task<ChatRoom?> GetChatAsync(Guid id);

        /// <summary>Lista todos los chats.</summary>
        Task<IEnumerable<ChatRoom>> ListChatsAsync();

        /// <summary>Devuelve los usuarios de un chat.</summary>
        Task<IEnumerable<Usuario>> GetUsuariosAsync(Guid chatId);

        /// <summary>Devuelve los mensajes de un chat.</summary>
        Task<IEnumerable<Mensaje>> GetMensajesAsync(Guid chatId);

        /// <summary>Añade un usuario al chat (no duplica si ya existe).</summary>
        Task AddUsuarioAsync(Guid chatId, string userName);

        /// <summary>Añade un mensaje al chat.</summary>
        Task AddMensajeAsync(Guid chatId, Mensaje mensaje);

        /// <summary>Persiste cambios (en EF ejecutaría SaveChangesAsync).</summary>
        Task SaveChangesAsync();
    }
}