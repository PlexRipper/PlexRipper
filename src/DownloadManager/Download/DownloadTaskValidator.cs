using Data.Contracts;
using FluentValidation;
using PlexRipper.Application;

namespace PlexRipper.DownloadManager;

public class DownloadTaskValidator : IDownloadTaskValidator
{
    #region Fields

    private readonly IMediator _mediator;

    private readonly INotificationsService _notificationsService;

    #endregion

    #region Constructor

    public DownloadTaskValidator(IMediator mediator, INotificationsService notificationsService)
    {
        _mediator = mediator;
        _notificationsService = notificationsService;
    }

    #endregion

    #region Public Methods

    public async Task<Result<List<DownloadTask>>> CheckIfDownloadTasksExist(List<DownloadTask> downloadTasks)
    {
        if (!downloadTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

        var downloadTasksDb = await _mediator.Send(new GetAllDownloadTasksQuery());
        if (downloadTasksDb.IsFailed)
            return downloadTasksDb.ToResult();

        var existingDownloadTasks = new List<DownloadTask>();
        var flattenedDownloadTaskDb = downloadTasksDb.Value.Flatten(x => x.Children).ToList();
        var errors = new List<Error>();
        foreach (var downloadTask in downloadTasks)
            if (flattenedDownloadTaskDb.Any(x =>
                    x.PlexServerId == downloadTask.PlexServerId &&
                    x.PlexLibraryId == downloadTask.PlexLibraryId &&
                    x.Key == downloadTask.Key))
            {
                existingDownloadTasks.Add(downloadTask);
                errors.Add(new Error($"DownloadTask {downloadTask.FullTitle} is already added to the download list"));
            }

        // All download tasks are already added
        if (existingDownloadTasks.Count == downloadTasks.Count)
            return Result.Fail("All downloadTasks already exist and cannot be added again").LogError();

        // Some failed, alert front-end of some failing
        if (existingDownloadTasks.Any())
        {
            var result = Result.Fail("The following DownloadTasks already exist").WithErrors(errors);
            await _notificationsService.SendResult(result);
            return Result.Ok(downloadTasks.Except(existingDownloadTasks).ToList());
        }

        return Result.Ok(downloadTasks);
    }

    public Result<List<DownloadTask>> ValidateDownloadTasks(List<DownloadTask> downloadTasks)
    {
        if (!downloadTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

        var validator = new AddDownloadTaskValidator();

        var failedList = new List<DownloadTask>();
        var result = Result.Fail("Failed to add the following DownloadTasks");

        for (var i = 0; i < downloadTasks.Count; i++)
        {
            var downloadTask = downloadTasks[i];

            // Check validity
            Log.Debug($"Validating DownloadTask {i + 1} of {downloadTasks.Count} with title {downloadTask.FullTitle}");
            var validationResult = validator.Validate(downloadTask).ToFluentResult();
            if (validationResult.IsFailed)
            {
                validationResult.Errors.ForEach(x => x.Metadata.Add("downloadTask Title", downloadTask.FullTitle));
                validationResult.LogError();
                failedList.Add(downloadTask);
                result.AddNestedErrors(validationResult.Errors);
            }
            else
                Log.Information($"DownloadTask {i + 1} of {downloadTasks.Count} with title {downloadTask.FullTitle} was valid");
        }

        // All download tasks failed validation
        if (failedList.Count == downloadTasks.Count)
            return result;

        // Some failed, alert front-end of some failing
        if (failedList.Any())
        {
            _notificationsService.SendResult(result);
            return Result.Ok(downloadTasks.Except(failedList).ToList());
        }

        return Result.Ok(downloadTasks);
    }

    /// <summary>
    /// Validates the <see cref="DownloadTask"/>s and returns only the valid one while notifying of any failed ones.
    /// Returns only a failed result when all downloadTasks failed validation.
    /// </summary>
    /// <param name="downloadTasks">The <see cref="DownloadTask"/>s to validate.</param>
    /// <returns>Only the valid <see cref="DownloadTask"/>s.</returns>
    public async Task<Result<List<DownloadTask>>> VerifyDownloadTasks(List<DownloadTask> downloadTasks)
    {
        if (!downloadTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

        var validateResult = ValidateDownloadTasks(downloadTasks);
        if (validateResult.IsFailed)
        {
            Log.Debug($"All {downloadTasks.Count} download tasks failed validation");
            return validateResult.ToResult();
        }

        var existResult = await CheckIfDownloadTasksExist(validateResult.Value);
        if (existResult.IsFailed)
        {
            Log.Debug($"All {downloadTasks.Count} download tasks are already added to the download list");
            return existResult.ToResult();
        }

        // Return those that passed
        return Result.Ok(existResult.Value);
    }

    #endregion

    private class AddDownloadTaskValidator : AbstractValidator<DownloadTask>
    {
        #region Constructor

        public AddDownloadTaskValidator()
        {
            RuleFor(y => y).NotNull();
            RuleFor(y => y.DataReceived).GreaterThanOrEqualTo(0).LessThanOrEqualTo(y => y.DataTotal);
            RuleFor(y => y.DataTotal).GreaterThan(0);
            RuleFor(y => y.Key).GreaterThan(0);
            RuleFor(y => y.MediaType).NotEqual(PlexMediaType.None).NotEqual(PlexMediaType.Unknown);

            RuleFor(y => y.Title).NotEmpty();

            RuleFor(y => y.DownloadDirectory).NotEmpty();
            RuleFor(y => y.DestinationDirectory).NotEmpty();

            RuleFor(y => y.FullTitle).NotEmpty();

            RuleFor(y => y.PlexServerId).GreaterThan(0);
            RuleFor(y => y.PlexLibraryId).GreaterThan(0);

            RuleFor(y => y.DownloadFolderId).GreaterThan(0);
            RuleFor(y => y.DestinationFolderId).GreaterThan(0);

            When(c => c.IsDownloadable, () =>
            {
                RuleFor(y => y.PlexServerId).GreaterThan(0);
                RuleFor(y => y.PlexLibraryId).GreaterThan(0);

                RuleFor(y => y.DownloadFolderId).GreaterThan(0);
                RuleFor(y => y.DestinationFolderId).GreaterThan(0);

                RuleFor(y => y.FileName).NotEmpty();

                RuleFor(y => y.FileLocationUrl).NotEmpty();
                RuleFor(y => y.DownloadUrl).NotEmpty();
                RuleFor(y => y.DownloadUri).NotNull();
                RuleFor(y => y.DownloadUri.IsAbsoluteUri)
                    .NotNull()
                    .When(y => y.DownloadUri != null);

                RuleFor(x => Uri.IsWellFormedUriString(x.DownloadUri.AbsoluteUri, UriKind.Absolute))
                    .NotEqual(false)
                    .When(y => y.DownloadUri != null);
                RuleFor(y => y.Created).NotEqual(DateTime.MinValue);
            });
        }

        #endregion
    }
}