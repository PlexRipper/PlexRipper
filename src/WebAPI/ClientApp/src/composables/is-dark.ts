import { useQuasar } from '#imports';

export function isDark(): boolean {
	return useQuasar().dark.isActive;
}
