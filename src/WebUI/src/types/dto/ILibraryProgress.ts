export default interface ILibraryProgress {
	id: number;
	percentage: number;
	received: number;
	total: number;
	isRefreshing: boolean;
	isComplete: boolean;
}
