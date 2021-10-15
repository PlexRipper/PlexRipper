using System;
using System.Linq;
using FluentValidation;

namespace PlexRipper.Domain
{
    public class DownloadTaskValidator : AbstractValidator<DownloadTask>
    {
        public DownloadTaskValidator()
        {
            RuleFor(x => x.DataReceived).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.DataTotal);
            RuleFor(x => x.DataTotal).GreaterThan(0);
            RuleFor(x => x.Key).GreaterThan(0);
            RuleFor(x => x.MediaType)
                .NotEqual(PlexMediaType.None)
                .NotEqual(PlexMediaType.Unknown);

            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.DownloadUrl).NotEmpty();

            RuleFor(x => x.DownloadDirectory).NotEmpty();
            RuleFor(x => x.DestinationDirectory).NotEmpty();

            RuleFor(x => x.DownloadUri).NotNull();
            RuleFor(x => x.DownloadUri.IsAbsoluteUri).NotNull()
                .When(x => x.DownloadUri != null);

            RuleFor(x => Uri.IsWellFormedUriString(x.DownloadUri.AbsoluteUri, UriKind.Absolute)).NotEqual(false).When(x => x.DownloadUri != null);
            RuleFor(x => x.FileLocationUrl).NotEmpty();
            RuleFor(x => x.Created).NotEqual(DateTime.MinValue);

            RuleFor(x => x.FullTitle).NotEmpty();

            RuleFor(x => x.PlexServerId).GreaterThan(0);
            RuleFor(x => x.PlexServer).NotNull();

            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
            RuleFor(x => x.PlexLibrary).NotNull();

            RuleFor(x => x.DownloadFolderId).GreaterThan(0);
            RuleFor(x => x.DownloadFolder).NotNull();
            RuleFor(x => x.DownloadFolder.IsValid()).NotNull()
                .When(x => x.DownloadFolder != null);

            RuleFor(x => x.DestinationFolderId).GreaterThan(0);
            RuleFor(x => x.DestinationFolder).NotNull();
            RuleFor(x => x.DestinationFolder.IsValid()).NotNull()
                .When(x => x.DestinationFolder != null);

            RuleFor(x => x.DownloadWorkerTasks).NotNull();
            RuleFor(x => x.DownloadWorkerTasks.Any()).Equal(true)
                .When(x => x.DownloadWorkerTasks != null);
        }
    }
}