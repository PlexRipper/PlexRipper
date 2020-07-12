export default interface IDownloadProgress {
	id: number;
	status: string;
	percentage: number;
	dataReceived: string;
	dataTotal: string;
	downloadSpeed: string;
}
