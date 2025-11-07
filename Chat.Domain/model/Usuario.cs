using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Domain.model
{
    public class Usuario : IEquatable<Usuario>
    {
        public string userName { get; set; } = string.Empty;
        public byte[] fotoPerfil { get; set; } = Array.Empty<byte>();
        public List<ChatRoom> rooms { get; set; } = new();
        public Usuario() { }

        public bool Equals(Usuario? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return string.Equals(this.userName, other.userName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Usuario);

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(userName ?? string.Empty);
        }

        public static bool operator ==(Usuario? left, Usuario? right) => Equals(left, right);
        public static bool operator !=(Usuario? left, Usuario? right) => !Equals(left, right);
    }
}
