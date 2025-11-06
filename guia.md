# Forma simple y práctica, qué meter en cada capa y por qué 
## suficientemente concreto para implementarlo en tu solución.

### Objetivo del proyecto (resumen)
- Permitir múltiples salas de chat (`ChatRoom`) sin base de datos por ahora.
- Usar patrón Repositorio para poder cambiar a BD más adelante.
- Para enviar mensajes, el usuario debe existir previamente en la sala.
- Seguir Clean Architecture y principios DDD (entidades ricas y agregados simples).

---

### Domain (núcleo de la lógica)
- Qué poner: entidades, value objects, reglas de negocio puras, excepciones de dominio, interfaces de repositorio.
- Entidades/agregado:
  - `ChatRoom` (Aggregate Root): contiene `Usuarios` y `Mensajes`. Debe tener `Id` (Guid) como identidad de la sala.
  - `Usuario` (entidad): identidad por `UserName` dentro de una sala (el `UserName` es su identificador; no añadir otro Id).
  - `Mensaje` (entidad): `Id` (Guid) para trazabilidad, referencia a `Usuario`, `Contenido`, `FechaEnvio`.
- Reglas clave (invariantes):
  - No se pueden duplicar usuarios por `UserName` dentro de una sala.
  - No se puede enviar mensaje si el usuario no existe en la sala.
  - El contenido del mensaje no puede ser vacío.
- Interfaz repositorio: `IChatRepository` (contrato, sin lógica) para trabajar con `ChatRoom`.

### Application (casos de uso / orquestación)
- Qué poner: DTOs, contratos de servicios (`IChatService`), validación de entrada, orquestación de casos de uso.
- Casos de uso mínimos:
  - Crear sala: `CreateChatRoomAsync` (devuelve `Id` Guid de la sala).
  - Listar salas: `ListChatRoomsAsync` (devuelve lista de `Id` Guid).
  - Obtener sala: `GetChatRoomAsync` (devuelve DTO con `ChatId` Guid).
  - Añadir usuario a sala: `AddUserAsync`.
  - Enviar mensaje: `SendMessageAsync` (requiere usuario existente, devuelve DTO con `Id` Guid).
- Mapear entre entidades y DTOs (manual u `AutoMapper`, opcional ahora).

### Infrastructure (implementaciones concretas)
- Qué poner: `InMemoryChatRepository` (sin BD) y `ChatService` (implementación de `IChatService`).
- `InMemoryChatRepository` debe soportar múltiples salas y ser seguro para acceso concurrente básico.
- Más adelante podrás reemplazar por `EfChatRepository` + `ChatDbContext`.

### Api (exposición HTTP)
- `ChatController` con endpoints REST mínimos:
  - `POST /api/chats` -> crea una sala (devuelve `{ id: string }` con el Guid).
  - `GET /api/chats` -> lista `{ ids: string[] }` o una lista simple de `string` (Guids).
  - `GET /api/chats/{id}` -> obtiene sala por id (devuelve `ChatDto` con `ChatId` Guid).
  - `POST /api/chats/{id}/users` -> añade usuario a sala (el `UserName` identifica al usuario).
  - `POST /api/chats/{id}/messages` -> envía mensaje (devuelve `MensajeDto` con `Id` Guid).

### Dependencias entre proyectos (reglas)
- `Chat.Domain` <- aislado.
- `Chat.Application` -> `Chat.Domain`.
- `Chat.Infrastructure` -> `Chat.Domain`, `Chat.Application`.
- `Chat.Api` -> `Chat.Application`, `Chat.Infrastructure`.

### Qué es necesario ahora (imprescindible)
1) Domain
- Hacer `public` las entidades (`Usuario`, `Mensaje`, `ChatRoom`).
- Inicializar colecciones en `ChatRoom` (listas vacías por defecto).
- Definir interfaz `IChatRepository` con métodos necesarios (ver firmas abajo).
- (Opcional recomendado) Agregar métodos de dominio a `ChatRoom` para validar invariantes: `AddUser`, `AddMessage` (solo firmas si prefieres implementar después).
- IDs: `ChatRoom.Id` (Guid) y `Mensaje.Id` (Guid). `Usuario` se identifica por `UserName` (no añadir otro Id).

2) Application
- Crear DTOs: `UsuarioDto`, `MensajeDto`, `ChatDto`, `ChatRoomDto` (si distingues `Chat` vs `ChatRoom`).
- Definir `IChatService` con los casos de uso descritos (firmas).
- IDs en DTOs: incluir `ChatId` (Guid) en `ChatDto`, `Id` (Guid) en `MensajeDto`. No incluir `Usuario.Id` adicional (usar `UserName`).

3) Infrastructure
- Implementar `InMemoryChatRepository` (sin BD) con almacenamiento en memoria para múltiples salas.
- Implementar `ChatService` usando el repositorio y aplicando las reglas (no enviar mensajes si usuario no existe).
- Generación de IDs:
  - `ChatRoom.Id`: `Guid.NewGuid()`.
  - `Mensaje.Id`: `Guid.NewGuid()`.
