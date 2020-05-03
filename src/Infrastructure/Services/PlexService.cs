using AutoMapper;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.Enums;
using PlexRipper.Infrastructure.Common.Interfaces;
using PlexRipper.Infrastructure.Common.Models.Plex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    public class PlexService : IPlexService
    {

        #region Private Fields

        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string PlexServersUrl = "https://plex.tv/pms/servers.xml";
        private const string PlexSignInUrl = "https://plex.tv/users/sign_in.json";
        private static readonly HttpClient client = new HttpClient();
        private readonly IPlexApi _plexApi;
        private readonly IPlexRipperDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlexService> _logger;

        #endregion Private Fields

        #region Public Constructors

        public PlexService(IPlexRipperDbContext context, IMapper mapper, IPlexApi plexApi, ILogger<PlexService> logger)
        {
            _plexApi = plexApi;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            client.Timeout = new TimeSpan(0, 0, 0, 30);

        }

        #endregion Public Constructors

        #region Private Methods

        private HttpRequestMessage CreatePlexRequest(Account account, string url, HttpMethod httpMethod, string content = "", ContentType contentType = ContentType.Json)
        {
            if (account.Username.IsNullOrEmpty() || account.Password.IsNullOrEmpty())
            {
                _logger.LogDebug("Username or password was empty");
                return null;
            }

            // Convert to Base64 encoded string
            string base64AuthInfo = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{account.Username}:{account.Password}"));

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = httpMethod,
            };

            request.Headers.Add("X-Plex-Version", "1.1.0");
            request.Headers.Add("X-Plex-Product", "Saverr");
            request.Headers.Add("X-Plex-Client-Identifier", "271938");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            request.Content = new StringContent(content, Encoding.UTF8, $"application/{contentType.ToString().ToLower()}");

            return request;
        }

        private async Task<HttpResponseMessage> RequestSignInAsync(Account account)
        {
            var request = CreatePlexRequest(account, PlexSignInUrl, HttpMethod.Post);
            return await client.SendAsync(request);
        }

        private async Task<PlexAccountDTO> RequestPlexSignInDataAsync(Account account)
        {
            if (account.Username.IsNullOrEmpty() || account.Password.IsNullOrEmpty())
            {
                _logger.LogError("Either the username or password was empty in RequestPlexSignInDataAsync()", account);
                return null;
            }

            var response = await RequestSignInAsync(account);
            // Send request to Plex servers
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject o = JObject.Parse(responseBody);
                if (o.ContainsKey("user"))
                {
                    return JsonConvert.DeserializeObject<PlexAccountDTO>(o["user"].ToString());
                }
                _logger.LogError("The Plex Api returned an invalid Json object lacking key user", responseBody);
                return null;
            }

            _logger.LogError("The Plex Api response was unsuccessful", response);
            return null;
        }


        #endregion Private Methods
        #region Public Methods

        public PlexAccount AddOrUpdatePlexAccount(PlexAccountDTO plexAccountDto)
        {
            if (plexAccountDto == null)
            {
                _logger.LogError($"PlexAccountDTO given as a parameter in {nameof(AddOrUpdatePlexAccount)} was null.");
                return null;
            }

            PlexAccount plexAccount = _mapper.Map<PlexAccount>(plexAccountDto);
            var result = _context.PlexAccounts.Find(plexAccount.Id);
            if (result != null)
            {
                _logger.LogDebug($"PlexAccount with Id: {result.Id} already exists, will update now");
                // Update
                result = plexAccount;
                result.ConfirmedAt = DateTime.Now;
                _context.SaveChangesAsync();
                return result;

            }

            // Add
            _logger.LogDebug($"PlexAccount with Id: {plexAccount.Id} does not yet exist, will add now");
            plexAccount.ConfirmedAt = DateTime.Now;
            _context.PlexAccounts.Add(plexAccount);
            _context.SaveChangesAsync();
            return plexAccount;
        }

        public PlexAccount GetPlexAccount(long plexAccountId)
        {
            return _context.PlexAccounts.FirstOrDefault(x => x.Id == plexAccountId);
        }

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> associated with this <see cref="Account"/>
        /// </summary>
        /// <param name="account">The <see cref="Account"/> to use</param>
        /// <returns>Can return null when invalid</returns>
        public PlexAccount ConvertToPlexAccount(Account account)
        {
            if (!account.IsConfirmed)
            {
                _logger.LogWarning(
                    $"The account with Id: {account.Id} has not yet been confirmed." +
                           $" Confirm first before using ConvertToPlexAccount()");
                return null;
            }

            account = _context
                   .Accounts
                   .Include(x => x.PlexAccount)
                   .FirstOrDefault(x => x.Id == account.Id);
            return account?.PlexAccount;
        }

        public async Task<string> GetPlexToken(Account account)
        {
            var plexAccount = ConvertToPlexAccount(account);

            if (plexAccount == null)
            {
                _logger.LogWarning($"plexAccount result, converted from account with Id: {account.Id}, was null");
                return string.Empty;
            }

            if (plexAccount.AuthToken != string.Empty)
            {
                // TODO Make the token refresh limit configurable 
                if ((plexAccount.ConfirmedAt - DateTime.Now).TotalDays < 30)
                {
                    _logger.LogInformation("Plex AuthToken was still valid, using from local DB.");
                    return plexAccount.AuthToken;
                }
                _logger.LogInformation("Plex AuthToken has expired, refreshing Plex AuthToken now.");
                return await _plexApi.RefreshPlexAuthTokenAsync(account);
            }

            _logger.LogError($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!");
            return string.Empty;
        }

        public async Task<List<string>> GetServers(Account account)
        {
            var token = await GetPlexToken(account);

            if (!string.IsNullOrEmpty(token))
            {
                string serversUrl = $"{PlexServersUrl}?X-Plex-Token={token}";

                var request = CreatePlexRequest(account, serversUrl, HttpMethod.Get, "", ContentType.Xml);
                var response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("response: ", responseString);
            }


            return new List<string>();
        }

        /// <summary>
        /// Check the validity of <see cref="Account"/> credentials to the Plex API. 
        /// </summary>
        /// <param name="account">The Account to be validated</param>
        /// <returns>The PlexAccount in DB that is returned from the Plex API</returns>
        public async Task<PlexAccount> IsAccountValid(Account account)
        {
            var plexAccountDto = await RequestPlexSignInDataAsync(account);
            if (plexAccountDto != null)
            {
                return AddOrUpdatePlexAccount(plexAccountDto);
            }
            // TODO Add error logging here
            return null;
        }
        #endregion Public Methods

    }
}
