using System.Text.Json;
using Quartz;
using Reaparr.Domain;
using Reaparr.Logging;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application.Contracts;

public static class JobDataMapExtensions
{
    private static readonly ILog _log = new LogConfig().CreateLogInstance(typeof(JobDataMapExtensions));

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
