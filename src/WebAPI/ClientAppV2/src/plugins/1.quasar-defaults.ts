import { defineNuxtPlugin } from '#app';
import { ComponentConstructor, QSelect, QToggle } from 'quasar';

export default defineNuxtPlugin(() => {
	setQuasarComponentDefaultPropValues(QSelect, {
		outlined: true,
		dense: true,
		dropdownIcon: 'mdi-chevron-down',
		clearIcon: 'mdi-close',
	});

	setQuasarComponentDefaultPropValues(QToggle, {
		color: 'red',
		checkedIcon: 'mdi-check',
		uncheckedIcon: 'mdi-close',
	});
});

// Source: https://github.com/quasarframework/quasar/discussions/8761#discussioncomment-1042529
export const setQuasarComponentDefaultPropValues = <T extends ComponentConstructor>(
	component: T,
	propDefaults: {
		[K in keyof Partial<ExtractComponentProps<T>>]: ExtractComponentProps<T>[K];
	},
) => {
	for (const key in propDefaults) {
		const prop = component.props[key];
		switch (typeof prop) {
			case 'object':
				prop.default = propDefaults[key];
				break;
			case 'function':
				component.props[key] = {
					type: prop,
					default: propDefaults[key],
				};
				break;
			case 'undefined':
				throw new Error('unknown prop: ' + key);
			default:
				throw new Error('unhandled type: ' + typeof prop);
		}
	}
};

type ExtractComponentProps<T> = T extends ComponentConstructor<infer X> ? X['$props'] : never;
const setDefault = <T extends ComponentConstructor, K extends keyof ExtractComponentProps<T>>(
	component: T,
	key: K & string,
	value: ExtractComponentProps<T>[K],
) => {
	const prop = component.props[key];
	switch (typeof prop) {
		case 'object':
			prop.default = value;
			break;
		case 'function':
			component.props[key] = {
				type: prop,
				default: value,
			};
			break;
		case 'undefined':
			throw new Error('unknown prop: ' + key);
		default:
			throw new Error('unhandled type: ' + typeof prop);
	}
};
