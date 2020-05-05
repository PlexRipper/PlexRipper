using Carter;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using System.Collections.Generic;

namespace PlexRipper.WebAPI.Controllers
{
    public class PlexServerController : CarterModule
    {
        private readonly IPlexService _plexService;

        public PlexServerController(IPlexService plexService) : base("/api")
        {
            _plexService = plexService;
        }

        // GET: api/PlexServer
        [HttpGet]
        public IEnumerable<string> Get()
        {
            // await _plexService.GetServers();
            return new string[] { "value1", "value2" };
        }

        // GET: api/PlexServer/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PlexServer
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/PlexServer/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
