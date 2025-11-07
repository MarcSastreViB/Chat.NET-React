using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.model;
using Chat.Domain.Repositories;
using Chat.Infrastructure.Repositories;

namespace Chat.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly InMemoryChatRepository _chatRepo;
        private readonly InMemoryUserRepository _userRepo;

        public ChatService(InMemoryChatRepository chatRepo, InMemoryUserRepository userRepo)
        {
            _chatRepo = chatRepo ?? throw new ArgumentNullException(nameof(chatRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task<Guid> CreateChatRoomAsync()
        {   
            var chat = await _chatRepo.CreateChatAsync();
            await _chatRepo.SaveChangesAsync();
            return chat.Id;
        }

        public async Task<IEnumerable<Guid>> ListChatRoomsAsync()
        {
            var chats = await _chatRepo.ListChatsAsync();
            return chats.Select(c => c.Id);
        }

        public async Task<ChatDto> GetChatRoomAsync(Guid chatId)
        {
            if (chatId == Guid.Empty) throw new ArgumentException("chatId no puede ser Guid.Empty", nameof(chatId));

            var chat = await _chatRepo.GetChatAsync(chatId);
            if (chat is null) throw new KeyNotFoundException($"Chat {chatId} no encontrado.");

            // map to DTO
            var dto = new ChatDto
            {
                ChatId = chat.Id,
                Usuarios = chat.Usuarios.Select(u => new UsuarioDto
                {
                    UserName = u.userName,
                    PhotoPerfilBase64 = u.fotoPerfil != null && u.fotoPerfil.Length > 0 ? Convert.ToBase64String(u.fotoPerfil) : null
                }).ToList(),
                Mensajes = chat.Mensajes.Select(m => new MensajeDto
                {
                    Id = m.Id,
                    ChatId = chat.Id,
                    UserName = m.Usuario?.userName ?? string.Empty,
                    Contenido = m.Contenido,
                    FechaEnvio = m.FechaEnvio,
                    Editado = m.editado,
                    FechaEditado = m.FechaEditado
                }).ToList()
            };

            return dto;
        }

        public async Task AddUserAsync(Guid chatId, UsuarioDto usuario)
        {
            if (chatId == Guid.Empty) throw new ArgumentException("chatId no puede ser Guid.Empty", nameof(chatId));
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (string.IsNullOrWhiteSpace(usuario.UserName)) throw new ArgumentException("UserName no puede ser vacío", nameof(usuario.UserName));

            var chat = await _chatRepo.GetChatAsync(chatId);
            if (chat is null) throw new KeyNotFoundException($"Chat {chatId} no encontrado.");

            // Resolver usuario en repositorio global
            var userDomain = await _userRepo.GetUserAsync(usuario.UserName);
            if (userDomain is null) throw new KeyNotFoundException($"Usuario '{usuario.UserName}' no existe.");

            // Añadir al agregado (mantiene invariantes)
            lock (chat)
            {
                if (!chat.AddUser(userDomain))
                    throw new InvalidOperationException($"Usuario '{usuario.UserName}' ya existe en la sala {chatId}.");
            }

            await _chatRepo.AddUsuarioAsync(chatId, userDomain.userName);
            await _chatRepo.SaveChangesAsync();
        }

        public async Task<MensajeDto> SendMessageAsync(Guid chatId, MensajeDto mensaje)
        {
            if (chatId == Guid.Empty) throw new ArgumentException("chatId no puede ser Guid.Empty", nameof(chatId));
            if (mensaje == null) throw new ArgumentNullException(nameof(mensaje));
            if (string.IsNullOrWhiteSpace(mensaje.UserName)) throw new ArgumentException("UserName no puede ser vacío", nameof(mensaje.UserName));
            if (string.IsNullOrWhiteSpace(mensaje.Contenido)) throw new ArgumentException("Contenido no puede ser vacío", nameof(mensaje.Contenido));

            var chat = await _chatRepo.GetChatAsync(chatId);
            if (chat is null) throw new KeyNotFoundException($"Chat {chatId} no encontrado.");

            // Comprobar existencia global del usuario
            var userDomain = await _userRepo.GetUserAsync(mensaje.UserName);
            if (userDomain is null) throw new KeyNotFoundException($"Usuario '{mensaje.UserName}' no existe.");

            // Verificar que el usuario pertenece al chat
            var belongs = chat.Usuarios.Any(u => string.Equals(u.userName, mensaje.UserName, StringComparison.OrdinalIgnoreCase));
            if (!belongs) throw new InvalidOperationException($"Usuario '{mensaje.UserName}' no pertenece a la sala {chatId}.");

            // Crear entidad de dominio
            var mensajeDomain = new Mensaje(userDomain, mensaje.Contenido)
            {
                Id = mensaje.Id == Guid.Empty ? Guid.NewGuid() : mensaje.Id
            };

            lock (chat)
            {
                if (!chat.AddMessage(mensajeDomain))
                    throw new InvalidOperationException("La operación fue rechazada por las invariantes del agregado.");
            }

            await _chatRepo.AddMensajeAsync(chatId, mensajeDomain);
            await _chatRepo.SaveChangesAsync();

            // Mapear y devolver
            return new MensajeDto
            {
                Id = mensajeDomain.Id,
                ChatId = chatId,
                UserName = mensajeDomain.Usuario.userName,
                Contenido = mensajeDomain.Contenido,
                FechaEnvio = mensajeDomain.FechaEnvio,
                Editado = mensajeDomain.editado,
                FechaEditado = mensajeDomain.FechaEditado
            };
        }
    }
}
