using Microsoft.AspNetCore.Mvc;
using Chat.Application.Interfaces;
using Chat.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        }

        /// <summary>
        /// Crea una nueva sala de chat
        /// </summary>
        /// <returns>Id de la sala creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<ActionResult<Guid>> CreateChatRoom()
        {
            try
            {
                var chatId = await _chatService.CreateChatRoomAsync();
                return CreatedAtAction(nameof(GetChatRoom), new { id = chatId }, chatId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al crear la sala", details = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos los ids de salas de chat
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Guid>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Guid>>> ListChatRooms()
        {
            try
            {
                var chatIds = await _chatService.ListChatRoomsAsync();
                return Ok(chatIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al listar salas", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una sala de chat
        /// </summary>
        /// <param name="id">Id de la sala</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChatDto>> GetChatRoom(Guid id)
        {
            try
            {
                var chat = await _chatService.GetChatRoomAsync(id);
                return Ok(chat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener la sala", details = ex.Message });
            }
        }

        /// <summary>
        /// Añade un usuario a una sala de chat y devuelve el chat actualizado
        /// </summary>
        /// <param name="id">Id de la sala</param>
        /// <param name="usuario">Datos del usuario a añadir</param>
        [HttpPost("{id}/users")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ChatDto>> AddUserToChat(Guid id, [FromBody] UsuarioDto usuario)
        {
            try
            {
                await _chatService.AddUserAsync(id, usuario);
                var updatedChat = await _chatService.GetChatRoomAsync(id);
                return Ok(updatedChat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al añadir usuario", details = ex.Message });
            }
        }

        /// <summary>
        /// Envía un mensaje a una sala de chat y devuelve el chat actualizado
        /// </summary>
        /// <param name="id">Id de la sala</param>
        /// <param name="mensaje">Datos del mensaje</param>
        [HttpPost("{id}/messages")]
        [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ChatDto>> SendMessage(Guid id, [FromBody] MensajeDto mensaje)
        {
            try
            {
                // Asegurar que el chatId del mensaje coincide con el parámetro de ruta
                mensaje.ChatId = id;

                await _chatService.SendMessageAsync(id, mensaje);
                var updatedChat = await _chatService.GetChatRoomAsync(id);
                return Ok(updatedChat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al enviar mensaje", details = ex.Message });
            }
        }
    }
}
