using System;
using FluentValidation;

namespace PlexRipper.Domain
{
    public class DownloadTaskValidator : AbstractValidator<DownloadTask>
    {
        public DownloadTaskValidator()
        {
            RuleFor(x => x.DataReceived).Equal(0);
            RuleFor(x => x.DataTotal).GreaterThan(0);
            RuleFor(x => x.Key).GreaterThan(0);
            RuleFor(x => x.MediaType).NotEqual(PlexMediaType.None);
            RuleFor(x => x.MediaType).NotEqual(PlexMediaType.Unknown);

            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.ServerToken).NotEmpty();
            RuleFor(x => x.DownloadUrl).NotEmpty();
            RuleFor(x => x.DownloadPath).NotEmpty();
            RuleFor(x => x.DownloadUri.IsAbsoluteUri).NotNull();
            RuleFor(x => Uri.IsWellFormedUriString(x.DownloadUri.AbsoluteUri, UriKind.Absolute)).NotNull();
            RuleFor(x => x.FileLocationUrl).NotEmpty();
            RuleFor(x => x.Created).NotEqual(DateTime.MinValue);

            RuleFor(x => x.TitleTvShow).NotEmpty().When(x => x.MediaType == PlexMediaType.Episode);
            RuleFor(x => x.TitleTvShowSeason).NotEmpty().When(x => x.MediaType == PlexMediaType.Episode);

            RuleFor(x => x.PlexServerId).GreaterThan(0);
            RuleFor(x => x.PlexServer).NotNull();
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.DownloadFolderId).GreaterThan(0);
            RuleFor(x => x.DownloadFolder).NotNull();
            RuleFor(x => x.DownloadFolder.IsValid()).NotNull();
            RuleFor(x => x.DestinationFolderId).GreaterThan(0);
            RuleFor(x => x.DestinationFolder).NotNull();
            RuleFor(x => x.DestinationFolder.IsValid()).NotNull();
        }
    }
}