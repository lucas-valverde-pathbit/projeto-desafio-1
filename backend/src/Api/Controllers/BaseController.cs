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
            var entity = await _service.GetById(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // Obter todos
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAll()

        {
            var entities = await _service.GetAll();
            return Ok(entities);
        }

        // Atualizar por ID
        [HttpPut("{id}")]
        public async Task<ActionResult<T>> Update(Guid id, [FromBody] T entity)
        {
            try
            {
                // Verifica se o ID da entidade corresponde ao ID fornecido
                if (id != entity.Id)
                    return BadRequest("O ID fornecido não corresponde ao ID da entidade.");

                var updatedEntity = await _service.Update(id, entity);
                if (updatedEntity == null)
                    return NotFound("A entidade não foi encontrada.");

                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao tentar atualizar a entidade: {ex.Message}");
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
                    return NotFound("Entidade não encontrada para deletar.");

                return NoContent(); // NoContent é a resposta correta para deletar
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao tentar deletar a entidade: {ex.Message}");
            }
        }
    }
}
