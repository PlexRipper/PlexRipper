using System.Text.Json;
using Quartz;
using Reaparr.Domain;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application.Contracts;

public static class JobDataMapExtensions
{
    private static readonly ILogger _log = LogFactory.GetLogger(typeof(JobDataMapExtensions));

    public static List<int> GetIntListValue(this JobDataMap dataMap, string parameterName)
    {
        try
        {
            var serializedIds = dataMap.GetString(parameterName);
            if (serializedIds is null)
            {
                _log.Here().Warning("No {ParameterName} found in job data map", parameterName);
                return [];
            }

            return JsonSerializer.Deserialize<List<int>>(serializedIds, DefaultJsonSerializerOptions.ConfigStandard)
                ?? [];
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }

        return [];
    }
}
