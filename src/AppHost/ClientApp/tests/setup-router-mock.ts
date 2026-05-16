import { beforeEach, vi } from 'vitest';

let query: Record<string, string | number | undefined> = {};

vi.mock('@vueuse/router', () => ({
	useRouteQuery: <T extends string | number>(name: string, defaultValue: T) => ({
		get value() {
			return query[name] ?? defaultValue;
		},
		set value(value: T | undefined) {
			if (value === undefined) {
				delete query[name];
				return;
			}

			query[name] = value;
		},
	}),
}));

beforeEach(() => {
	query = {};
});
