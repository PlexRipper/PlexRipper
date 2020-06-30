using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Source: https://medium.com/the-cloud-builders-guild/validation-without-exceptions-using-a-mediatr-pipeline-behavior-278f124836dc
    /// </summary>
    public class ValidationResponse
    {
        private readonly IList<string> _errorMessages;

        public ValidationResponse(IList<string> errors = null)
        {
            _errorMessages = errors ?? new List<string>();
        }

        public bool IsValidResponse => !_errorMessages.Any();

        public IReadOnlyCollection<string> Errors => new ReadOnlyCollection<string>(_errorMessages);
    }

    /// <summary>
    /// Source: https://medium.com/the-cloud-builders-guild/validation-without-exceptions-using-a-mediatr-pipeline-behavior-278f124836dc
    /// </summary>
    public class ValidationResponse<TModel> : ValidationResponse
        where TModel : class
    {

        public ValidationResponse(TModel model, IList<string> validationErrors = null)
            : base(validationErrors)
        {
            Data = model;
        }

        public TModel Data { get; }
    }
}
