import ButtonType from '@enums/buttonType';

export interface IBaseButtonProps {
	// PlexRipper
	cy: string;
	type: ButtonType;
	textId: string;
	tooltipId: string;
	width: number;
	height: number;
	iconAlign: 'left' | 'right';
	vertical: boolean;
	// Quasar native
	label: string;
	icon: string;
	size: string | 'xs' | 'sm' | 'md' | 'lg' | 'xl';
	flat: boolean;
	glossy: boolean;
	round: boolean;
	rounded: boolean;
	outline: boolean;
	loading: boolean;
	disabled: boolean;
	push: boolean;
	// Vuetify Legacy
	block: boolean;
	iconOnly: boolean;
	// Unorganized
	iconSize: number;
	color: 'default' | 'positive' | 'warning' | 'negative';
	href: string;
	to: string;
}
