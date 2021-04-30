using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try 
            {
                _logger.LogInfo("Attempted Get All Authors");

                var authors = await _authorRepository.FindAll();

                if (authors == null)
                {
                    _logger.LogWarn("There is no authors in the databse");
                    return NotFound();
                }
                var response = _mapper.Map<IList<AuthorDTO>>(authors);

                _logger.LogInfo("Successfully got all Authors");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
           
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted get {id}-author");

                var author = await _authorRepository.FindById(id);

                if(author == null)
                {
                    _logger.LogWarn($"Cannot find the author with id:{id}");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Successfullly got author {id}-author");

                return Ok(response);
            }
            catch(Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author Submission Attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Data was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess)
                {
                    return InternalError($"Author creation failed");
                }
                _logger.LogInfo("Author Created");
                return Created("Create", new { author });
            }
            catch(Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id,[FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author Update Attempted -id:{id}");
                if (id<1 || authorDTO == null || id !=authorDTO.Id)
                {
                    _logger.LogWarn($"Author Update failed with bad data");
                    return BadRequest();
                }
                var isExists = await _authorRepository.IsExists(id);
                if (!isExists)
                {
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                author.Id = id;
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                    return InternalError($"Update Operation Fail");
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if(id < 1)
                {
                    return BadRequest();
                }
                var isExists = await _authorRepository.IsExists(id);
                if (!isExists)
                {
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);

                var isSuccess = await _authorRepository.Delete(author);

                if (!isSuccess)
                {
                    return InternalError($"Author Delete Failed");
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }
            private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);

            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong!!");
        }
    }
}