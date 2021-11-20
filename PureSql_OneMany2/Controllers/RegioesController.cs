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
    public class RegioesController : ControllerBase
    {
        private readonly ILogger<RegioesController> _logger;
        private readonly RegioesRepository _repository;

        public RegioesController(ILogger<RegioesController> logger,
            RegioesRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("regiao-estado-list")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IEnumerable<RegiaoEstado> GetRegiaoEstados()
        {
            var dados = _repository.GetRegiaoEstados();

            _logger.LogInformation(
                $"{nameof(GetRegiaoEstados)}: {dados.Count()} registro(s) encontrado(s)");

            return dados;
        }

        [HttpGet("regiao-estado/{codRegiao}")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<RegiaoEstado> GetEstadosByRegiao([FromRoute] string codRegiao)
        {
            var dados = _repository.GetRegiaoEstados(codRegiao).SingleOrDefault();
            _logger.LogInformation($"{nameof(GetEstadosByRegiao)}: {codRegiao}");

            if (dados is null)
                return NotFound();

            return Ok(dados);
        }

        [HttpGet("regiao/estados")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IEnumerable<Regiao> GetRegiao()
        {
            var dados = _repository.GetRegiao();

            _logger.LogInformation(
                $"{nameof(GetRegiaoEstados)}: {dados.Count()} registro(s) encontrado(s)");

            return dados;
        }

        [HttpGet("regiao/{codRegiao}/estados")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IEnumerable<Regiao> GetRegiaoById([FromRoute] string codRegiao)
        {
            var dados = _repository.GetRegiao(codRegiao);

            _logger.LogInformation(
                $"{nameof(GetRegiaoEstados)}: {dados.Count()} registro(s) encontrado(s)");

            return dados;
        }


        [HttpPost("regiao-insert")]
        [ProducesResponseType(typeof(Regiao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IEnumerable<Regiao> InsertRegiao([FromBody] RegiaoInsert regiaoInsert)
        {
            var dados = _repository.InsertRegiao(regiaoInsert);

            _logger.LogInformation(
                $"{nameof(InsertRegiao)}: registro inserted");

            return null;
        }
    }
}