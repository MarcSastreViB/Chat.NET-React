using Microsoft.AspNetCore.Mvc;
using Chat.Domain.model;
using Chat.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly InMemoryUserRepository _userRepo;

        public UserController(InMemoryUserRepository userRepo)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="request">Datos del usuario</param>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest(new { error = "UserName es requerido" });

                var usuario = new Usuario
                {
                    userName = request.UserName,
                    fotoPerfil = string.IsNullOrWhiteSpace(request.FotoPerfilBase64) 
                        ? Array.Empty<byte>() 
                        : Convert.FromBase64String(request.FotoPerfilBase64)
                };

                var created = await _userRepo.CreateUserAsync(usuario);
                
                var response = new UserResponse
                {
                    UserName = created.userName,
                    FotoPerfilBase64 = created.fotoPerfil != null && created.fotoPerfil.Length > 0 
                        ? Convert.ToBase64String(created.fotoPerfil) 
                        : null
                };

                return CreatedAtAction(nameof(GetUser), new { userName = created.userName }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al crear usuario", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su userName
        /// </summary>
        /// <param name="userName">Nombre del usuario</param>
        [HttpGet("{userName}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetUser(string userName)
        {
            try
            {
                var usuario = await _userRepo.GetUserAsync(userName);
                if (usuario is null)
                    return NotFound(new { error = $"Usuario '{userName}' no encontrado" });

                var response = new UserResponse
                {
                    UserName = usuario.userName,
                    FotoPerfilBase64 = usuario.fotoPerfil != null && usuario.fotoPerfil.Length > 0 
                        ? Convert.ToBase64String(usuario.fotoPerfil) 
                        : null
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener usuario", details = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos los usuarios
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> ListUsers()
        {
            try
            {
                var usuarios = await _userRepo.ListUsuariosAsync();
                var response = usuarios.Select(u => new UserResponse
                {
                    UserName = u.userName,
                    FotoPerfilBase64 = u.fotoPerfil != null && u.fotoPerfil.Length > 0 
                        ? Convert.ToBase64String(u.fotoPerfil) 
                        : null
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al listar usuarios", details = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="userName">Nombre del usuario a eliminar</param>
        [HttpDelete("{userName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            try
            {
                var deleted = await _userRepo.DeleteUserAsync(userName);
                if (!deleted)
                    return NotFound(new { error = $"Usuario '{userName}' no encontrado" });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al eliminar usuario", details = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un usuario existe
        /// </summary>
        /// <param name="userName">Nombre del usuario</param>
        [HttpHead("{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserExists(string userName)
        {
            try
            {
                var exists = await _userRepo.UserExistsAsync(userName);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al verificar usuario", details = ex.Message });
            }
        }
    }

    // DTOs para el controlador de usuarios
    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string? FotoPerfilBase64 { get; set; }
    }

    public class UserResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string? FotoPerfilBase64 { get; set; }
    }
}
