using FluentResults;

namespace PlexRipper.Domain
{
    public static partial class ResultExtensions
    {
        #region Implementations

        #region General

        private static Result _IsNull(string parameterName = "")
        {
            return Result.Fail(new Error($"The {parameterName} parameter is null."));
        }

        private static Result _IsEmpty(string parameterName)
        {
            return Result.Fail(new Error($"The {parameterName} parameter was empty"));
        }

        private static Result _IsInvalidId(string parameterName, int value = 0)
        {
            return Result.Fail(new Error($"The {parameterName} parameter contained an invalid id of {value}"));
        }

        private static Result _EntityNotFound(string entityType, int id)
        {
            return Result.Fail(Get404NotFoundError($"The entity of type {entityType} with id {id} could not be found"));
        }

        #endregion

        #endregion

        #region Result Signatures

        #region General

        public static Result IsNull(this Result result, string parameterName = "")
        {
            return _IsNull(parameterName);
        }


        public static Result IsInvalidId(this Result result, string parameterName, int value = 0)
        {
            return _IsInvalidId(parameterName, value);
        }

        public static Result IsEmpty(this Result result, string parameterName)
        {
            return _IsEmpty(parameterName);
        }

        public static Result EntityNotFound(this Result result, string entityType, int id)
        {
            return _EntityNotFound(entityType, id);
        }

        #endregion

        #region With New Result

        public static Result IsNull(string parameterName = "")
        {
            return _IsNull(parameterName);
        }

        public static Result IsEmpty(string parameterName)
        {
            return _IsEmpty(parameterName);
        }

        public static Result IsInvalidId(string parameterName, int value = 0)
        {
            return _IsInvalidId(parameterName, value);
        }

        /// <summary>
        /// Creates a new Result with a 404NotFound error set and will also log as a warning.
        /// </summary>
        /// <param name="entityType"> The name of the entity, e.g. nameof(EntityType)</param>
        /// <param name="entityId">The entityId which failed to be found</param>
        /// <returns>A Result.Fail() with a 404 Error</returns>
        public static Result GetEntityNotFound(string entityType, int entityId)
        {
            return _EntityNotFound(entityType, entityId);
        }

        #endregion

        #endregion

        #region Result<T> Signatures

        #region General

        public static Result<T> IsNull<T>(this Result<T> result, string parameterName = "")
        {
            return Result.Fail(new Error($"The {parameterName} parameter is null."));
        }

        public static Result<T> IsNull<T>(string parameterName = "")
        {
            return _IsNull(parameterName);
        }

        public static Result<T> IsInvalidId<T>(this Result<T> result, string parameterName, int value = 0)
        {
            return Result.Fail(new Error($"The {parameterName} parameter contained an invalid id of {value}"));
        }

        public static Result<T> IsInvalidId<T>(string parameterName, int value = 0)
        {
            return new Result().IsInvalidId(parameterName, value);
        }

        public static Result<T> IsEmpty<T>(this Result<T> result, string parameterName)
        {
            return _IsEmpty(parameterName);
        }

        public static Result<T> IsEmpty<T>(string parameterName)
        {
            return IsEmpty(parameterName);
        }

        #endregion

        #endregion
    }
}