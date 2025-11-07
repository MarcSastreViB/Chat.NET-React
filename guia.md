# Forma simple y práctica, qué meter en cada capa y por qué 
## suficientemente concreto para implementarlo en tu solución.

### Objetivo del proyecto (resumen)
- Permitir múltiples salas de chat (`ChatRoom`) sin base de datos por ahora.
- Usar patrón Repositorio para poder cambiar a BD más adelante.
- Para enviar mensajes, el usuario debe existir previamente en la sala.
- Usuarios gestionados de forma independiente (repositorio propio), y luego se asocian a chats.
- Seguir Clean Architecture y principios DDD (entidades ricas y agregados simples).

---

### Domain (núcleo de la lógica)
- Entidades/agregado:
  - `ChatRoom` (Aggregate Root): contiene `Usuarios` (referencias) y `Mensajes`. `Id` (Guid).
  - `Usuario`: identidad por `UserName` (no otro Id). No depende de chats; los chats referencian usuarios existentes.
  - `Mensaje`: `Id` (Guid), referencia a `Usuario` y contenido.
- Interfaces:
  - `IChatRepository`: operaciones sobre salas y sus relaciones (añadir usuario ya existente, añadir mensaje).
  - `IUserRepositories` (o renombrar a `IUserRepository`): operaciones CRUD sobre usuarios globales.
- Invariantes:
  - Un chat no duplica usuarios (comparación por `UserName`).
  - Un mensaje sólo puede añadirse si el usuario ya pertenece al chat.

### Application
- `IChatService`: orquesta casos de uso de chats (crear sala, listar, obtener, añadir usuario existente, enviar mensaje).
- `IUserService` (opcional futuro): gestionará creación/listado de usuarios independientes.
- DTOs: `UsuarioDto`, `MensajeDto`, `ChatDto`.
- Los métodos de añadir usuario a chat deben primero resolver el usuario vía `IUserRepositories` antes de delegar al repositorio de chat.

### Infrastructure
- `InMemoryUserRepository`: almacena usuarios en `ConcurrentDictionary<string, Usuario>`.
- `InMemoryChatRepository`: almacena salas en `ConcurrentDictionary<Guid, ChatRoom>`; no crea usuarios, sólo asocia instancias existentes.
- `ChatService`: inyecta ambos repositorios (`IChatRepository`, `IUserRepositories`).

### Api
- Endpoints chat: crear, listar, obtener, añadir usuario existente, enviar mensaje.
- Endpoints usuario (opcional): crear usuario, listar usuarios.

### Checklist adicional (usuarios independientes)
- Crear repositorio de usuarios.
- Inyectar repositorio de usuarios en el servicio de chat.
- Validar existencia de usuario antes de añadirlo al chat.
- No crear usuarios desde el repositorio de chat.

---

(El resto del documento se mantiene; esta sección resume la independencia de usuarios.)