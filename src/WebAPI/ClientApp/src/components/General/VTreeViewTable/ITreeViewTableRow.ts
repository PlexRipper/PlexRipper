export default interface ITreeViewTableRow {
	id: number;
	key: string;
	title: string;
	childCount: number;
	children?: ITreeViewTableRow[];
}
