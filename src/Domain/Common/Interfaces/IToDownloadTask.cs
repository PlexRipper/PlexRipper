using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public interface IToDownloadTask
    {
        /// <summary>
        /// A <see cref="PlexMovie"/> can have multiple media parts, which is why we return a list.
        /// </summary>
        /// <returns>The <see cref="DownloadTask"/>s created from all media parts.</returns>
        List<DownloadTask> CreateDownloadTasks();
    }
}