import { orderBy } from 'lodash-es';
import type { BaseResultDTO, PlexServerConnectionDTO } from '@dto';

export function sortPlexServerConnections(connections: PlexServerConnectionDTO[]) {
	const keysOrder: (keyof PlexServerConnectionDTO)[] = ['local', 'isPlexTvConnection'];
	return orderBy(connections, keysOrder, ['asc', 'asc']);
}

export function waitForElement(parentElement: HTMLElement | null, selector: string, intervalMs: number = 100): Promise<HTMLElement | null> {
	return new Promise((resolve) => {
		const timeout = setTimeout(() => {
			clearInterval(checkExist);
			resolve(null); // Resolve with null if timeout
		}, 5000);

		const checkExist = setInterval(() => {
			const element = parentElement?.querySelector(selector) as HTMLElement | null;
			if (element) {
				clearInterval(checkExist);
				clearTimeout(timeout);
				resolve(element);
			}
		}, intervalMs);
	});
}

export function discordInviteLink() {
	return 'https://discord.com/invite/Qa3BtxN77g';
}

export function formatErrorResponse(res: BaseResultDTO | null): string {
	if (!res) {
		return '❌ An unknown error occurred.';
	}

	const header = `❌ Error ${res.statusCode}`;
	const messages = res.errors.map((err) => {
		const reasons
			= err.reasons && err.reasons.length
				? `\n   ↳ Reasons: ${err.reasons.map((r) => r?.message ?? String(r)).join(', ')}`
				: '';
		return `• ${err.message}${reasons}`;
	});
	return [header, ...messages].join('\n');
}
