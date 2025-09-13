import type { NamedColor } from 'quasar';

export interface QIconTooltipData {
	value: number | string;
	icon: string;
	tooltip?: string;
	color?: NamedColor;
}
