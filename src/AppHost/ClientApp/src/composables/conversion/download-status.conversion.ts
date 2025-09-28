import { DownloadStatus } from '@dto';

export function translateDownloadStatus(status: DownloadStatus) {
	const { $i18n } = useNuxtApp();
	const { t } = $i18n;

	switch (status) {
		case DownloadStatus.Unknown:
			return t('general.download-status.unknown');
		case DownloadStatus.DownloadFinished:
			return t('general.download-status.download-finished');
		case DownloadStatus.Deleted:
			return t('general.download-status.deleted');
		case DownloadStatus.Merging:
			return t('general.download-status.merging');
		case DownloadStatus.Moving:
			return t('general.download-status.moving');
		case DownloadStatus.Completed:
			return t('general.download-status.completed');
		case DownloadStatus.Downloading:
			return t('general.download-status.downloading');
		case DownloadStatus.Error:
			return t('general.download-status.error');
		case DownloadStatus.ServerUnreachable:
			return t('general.download-status.server-unreachable');
		case DownloadStatus.Paused:
			return t('general.download-status.paused');
		case DownloadStatus.Queued:
			return t('general.download-status.queued');
		case DownloadStatus.Stopped:
			return t('general.download-status.stopped');
		case DownloadStatus.MovePaused:
			return t('general.download-status.move-paused');
		case DownloadStatus.MoveFinished:
			return t('general.download-status.move-finished');
		case DownloadStatus.MoveError:
			return t('general.download-status.move-error');
		default:
			return t('general.error.unknown');
	}
}
