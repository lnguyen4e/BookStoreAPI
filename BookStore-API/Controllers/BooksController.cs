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

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");

                var books = await _bookRepository.FindAll();

                if(books == null)
                {
                    _logger.LogWarn($"{location}: Not Found");
                    return NotFound();
                }

                var response = _mapper.Map<IList<BookDTO>>(books);

                return Ok(response);
            }
            catch(Exception ex)
            {
                return InternalError($"{location}:{ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");

                var book = await _bookRepository.FindById(id);

                if (book == null)
                {
                    _logger.LogWarn($"{location}: Not Found");
                    return NotFound();
                }

                var response = _mapper.Map<BookDTO>(book);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}:{ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");

                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}:Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}:Data was incomplete");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await  _bookRepository.Create(book);

                if (!isSuccess)
                {
                    _logger.LogError($"{location}: Creation Failed");
                    return InternalError("Book creation failed");
                }

                return Created("Create", new { book});

            }
            catch (Exception ex)
            {
                return InternalError($"{location}:{ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");
                if(id<  1 || bookDTO == null || bookDTO.Id != id)
                {
                    _logger.LogWarn($"{location}: Update failed with bad data - id: {id}");
                    return BadRequest();
                }

                var isExists = await _bookRepository.IsExists(id);
                
                if(!isExists){
                    _logger.LogWarn($"{location}: Failed to retreive record with id :{id}");
                    return NotFound();
                }

                if(!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await _bookRepository.Update(book);

                if(!isSuccess){
                    return InternalError($"{location}: Update failed");
                }

                _logger.LogWarn($"{location}: Record with id : {id} successfully updated");
                return NoContent();
            }
            catch(Exception ex)
            {
                return InternalError($"{location}:{ex.Message} - {ex.InnerException}");
            }
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Delete Attemped on record wit id : {id}");
                if(id <1)
                {
                    _logger.LogWarn($"{location}: Delete failed with bad data id: {id}");
                    return BadRequest();
                }
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Delete failed with no data retreive id:{id}");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);

                var isSuccess = await _bookRepository.Delete(book);

                if (!isSuccess)
                {
                    return InternalError($"{location} : Deletion fail");
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                return InternalError($"{location}:{ex.Message} - {ex.InnerException}");
            }

        }
                private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);

            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong!!");
        }
    }
}