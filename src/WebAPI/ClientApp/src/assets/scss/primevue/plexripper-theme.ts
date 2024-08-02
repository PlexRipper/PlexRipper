import { definePreset } from '@primevue/themes';
import Aura from '@primevue/themes/aura';

const plexRipperTheme = definePreset(Aura, {
	options: {
		darkModeSelector: '.body--dark',
	},
	// Colors: https://primevue.org/theming/styled/#colors
	semantic: {
		primary: {
			50: '{red.50}',
			100: '{red.100}',
			200: '{red.200}',
			300: '{red.300}',
			400: '{red.400}',
			500: '{red.500}',
			600: '{red.600}',
			700: '{red.700}',
			800: '{red.800}',
			900: '{red.900}',
			950: '{red.950}',
		},
		colorScheme: {
			light: {
				primary: {
					color: '#ff0000',
				},
				surface: {
					0: '#ffffff',
					50: '{red.50}',
					100: '{red.100}',
					200: '{red.200}',
					300: '{red.300}',
					400: '{red.400}',
					500: '{red.500}',
					600: '{red.600}',
					700: '{red.700}',
					800: '{red.800}',
					900: 'transparent',
					950: 'transparent',
				},
			},
			dark: {
				primary: {
					color: '{red.600}',
					hoverColor: '{red.600}',
				},
				highlight: {
					background: 'rgba(255, 0, 0, 0.47)',
					focusBackground: 'rgba(250, 250, 250, .24)',
					color: 'rgba(255,255,255,.87)',
					focusColor: 'rgba(255,255,255,.87)',
				},
				surface: {
					0: '#ffffff',
					50: '{slate.50}',
					100: '{slate.100}',
					200: '{slate.200}',
					300: '{slate.300}',
					400: '{slate.400}',
					500: '{slate.500}',
					600: '{slate.600}',
					700: '{slate.700}',
					800: 'transparent',
					900: 'transparent',
					950: 'transparent',
				},
			},
		},
	},
	components: {
		include: ['TreeTable', 'Column', 'Checkbox'],
		treetable: {
			colorScheme: {
				light: {
					border: {
						color: '{zinc.400}',
					},
				},
				dark: {
					border: {
						color: '{zinc.400}',
					},
				},
			},
		},
		checkbox: {
			colorScheme: {
				light: {
					border: {
						color: '{neutral.100}',
					},
				},
				dark: {
					border: {
						color: '{neutral.100}',
					},
				},
			},
		},
	},
});

export default {
	preset: plexRipperTheme,
	options: {
		ripple: true,
	},
};
