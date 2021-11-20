using Candal.Data;
using Candal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Candal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ILogger<TodosController> _logger;
        private readonly TodosRepository _repository;

        public TodosController(ILogger<TodosController> logger,
            TodosRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("todo-all")]
        [ProducesResponseType(typeof(Todo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IEnumerable<Todo> GetAll()
        {
            var dados = _repository.Get();

            _logger.LogInformation(
                $"{nameof(GetAll)}: {dados.Count()} rows found");

            return dados;
        }

        [HttpGet("todo/{id}")]
        [ProducesResponseType(typeof(Todo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<Todo> GetOne1([FromRoute] int id)
        {
            var dados = _repository.Get(id).SingleOrDefault();
            _logger.LogInformation($"{nameof(GetOne1)}: {id}");

            if (dados is null)
                return NotFound();

            return Ok(dados);
        }

        [HttpGet("todo")]
        [ProducesResponseType(typeof(Todo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<Todo> GetOne2([FromQuery] int id)
        {
            var dados = _repository.Get(id).SingleOrDefault();
            _logger.LogInformation($"{nameof(GetOne2)}: {id}");

            if (dados is null)
                return NotFound();

            return Ok(dados);
        }
        [HttpPost("todo-insert")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Todo InsertTodo([FromBody] TodoInsert todoInsert)
        {
            var todo = _repository.InsertTodo(todoInsert);

            _logger.LogInformation(
                $"{nameof(InsertTodo)}: row inserted");

            return todo;
        }

        [HttpPut("todo-update")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Todo UpdateTodo([FromBody] Todo todoUpdate)
        {
            var todo = _repository.UpdateTodo(todoUpdate);

            _logger.LogInformation(
                $"{nameof(UpdateTodo)}: row updated");

            return todo;
        }

        [HttpDelete("todo-delete/{id}")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public int DeleteTodo([FromRoute] int id)
        {
            var todo = _repository.DeleteTodo(id);

            _logger.LogInformation(
                $"{nameof(DeleteTodo)}: row deleted");

            return todo;
        }
    }
}