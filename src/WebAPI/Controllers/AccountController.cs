using AutoMapper;
using Carter;
using Carter.ModelBinding;
using Carter.Request;
using Carter.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces;
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

            Get("/accounts", GetAll);
            Get("/accounts/{id:int}", Get);
            Post("/accounts", Post);
            Delete("/accounts/{id:int}", Delete);
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
                var exists = await _accountService.GetAccountAsync(result.Data.Username) != null;
                if (exists)
                {
                    res.StatusCode = StatusCodes.Status403Forbidden;
                    await res.AsJson(new
                    {
                        message = $"Account with username: \"{result.Data.Username}\" already exists!"
                    });
                    return;
                }
                var accountDB = await _accountService.AddAccountAsync(result.Data.Username, result.Data.Password);

                if (accountDB != null)
                {
                    res.StatusCode = StatusCodes.Status201Created;
                    await res.Negotiate(_mapper.Map<AccountDTO>(accountDB));
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

    }
}
