import ButtonType from '@enums/buttonType';

export interface IBaseButtonProps extends Record<string, any> {
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
	/**
	 * Makes a circle shaped button
	 */
	round: boolean;
	/**
	 * Applies a more prominent border-radius for a squared shape button
	 */
	rounded: boolean;
	square: boolean;
	outline: boolean;
	loading: boolean;
	disabled: boolean;
	push: boolean;
	// Vuetify Legacy
	block: boolean;
	// Unorganized
	iconSize: number;
	color: 'default' | 'positive' | 'warning' | 'negative';
	href: string;
	to: string;
}
