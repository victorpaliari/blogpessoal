
using blogpessoal.Model;
using blogpessoal.Service;
using blogpessoal.Service.Implements;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace blogpessoal.Controllers 
{
    //[Route] - Indica o endereço Http
    [Route("~/postagens")]
    //[ApiController] indica que a classe é do tipo Controller
    [ApiController]
    public class PostagemController : ControllerBase
    {
        private readonly IPostagemService _postagemService;
        private readonly IValidator<Postagem> _postagemValidator;

        public PostagemController(

            IPostagemService postagemService,
                IValidator<Postagem> postagemValidator
        )
        {
            _postagemService = postagemService;
            _postagemValidator = postagemValidator;
        }

        //[HttpGet] é um método dentre os 4 tipos para informar para a API
        //se vai puxar(Get), incluir(Post), alterar(Put) ou deletar(delete) algum dado do backend
        //No caso abaixo [HttpGet] = "chama" um valor
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _postagemService.GetAll());
        }


        //Path de caminho (id = variavel) 
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(long id)
        {
            var Resposta = await _postagemService.GetById(id);

            if (Resposta == null)
                return NotFound();

            return Ok(Resposta);
        }


        // o que está em () é um titulo de caminho
        [HttpGet("titulo/{titulo}")]
        public async Task<ActionResult> GetByTitulo(string titulo)
        {
            return Ok(await _postagemService.GetByTitulo(titulo));
        }
        //[HttpPost] = Cria um valor
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Postagem postagem)
        {
            var validarPostagem = await _postagemValidator.ValidateAsync(postagem);

            if (!validarPostagem.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, validarPostagem);

            var Resposta = await _postagemService.Create(postagem);

            if (Resposta is null)
                return BadRequest("Tema não encontrado!");
            
            return CreatedAtAction(nameof(GetById), new { id = postagem.Id }, postagem);

        }

        //[HttpPut] = altera um valor
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Postagem postagem)
        {
            if (postagem.Id == 0)
                return BadRequest("Id da Postagem é inválido");

            var validarPostagem = await _postagemValidator.ValidateAsync(postagem);
            if (!validarPostagem.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, validarPostagem);

            }
            var Resposta = await _postagemService.Update(postagem);
            if (Resposta is null)
                return NotFound("Postagem e/ou Tema não encontrados!");


            return Ok(Resposta);
        }

        //[HttpDelete] = Deleta um valor, especificamente chamando pelo id
        [HttpDelete("{id}")]
        public async Task<ActionResult>Delete(long id)
        {
            var BuscaPostagem = await _postagemService.GetById(id);
            if (BuscaPostagem is null)
                return NotFound("Postagem não foi encontrada!");
            await _postagemService.Delete(BuscaPostagem);
            return NoContent();
        }
            


        }
}
