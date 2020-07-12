using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class PlexAccountController : BaseController
    {

        private readonly IPlexAccountService _plexAccountService;
        private readonly IMapper _mapper;

        public PlexAccountController(IPlexAccountService plexAccountService, IMapper mapper)
        {
            _plexAccountService = plexAccountService;
            _mapper = mapper;
        }


        //GET: api/<PlexAccountController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlexAccountDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] bool enabledOnly = false)
        {
            try
            {
                var validationResult = await _plexAccountService.GetAllPlexAccountsAsync(enabledOnly);
                if (validationResult.IsFailed)
                {
                    return BadRequest(validationResult.Errors);
                }

                var result = _mapper.Map<List<PlexAccountDTO>>(validationResult.Value);
                if (!result.Any() && enabledOnly)
                {
                    Log.Debug("Could not find any enabled accounts");
                    return NotFound();
                }
                Log.Debug($"Returned {result.Count} accounts");
                return Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // GET api/<PlexAccountController>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlexAccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            try
            {
                var validationResult = await _plexAccountService.GetPlexAccountAsync(id);
                if (validationResult.IsFailed)
                {
                    return BadRequest(validationResult.Errors);
                }

                if (validationResult.Value != null)
                {
                    return Ok(_mapper.Map<PlexAccountDTO>(validationResult.Value));
                }
                string message = $"Could not find a PlexAccount with Id: {id}";
                Log.Warning(message);
                return NotFound(message);

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        // PUT api/<PlexAccountController>/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlexAccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] UpdatePlexAccountDTO account)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            try
            {
                account.Id = id;
                var result = await _plexAccountService.UpdateAccountAsync(_mapper.Map<PlexAccount>(account));
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(_mapper.Map<PlexAccountDTO>(result));

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        // POST api/<AccountController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreatePlexAccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] CreatePlexAccountDTO newAccount)
        {
            try
            {
                var result = _mapper.Map<PlexAccount>(newAccount);
                var validationResult = await _plexAccountService.CreatePlexAccountAsync(result);
                if (validationResult.IsFailed)
                {
                    return BadRequest(validationResult.Errors);
                }

                if (validationResult.Value != null)
                {
                    var message = $"Account with id {validationResult.Value.Id} was created and/or retrieved successfully";
                    Log.Information(message);
                    return Created(message, _mapper.Map<PlexAccountDTO>(validationResult.Value));

                }
                else
                {
                    var message = $"Could not process the account";
                    Log.Warning(message);
                    return UnprocessableEntity(message);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }



        // DELETE api/<AccountController>/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            try
            {
                var result = await _plexAccountService.DeletePlexAccountAsync(id);
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }
                var message = $"Successfully deleted account with id: {id}";
                Log.Debug(message);
                return Ok(message);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Validate([FromBody] CredentialsDTO account)
        {
            try
            {
                var validationResult = await _plexAccountService.ValidatePlexAccountAsync(account.Username, account.Password);

                if (validationResult.IsFailed)
                {
                    string message = $"The account failed to validate, {validationResult.Errors}";
                    Log.Error(message);
                    return BadRequest(validationResult.Errors);
                }

                if (validationResult.Value)
                {
                    string message = $"Account with username: {account.Username} was valid";
                    Log.Information(message);
                    return Ok(message);
                }
                else
                {
                    string message = $"Account with username: {account.Username} was invalid";
                    Log.Warning(message);
                    return Unauthorized(message);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet("check/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(List<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 5) { return BadRequest("Invalid username"); }
            try
            {
                var result = await _plexAccountService.CheckIfUsernameIsAvailableAsync(username);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                if (result.Value)
                {
                    string message = $"Username: {username} is available";
                    Log.Debug(message);
                    return Ok($"Username: {username} is available");
                }
                else
                {
                    string message = $"Account with username: \"{username}\" already exists!";
                    Log.Warning(message);
                    return Forbid(message);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}
