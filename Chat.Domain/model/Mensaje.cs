using System;

namespace Chat.Domain.model
{
    public class Mensaje
    {
        public Guid Id { get; set; }
        public Usuario Usuario { get; set; }

        private string _contenido = string.Empty;
        public string Contenido
        {
            get => _contenido;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Contenido no puede ser vacío.", nameof(value));

                // Primer asignado: establecer fecha de envío
                if (string.IsNullOrEmpty(_contenido))
                {
                    _contenido = value;
                    FechaEnvio = DateTime.UtcNow;
                    editado = false;
                }
                else if (!string.Equals(_contenido, value, StringComparison.Ordinal))
                {
                    // Edición posterior
                    _contenido = value;
                    editado = true;
                    FechaEditado = DateTime.UtcNow;
                }
            }
        }

        // Fecha original de envío
        public DateTime FechaEnvio { get; private set; }

        // Indica si fue editado alguna vez
        public bool editado { get; private set; } = false;

        // Fecha de la última edición (nullable)
        public DateTime? FechaEditado { get; private set; }

        public Mensaje() { }

        public Mensaje(Usuario usuario, string contenido)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            Contenido = contenido; // inicializa FechaEnvio
        }
    }
}
