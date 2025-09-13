export interface QExpansionListProps {
	icon?: string;
	title?: string;

	link?: string;
	type?: 'badge' | 'server' | 'library';
	count?: number;
	children?: QExpansionListProps[];
}
