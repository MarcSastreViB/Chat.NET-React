using System;
using System.Collections.Generic;

namespace Chat.Application.DTOs
{
    public class ChatDto
    {
        public Guid ChatId { get; set; }
        public List<UsuarioDto> Usuarios { get; set; } = new();
        public List<MensajeDto> Mensajes { get; set; } = new();
    }
}
