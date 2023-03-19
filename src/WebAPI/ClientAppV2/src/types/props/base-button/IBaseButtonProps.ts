import { QBtnProps } from './QBtnProps';
import { PlexBtnProps } from './PlexBtnProps';

export interface IBaseButtonProps extends QBtnProps, PlexBtnProps {
	block?: boolean;
	disabled?: boolean;
	iconSize?: number;
	iconOnly?: boolean;
	color?: string;
	lightColor?: string;
	darkColor?: string;
	href?: string;
	to?: string;
	width?: number;
	height?: number;
}
