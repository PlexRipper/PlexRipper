using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlexRipper.Domain.Validation
{
    /// <summary>
    /// Source: https://medium.com/the-cloud-builders-guild/validation-without-exceptions-using-a-mediatr-pipeline-behavior-278f124836dc
    /// </summary>
    public class ValidateableResponse
    {
        private readonly IList<string> _errorMessages;

        public ValidateableResponse(IList<string> errors = null)
        {
            _errorMessages = errors ?? new List<string>();
        }

        public bool IsValidResponse => !_errorMessages.Any();

        public IReadOnlyCollection<string> Errors => new ReadOnlyCollection<string>(_errorMessages);
    }

    /// <summary>
    /// Source: https://medium.com/the-cloud-builders-guild/validation-without-exceptions-using-a-mediatr-pipeline-behavior-278f124836dc
    /// </summary>
    public class ValidateableResponse<TModel> : ValidateableResponse
        where TModel : class
    {

        public ValidateableResponse(TModel model, IList<string> validationErrors = null)
            : base(validationErrors)
        {
            Data = model;
        }

        public TModel Data { get; }
    }
}