- Registrar en DI (repositorio + servicio).

4) Api
- Crear `ChatController` con los endpoints REST mínimos arriba.
- Respuestas deben incluir IDs donde aplique (p. ej. `201 Created` con `Location` a `/api/chats/{id}` y el `id` en el cuerpo).

### Qué NO es necesario ahora (posponer)
- EF Core, migraciones, `DbContext` (posponer hasta querer BD).
- Autenticación/autorización.
- AutoMapper/FluentValidation (útiles pero prescindibles al inicio).
- Paginación y filtros de mensajes (añadir más adelante).

---

## Checklist práctica (paso a paso)
1. Domain
   - Asegurar `public` en `Chat.Domain\model\Usuario.cs`, `Mensaje.cs`, `ChatRoom.cs`.
   - Inicializar listas en `ChatRoom`.
   - Crear `Chat.Domain\repositories\IChatRepository.cs` (firmas abajo).
   - IDs: confirmar `ChatRoom.Id` (Guid) y `Mensaje.Id` (Guid).
2. Application
   - Crear `Chat.Application\DTOs`: `UsuarioDto`, `MensajeDto`, `ChatDto` (y/o `ChatRoomDto`).
   - Añadir campos de IDs en DTOs: `ChatDto.ChatId` (Guid), `MensajeDto.Id` (Guid).
   - Crear `Chat.Application\Interfaces\IChatService.cs` con firmas abajo.
3. Infrastructure
   - Crear `Chat.Infrastructure\Repositories\InMemoryChatRepository.cs` con soporte para múltiples salas.
   - Crear `Chat.Infrastructure\Services\ChatService.cs` que use el repositorio.
   - Generación de IDs: `Guid.NewGuid()` para `ChatRoom.Id` y `Mensaje.Id`.
   - Registrar en `Chat.Api\Program.cs`: `IChatRepository` e `IChatService`.
4. Api
   - Crear `Chat.Api\Controllers\ChatController.cs` con endpoints REST.
   - Asegurar que las respuestas incluyen IDs (`201 Created` + Location).
5. Probar
   - Crear sala -> añadir usuario -> enviar mensaje -> obtener sala.

---

## Firmas de contratos (qué deben hacer, no cómo)

### Repositorio en Domain (`Chat.Domain/repositories/IChatRepository.cs`)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Domain.model;

namespace Chat.Domain.repositories
{
    public interface IChatRepository
    {
        // Crea una nueva sala y la devuelve (con id asignado)
        Task<ChatRoom> CreateChatAsync();

        // Devuelve la sala por id o null si no existe
        Task<ChatRoom?> GetChatAsync(Guid id);

        // Devuelve todas las salas (ids y/o info básica)
        Task<IEnumerable<ChatRoom>> ListChatsAsync();

        // Devuelve los usuarios de una sala
        Task<IEnumerable<Usuario>> GetUsuariosAsync(Guid chatId);

        // Devuelve los mensajes de una sala
        Task<IEnumerable<Mensaje>> GetMensajesAsync(Guid chatId);

        // Añade un usuario a la sala (no duplicar por UserName)
        Task AddUsuarioAsync(Guid chatId, Usuario usuario);

        // Añade un mensaje a la sala (el usuario debe existir)
        Task AddMensajeAsync(Guid chatId, Mensaje mensaje);

        // Persiste cambios si aplica (en memoria: no-op)
        Task SaveChangesAsync();
    }
}
```

### Servicio de aplicación (`Chat.Application/Interfaces/IChatService.cs`)
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Application.DTOs;

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
```

### DTOs sugeridos (`Chat.Application\DTOs`)
- `UsuarioDto`: `UserName`, `FotoPerfilBase64?`.
- `MensajeDto`: `UserName`, `Contenido`, `FechaEnvio`.
- `ChatDto`: `Usuarios: List<UsuarioDto>`, `Mensajes: List<MensajeDto>`.

---

## Pautas de implementación (alto nivel)
- `InMemoryChatRepository`
  - Diccionario `chatId -> ChatRoom` (p. ej. `ConcurrentDictionary<Guid, ChatRoom>`).
  - Ids `Guid` para nuevas salas (`ChatRoom.Id = Guid.NewGuid()`).
  - Añadir usuario sin duplicar por `UserName`.
  - Al añadir mensaje, exigir usuario existente y asignar `Mensaje.Id = Guid.NewGuid()`.
- `ChatService`
  - Validar entradas: `chatId != Guid.Empty`, `UserName` y `Contenido` no vacíos.
  - Orquestar repositorio y mapear a DTOs (asegurar `ChatDto.ChatId` Guid, `MensajeDto.Id` Guid).
- `ChatController`
  - Responder 404 si la sala no existe y 400 si la validación falla.
  - En `POST /api/chats`, devolver `201 Created` con `Location: /api/chats/{id}` (Guid).

---

