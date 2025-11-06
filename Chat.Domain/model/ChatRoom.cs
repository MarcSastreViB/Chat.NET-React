using System;
using System.Collections.Generic;

namespace Chat.Domain.model
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public List<Usuario> Usuarios { get; set; } = new();
        public List<Mensaje> Mensajes { get; set; } = new();

        public ChatRoom() { }

        // Añade usuario si no existe (usa Equals/GetHashCode de Usuario)
        public bool AddUser(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (Usuarios.Contains(usuario)) return false;
            Usuarios.Add(usuario);
            return true;
        }

        // Añade mensaje solo si el usuario existe en la sala
        public bool AddMessage(Mensaje mensaje)
        {
            if (mensaje == null) throw new ArgumentNullException(nameof(mensaje));
            if (mensaje.Usuario == null) throw new ArgumentException("Mensaje must reference a Usuario", nameof(mensaje));
            if (!Usuarios.Contains(mensaje.Usuario))
            {
                // Opcional: lanzar excepción en lugar de false, según tu política de negocio
                return false;
            }

            Mensajes.Add(mensaje);
            return true;
        }
    }
}
