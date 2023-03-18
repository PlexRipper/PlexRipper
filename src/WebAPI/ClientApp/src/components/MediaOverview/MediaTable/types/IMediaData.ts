import IPart from '@mediaOverview/MediaTable/types/IPart';

export default interface IMediaData {
	id: number;
	mediaFormat: string;
	duration: number;
	videoResolution: string;
	width: number;
	height: number;
	bitrate: number;
	videoCodec: string;
	videoFrameRate: string;
	aspectRatio: number;
	videoProfile: string;
	audioProfile: string;
	audioCodec: string;
	audioChannels: number;
	parts: IPart[];
}
