using Chat.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chat.Application.Interfaces
{
    public interface IChatService
    {
        // Crea una sala y devuelve su id
        Task<Guid> CreateChatRoomAsync();

        // Lista ids de salas
        Task<IEnumerable<Guid>> ListChatRoomsAsync();

        // Devuelve los datos de una sala
        Task<ChatDto> GetChatRoomAsync(Guid chatId);

        // Añade un usuario a una sala
        Task AddUserAsync(Guid chatId, UsuarioDto usuario);

        // Envía un mensaje (usuario debe existir previamente)
        Task<MensajeDto> SendMessageAsync(Guid chatId, MensajeDto mensaje);
    }
}
