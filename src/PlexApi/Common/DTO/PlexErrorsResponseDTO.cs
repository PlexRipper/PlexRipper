namespace PlexRipper.PlexApi;

public class PlexErrorsResponseDTO
{
    #region Properties

    public List<PlexErrorDTO> Errors { get; set; } = new();

    #endregion

    #region Methods

    #region Public

    public List<PlexError> ToResultErrors()
    {
        return Errors.Select(x => new PlexError(x.Message)
            {
                Code = x.Code,
                Status = x.Status,
            })
            .ToList();
    }

    #endregion

    #endregion
}