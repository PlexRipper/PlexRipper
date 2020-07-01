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
        public bool HasValidationErrors { get; set; }
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

        public ValidationResponse(TModel model, string errorMsg) : base(new List<string> { errorMsg })
        {
            Data = model;
        }

        public TModel Data { get; }

        /// <summary>
        /// Change the Data type of <see cref="ValidationResponse{TModel}"/> and copy any errors
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="response">The response containing the errors</param>
        /// <returns>A new <see cref="ValidationResponse{T}"/> with any errors</returns>
        public ValidationResponse<T> ConvertTo<T>(ValidationResponse response = null) where T : class
        {
            var errorList = new List<string>();
            if (response == null)
            {
                errorList = response.Errors.ToList();
            }
            return new ValidationResponse(errorList) as ValidationResponse<T>;
        }

    }
}
