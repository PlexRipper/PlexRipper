using System.Text.Json;
using Logging;
using Logging.Interface;
using PlexRipper.Domain.Config;
using Quartz;

namespace Application.Contracts;

public static class JobDataMapExtensions
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(JobDataMapExtensions));

    public static List<int> GetIntListValue(this JobDataMap dataMap, string parameterName)
    {
        try
        {
            var serializedIds = dataMap.GetString(parameterName);
            if (serializedIds is null)
            {
                _log.WarningLine("No {ParameterName} found in job data map", parameterName);
                return [];
            }

            return JsonSerializer.Deserialize<List<int>>(serializedIds, DefaultJsonSerializerOptions.ConfigStandard)
                ?? [];
        }
        catch (Exception e)
        {
            _log.Error(e);
        }

        return [];
    }
}
