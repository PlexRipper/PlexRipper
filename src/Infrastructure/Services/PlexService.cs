using AutoMapper;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.ValueObjects;
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
        private readonly IPlexRipperDbContext _context;
        private readonly IMapper _mapper;

        #endregion Private Fields

        #region Public Constructors

        public PlexService(IPlexRipperDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            client.Timeout = new TimeSpan(0, 0, 0, 30);
        }

        #endregion Public Constructors

        #region Private Methods

        private HttpRequestMessage CreatePlexRequest(Account account, string url, HttpMethod httpMethod)
        {
            if (account.Username.IsNullOrEmpty() || account.Password.IsNullOrEmpty())
            {
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
                // TODO Add error logging here
                return null;
            }

            // TODO Add Error logging here
            return null;
        }

        /// <summary>
        /// Returns a new AuthToken and will update the <see cref="PlexAccount"/> in the DB.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private async Task<string> RefreshPlexAuthTokenAsync(Account account)
        {
            var result = await RequestPlexSignInDataAsync(account);
            if (result != null)
            {
                AddOrUpdatePlexAccount(result);
                return result.AuthToken;
            }
            return string.Empty;
        }

        #endregion Private Methods
        #region Public Methods

        public PlexAccount AddOrUpdatePlexAccount(PlexAccountDTO plexAccountDto)
        {
            if (plexAccountDto == null)
            {
                //TODO Add logging error
                return null;
            }

            PlexAccount plexAccount = _mapper.Map<PlexAccount>(plexAccountDto);
            var result = _context.PlexAccounts.Find(plexAccount.Id);
            if (result != null)
            {
                // Update
                result = plexAccount;
                result.ConfirmedAt = DateTime.Now;
                _context.SaveChangesAsync();
                return result;

            }
            else
            {
                // Add
                plexAccount.ConfirmedAt = DateTime.Now;
                _context.PlexAccounts.Add(plexAccount);
                _context.SaveChangesAsync();
                return plexAccount;
            }
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
                // TODO Add error logging here
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

            //TODO Add logging error
            if (plexAccount == null) return string.Empty;

            if (plexAccount.AuthToken != string.Empty)
            {
                // TODO Make the token refresh limit configurable 
                if ((plexAccount.ConfirmedAt - DateTime.Now).TotalDays < 30)
                {
                    return plexAccount.AuthToken;
                }
                else
                {
                    return await RefreshPlexAuthTokenAsync(account);
                }

            }

            return null;
        }

        public async Task<List<string>> GetServers(Account account)
        {
            var token = await GetPlexToken(account);

            if (!string.IsNullOrEmpty(token))
            {

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
