using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using PlexRipper.WebUI.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class WeatherForecastController : ApiController
    {
        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await Mediator.Send(new GetWeatherForecastsQuery());
        }
    }
}
