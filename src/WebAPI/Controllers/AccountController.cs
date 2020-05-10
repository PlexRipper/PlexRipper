using AutoMapper;
using Carter;
using Carter.ModelBinding;
using Carter.Request;
using Carter.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class AccountController : CarterModule
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> Log;

        public AccountController(IAccountService accountService, IMapper mapper, ILogger<AccountController> log) : base("/api")
        {

            _accountService = accountService;
            _mapper = mapper;
            Log = log;
            string path = "/accounts";
            Get(path, GetAll);
            Get(path + "/{id:int}", Get);
            Get(path + "/check/", CheckUsername); // Check if username exists
            Post(path, Post);
            Post(path + "/validate", Validate); // Validate Account
            Delete(path + "/{id:int}", Delete);
        }

        private async Task CheckUsername(HttpRequest req, HttpResponse res)
        {
            string username = req.Query["username"].ToString();

            if (string.IsNullOrEmpty(username))
            {
                res.StatusCode = StatusCodes.Status400BadRequest;
                await res.Negotiate(null);
            }

            var exists = await _accountService.GetAccountAsync(username) != null;
            if (exists)
            {
                res.StatusCode = StatusCodes.Status406NotAcceptable;
                await res.AsJson(new
                {
                    message = $"Account with username: \"{username}\" already exists!"
                });
            }
            else
            {
                res.StatusCode = StatusCodes.Status200OK;
            }
        }

        private async Task Delete(HttpRequest req, HttpResponse res)
        {
            var result = await _accountService.DeleteAccountAsync(req.RouteValues.As<int>("id"));
            res.StatusCode = result ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
        }

        private async Task Post(HttpRequest req, HttpResponse res)
        {
            var result = await req.BindAndValidate<AccountDTO>();

            if (!result.ValidationResult.IsValid)
            {
                res.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await res.Negotiate(result.ValidationResult.GetFormattedErrors());
                return;
            }

            // Save the account in the DB
            try
            {
                var accountDB = await _accountService.AddOrUpdateAccountAsync(_mapper.Map<Account>(result.Data));

                if (accountDB != null)
                {
                    res.StatusCode = StatusCodes.Status201Created;
                    await res.Negotiate(_mapper.Map<AccountDTO>(accountDB));
                }
                else
                {
                    res.StatusCode = StatusCodes.Status400BadRequest;
                    await res.Negotiate(null);
                }
            }
            catch (Exception e)
            {
                Log.LogError("Error:", e);
                res.StatusCode = StatusCodes.Status500InternalServerError;
                await res.Negotiate(null);
            }
        }


        private async Task GetAll(HttpRequest req, HttpResponse res)
        {
            var data = await _accountService.GetAllAccountsAsync();
            await res.AsJson(data);
        }

        private async Task Get(HttpRequest req, HttpResponse res)
        {
            var data = await _accountService.GetAccountAsync(req.RouteValues.As<int>("id"));
            await res.Negotiate(data);
        }

        private async Task Validate(HttpRequest req, HttpResponse res)
        {
            var result = await req.BindAndValidate<AccountDTO>();

            if (!result.ValidationResult.IsValid)
            {
                res.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await res.Negotiate(result.ValidationResult.GetFormattedErrors());
                return;
            }

            var isValid = await _accountService.ValidateAccountAsync(result.Data.Username, result.Data.Password);
            res.StatusCode = isValid ? StatusCodes.Status200OK : StatusCodes.Status401Unauthorized;
            return;
        }
    }
}