## Registro en DI (`Chat.Api/Program.cs`)
```csharp
builder.Services.AddSingleton<IChatRepository, InMemoryChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
```

---

## Estructura de carpetas y proyectos
```mermaid
graph TD
  subgraph "Solution Chat (.NET 8)"
    subgraph "Chat.Domain"
      D1["model/Usuario.cs"]
      D2["model/Mensaje.cs"]
      D3["model/ChatRoom.cs"]
      D4["repositories/IChatRepository.cs"]
    end
    subgraph "Chat.Application"
      A1["DTOs/UsuarioDto.cs"]
      A2["DTOs/MensajeDto.cs"]
      A3["DTOs/ChatDto.cs"]
      A4["Interfaces/IChatService.cs"]
    end
    subgraph "Chat.Infrastructure"
      I1["Repositories/InMemoryChatRepository.cs"]
      I2["Services/ChatService.cs"]
      I3["Persistence/ChatDbContext.cs (opcional)"]
    end
    subgraph "Chat.Api"
      AP1["Program.cs"]
      AP2["Controllers/ChatController.cs"]
      AP3["Endpoints REST (POST/GET)"]
    end
  end

  AP1 --> A4
  AP1 --> I1
  I1 --> D4
  I2 --> A4
  A4 --> D4
```

---

## Mapa de llamadas (end-to-end)

### Dependencias (alto nivel)
```mermaid
graph TD
  AP["Chat.Api (Controllers)"] --> A["Chat.Application (IChatService)"]
  A --> I["Chat.Infrastructure (ChatService)"]
  I --> R["Infra Repo (InMemory/Ef) implementa IChatRepository"]
  R --> D["Chat.Domain (Entidades + IChatRepository)"]
```

- `ChatController` llama a `IChatService`.
- `ChatService` (Infrastructure) orquesta y usa `IChatRepository`.
- El repositorio trabaja con entidades del dominio (`ChatRoom`, `Usuario`, `Mensaje`).

### Flujos por caso de uso

1) Crear sala: `POST /api/chats`
```mermaid
sequenceDiagram
  participant C as Controller
  participant S as IChatService (ChatService)
  participant R as IChatRepository
  participant D as ChatRoom (Domain)

  C->>S: CreateChatRoomAsync()
  S->>R: CreateChatAsync()
  R-->>S: ChatRoom (Id asignado)
  S-->>C: Id (Guid) de la sala creada
```

2) Listar salas: `GET /api/chats`
```mermaid
sequenceDiagram
  participant C as Controller
  participant S as IChatService (ChatService)
  participant R as IChatRepository

  C->>S: ListChatRoomsAsync()
  S->>R: ListChatsAsync()
  R-->>S: IEnumerable<ChatRoom>
  S-->>C: IEnumerable<Guid>
```

3) Obtener sala: `GET /api/chats/{id}`
```mermaid
sequenceDiagram
  participant C as Controller
  participant S as IChatService (ChatService)
  participant R as IChatRepository

  C->>S: GetChatRoomAsync(chatId: Guid)
  S->>R: GetChatAsync(chatId: Guid)
  R-->>S: ChatRoom? (null si no existe)
  S-->>C: ChatDto (404 si null)
```

4) Añadir usuario: `POST /api/chats/{id}/users`
```mermaid
sequenceDiagram
  participant C as Controller
  participant S as IChatService (ChatService)
  participant R as IChatRepository
  participant D as ChatRoom (Domain)

  C->>S: AddUserAsync(chatId: Guid, UsuarioDto)
  S->>R: GetChatAsync(chatId: Guid)
  R-->>S: ChatRoom (o null->404)
  S->>D: ChatRoom.AddUser(Usuario)
  S->>R: AddUsuarioAsync(chatId: Guid, Usuario)
  S->>R: SaveChangesAsync()
  S-->>C: 204 NoContent (o 409 si ya existía)
```

5) Enviar mensaje: `POST /api/chats/{id}/messages`
```mermaid
sequenceDiagram
  participant C as Controller
  participant S as IChatService (ChatService)
  participant R as IChatRepository
  participant D as ChatRoom (Domain)

  C->>S: SendMessageAsync(chatId: Guid, MensajeDto)
  S->>R: GetChatAsync(chatId: Guid)
  R-->>S: ChatRoom (o null->404)
  S->>D: validar que Usuario existe en ChatRoom
  alt usuario no existe
    S-->>C: 400/409 (según política)
  else usuario existe
    S->>D: ChatRoom.AddMessage(Mensaje)
    S->>R: AddMensajeAsync(chatId: Guid, Mensaje)
    S->>R: SaveChangesAsync()
    S-->>C: MensajeDto (201 Created)
  end
```

Notas:
- La validación de reglas (no duplicados, usuario requerido) vive en el agregado `ChatRoom` y se orquesta desde `IChatService` antes de persistir.
- Al cambiar la implementación del repositorio (in-memory → EF) no se toca `Chat.Application` ni `Chat.Api`.