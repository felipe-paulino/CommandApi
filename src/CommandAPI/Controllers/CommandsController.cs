using System.Collections.Generic;
using AutoMapper;
using CommandAPI.Data;
using CommandAPI.Dtos;
using CommandAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
namespace CommandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandAPIRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandAPIRepo repository, IMapper mapper){
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{id}", Name="GetCommandById")]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var command = _repository.GetCommandById(id);
            if (command == null){
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            var command = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(command);
            _repository.SaveChanges();

            var CommandReadDto = _mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(GetCommandById),
                new {Id = CommandReadDto.Id}, CommandReadDto);
        }

        [HttpPut("{id}")]
        public ActionResult<CommandReadDto> UpdateCommand(int id, CommandUpdateDto commandUpdateDto)
        {
            var existingCommand = _repository.GetCommandById(id);
            if(existingCommand == null){
                return NotFound();
            }
            _mapper.Map(commandUpdateDto, existingCommand);

            _repository.UpdateCommand(existingCommand);
            _repository.SaveChanges();
            
            return NoContent();
        }

        [HttpPatch("{id}")]
        public ActionResult PartialUpdateCommand(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            var existingCommand = _repository.GetCommandById(id);
            if(existingCommand == null){
                return NotFound();
            }

            var commandToPatch = _mapper.Map<CommandUpdateDto>(existingCommand);
            patchDoc.ApplyTo(commandToPatch, ModelState);

            if(!TryValidateModel(commandToPatch)){
                return ValidationProblem(ModelState);
            }

            _mapper.Map(commandToPatch, existingCommand);

            _repository.UpdateCommand(existingCommand);

            _repository.SaveChanges();

            return NoContent();
        }
    }
}