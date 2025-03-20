using Microsoft.AspNetCore.Mvc;
using Domain.Services;
using Domain.Models; // Adicionado para usar IEntity
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<T, TService> : ControllerBase
        where T : class, IEntity // Modificação: 'T' agora precisa implementar 'IEntity'
        where TService : IBaseService<T>
    {
        protected readonly TService _service;

        public BaseController(TService service)
        {
            _service = service;
        }

        // Obter por ID
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<T>> GetById(Guid id)
        {
            try
            {
                var entity = await _service.GetById(id);
                if (entity == null)
                {
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "Entidade não encontrada.",
                        StatusCode = 404,
                        Details = $"Nenhuma entidade foi encontrada com o ID fornecido: {id}."
                    };
                    return NotFound(errorResponse);
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao obter a entidade.",
                    StatusCode = 500,
                    Details = $"Erro ao tentar obter a entidade: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        // Obter todos
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            try
            {
                var entities = await _service.GetAll();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao obter as entidades.",
                    StatusCode = 500,
                    Details = $"Erro ao tentar obter as entidades: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        // Atualizar por ID
        [HttpPut("{id}")]
        public async Task<ActionResult<T>> Update(Guid id, [FromBody] T entity)
        {
            try
            {
                // Verifica se o ID da entidade corresponde ao ID fornecido
                if (id != entity.Id)
                {
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "ID fornecido não corresponde ao da entidade.",
                        StatusCode = 400,
                        Details = $"O ID da URL ({id}) não corresponde ao ID da entidade ({entity.Id})."
                    };
                    return BadRequest(errorResponse);
                }

                var updatedEntity = await _service.Update(id, entity);
                if (updatedEntity == null)
                {
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "Entidade não encontrada para atualização.",
                        StatusCode = 404,
                        Details = $"Nenhuma entidade foi encontrada com o ID fornecido: {id}."
                    };
                    return NotFound(errorResponse);
                }

                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao atualizar a entidade.",
                    StatusCode = 500,
                    Details = $"Erro ao tentar atualizar a entidade: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        // Deletar por ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                if (!deleted)
                {
                    var errorResponse = new ErrorResponseDTO
                    {
                        Message = "Entidade não encontrada para deletar.",
                        StatusCode = 404,
                        Details = $"Nenhuma entidade foi encontrada com o ID fornecido: {id}."
                    };
                    return NotFound(errorResponse);
                }

                return NoContent(); // NoContent é a resposta correta para deletar
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Message = "Erro ao tentar deletar a entidade.",
                    StatusCode = 500,
                    Details = $"Erro ao tentar deletar a entidade: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}

