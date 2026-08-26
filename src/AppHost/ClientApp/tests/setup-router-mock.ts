import { beforeEach, vi } from 'vitest';
import { computed } from 'vue';

let query: Record<string, string | number | undefined> = {};

vi.mock('@vueuse/router', () => ({
	useRouteQuery: <T extends string | number>(name: string, defaultValue: T) => computed<T>({
		get() {
			return (query[name] ?? defaultValue) as T;
		},
		set(value: T | undefined) {
			if (value === undefined) {
				const { [name]: _removed, ...rest } = query;
				query = rest;
				return;
			}

			query[name] = value as string | number;
		},
	}),
}));

beforeEach(() => {
	query = {};
});
