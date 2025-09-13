import type { PlexMediaQualityDTO } from '@dto';

export interface IMediaActionEmits {
	(e: 'open-media-details'): void;
	(e: 'download', mediaQualities: PlexMediaQualityDTO[]): void;
}
