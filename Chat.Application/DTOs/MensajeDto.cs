using System;

namespace Chat.Application.DTOs
{
    public class MensajeDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Editado { get; set; }
        public DateTime? FechaEditado { get; set; }
    }
}
