using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class AccountController : BaseController
    {

        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AccountController(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }


        //GET: api/<AccountController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccountDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<AccountDTO>))]
        public async Task<IActionResult> GetAll([FromQuery] bool enabledOnly = false)
        {
            var data = await _accountService.GetAllAccountsAsync(enabledOnly);
            var result = _mapper.Map<List<AccountDTO>>(data);
            if (!result.Any() && enabledOnly)
            {
                Log.Debug("Could not find any enabled accounts");
                return NotFound();
            }
            Log.Debug($"Returned {result.Count} accounts");
            return Ok(result);
        }

        // GET api/<AccountController>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            string message;
            if (id <= 0)
            {
                message = $"The Id can not be 0 when getting an {nameof(Account)}";
                Log.Warning(message);
                return BadRequest(message);
            }

            try
            {
                var data = await _accountService.GetAccountAsync(id);
                if (data != null)
                {
                    return Ok(_mapper.Map<AccountDTO>(data));
                }
                message = $"Could not find an {nameof(Account)} with Id: {id}";
                Log.Warning(message);
                return NotFound(message);

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        // PUT api/<AccountController>/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] AccountDTO account)
        {
            string message;
            if (id <= 0)
            {
                message = "The Id can not be 0 or lower when updating an account";
                Log.Warning(message);
                return BadRequest(message);
            }

            var validator = new AccountDTOValidator();
            ValidationResult results = await validator.ValidateAsync(account);

            if (!results.IsValid)
            {
                Log.Error(results.Errors.ToString(), "Validation failed:");
                return BadRequest(results.Errors);
            }


            // Save the account in the DB
            try
            {
                account.Id = id;
                var accountDB = await _accountService.UpdateAccountAsync(_mapper.Map<Account>(account));

                if (accountDB != null)
                {
                    return Ok(_mapper.Map<AccountDTO>(accountDB));
                }
                message = $"Could not find a {nameof(PlexServer)} with Id: {id}";
                Log.Warning(message);
                return NotFound(message);

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        // POST api/<AccountController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] AccountDTO newAccount)
        {
            var validator = new AccountDTOValidator();
            ValidationResult results = await validator.ValidateAsync(newAccount);

            if (!results.IsValid)
            {
                return BadRequest(results.Errors);
            }

            // Save the account in the DB
            try
            {
                var accountDB = await _accountService.CreateAccountAsync(_mapper.Map<Account>(newAccount));

                if (accountDB != null)
                {
                    var message = $"Account with id {accountDB.Id} was created and/or retrieved successfully";
                    Log.Information(message);
                    return Created(message, _mapper.Map<AccountDTO>(accountDB));
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
            bool result = await _accountService.RemoveAccountAsync(id);
            var message = $"Successfully deleted account with id: {id}";
            if (!result)
            {
                message = $"Could not find account with id: {id} to delete";
                Log.Warning(message);
                return NotFound(message);
            }
            Log.Debug(message);
            return Ok(message);
        }

        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Validate([FromBody] AccountDTO account)
        {
            var validator = new AccountDTOValidator();
            ValidationResult results = await validator.ValidateAsync(account);

            if (!results.IsValid)
            {
                string message = $"The account failed to validate, {results.Errors}";
                Log.Error(message);
                return BadRequest(results.Errors);
            }

            bool isValid = await _accountService.ValidateAccountAsync(account.Username, account.Password);
            if (isValid)
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

        [HttpGet("check/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(bool))]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                string message = "Ensure that an username is send with the request";
                Log.Error(message);
                return BadRequest(message);
            }

            bool available = await _accountService.CheckIfUsernameIsAvailableAsync(username);
            if (available)
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
    }
}
