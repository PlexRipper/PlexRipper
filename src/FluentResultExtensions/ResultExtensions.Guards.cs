using FluentValidation.Results;

// ReSharper disable once CheckNamespace
// Needs to be in the same namespace as the FluentResults package
namespace FluentResults;

public static partial class ResultExtensions
{
    #region General

    /// <summary>
    /// Add the list of errors to the first error found as a nested error list.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result AddNestedErrors(this Result result, List<IError> errors)
    {
        if (result.Errors.Any())
            result.Errors.First().Reasons.AddRange(errors);

        return result;
    }

    public static Result ToFluentResult(this ValidationResult validationResult)
    {
        if (validationResult is null)
            return Result.Fail("Validation result was null");

        if (validationResult.IsValid)
            return Result.Ok();

        var error = new Error("Validation Error");
        foreach (var validationError in validationResult.Errors)
            error.Reasons.Add(
                new Error(validationError.ErrorMessage).WithMetadata(
                    validationError.ErrorCode,
                    validationError
                )
            );

        return Result.Fail(error);
    }

    #endregion

    #region Guards

    public static Result IsNull(string parameterName) =>
        Create400BadRequestResult($"The {parameterName} parameter is null.");

    public static Result IsEmpty(string parameterName) =>
        Create400BadRequestResult($"The {parameterName} parameter was empty");

    public static Result IsEmptyQueryResult(string queryName, string entityName, int entityId) =>
        Create204NoContentResult(
            $"The {queryName} returned an empty result with {entityName} id: {entityId}"
        );

    public static Result<T> IsEmptyQueryResult<T>(
        T value,
        string queryName,
        string entityName,
        int entityId
    ) =>
        Create204NoContentResult(
            value,
            $"The {queryName} returned an empty result with {entityName} id: {entityId}"
        );

    public static Result IsInvalidId(string parameterName, int value = 0) =>
        Create400BadRequestResult($"The {parameterName} parameter has an invalid id of {value}");

    public static Result IsInvalidId(string parameterName, Guid value) =>
        Create400BadRequestResult($"The {parameterName} parameter has an invalid id of {value}");

    /// <summary>
    /// Creates a new Result with a 404NotFound error set.
    /// </summary>
    /// <param name="entityType"> The name of the entity, e.g. nameof(EntityType).</param>
    /// <param name="id">The entityId which failed to be found.</param>
    /// <returns>A <see cref="Result">Result.Fail()</see> with a 404 Error.</returns>
    public static Result EntityNotFound(string entityType, int id) =>
        Create404NotFoundResult($"The entity of type {entityType} with id {id} could not be found");

    public static Result EntityNotFound(string entityType, Guid guid) =>
        Create404NotFoundResult(
            $"The entity of type {entityType} with id {guid} could not be found"
        );

    #endregion

    #region Check

    public static bool HasException(this Result result) => result.HasError<ExceptionalError>();

    #endregion
}
