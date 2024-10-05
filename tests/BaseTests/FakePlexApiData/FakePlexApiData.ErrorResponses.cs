using FluentResultExtensions;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    #region Methods

    #region Public

    public static PlexErrorsResponseDTO GetFailedServerResourceResponse()
    {
        var response = GetErrorResponse();
        response.Errors.Add(
            new PlexErrorDTO()
            {
                Code = 1001,
                Message = "User could not be authenticated",
                Status = HttpCodes.Status401Unauthorized,
            }
        );
        return response;
    }

    public static PlexErrorsResponseDTO GetFailedInvalidVerificationCode()
    {
        var response = GetErrorResponse();
        response.Errors.Add(
            new PlexErrorDTO()
            {
                Code = PlexErrorCodes.InvalidVerificationCode,
                Message = "Invalid verification code.",
                Status = HttpCodes.Status401Unauthorized,
            }
        );
        return response;
    }

    public static PlexErrorsResponseDTO GetFailedEnterVerificationCode()
    {
        var response = GetErrorResponse();
        response.Errors.Add(
            new PlexErrorDTO()
            {
                Code = PlexErrorCodes.EnterVerificationCode,
                Message = "Please enter the verification code",
                Status = HttpCodes.Status401Unauthorized,
            }
        );
        return response;
    }

    #endregion

    #region Private

    private static PlexErrorsResponseDTO GetErrorResponse()
    {
        return new PlexErrorsResponseDTO() { Errors = new List<PlexErrorDTO>() };
    }

    #endregion

    #endregion
}
