import { describe, expect, test } from 'vitest';
import { DownloadActions, DownloadStatus, type DownloadProgressDTO } from '@dto';
import { toDownloadTreeNodes } from '@/components/Views/Downloads/downloadTreeNodes';

describe('toDownloadTreeNodes()', () => {
	test('Does not add a label field to PrimeVue tree nodes', () => {
		// Arrange
		const downloads: DownloadProgressDTO[] = [
			{
				id: 'season-1',
				title: 'Season 1',
				mediaType: 'Season',
				status: DownloadStatus.Completed,
				percentage: 100,
				dataReceived: 1024,
				dataTotal: 1024,
				downloadSpeed: 0,
				timeRemaining: 0,
				children: [
					{
						id: 'episode-1',
						title: 'Episode 1',
						mediaType: 'Episode',
						status: DownloadStatus.Completed,
						percentage: 100,
						dataReceived: 512,
						dataTotal: 512,
						downloadSpeed: 0,
						timeRemaining: 0,
						children: [],
					},
				],
			},
		];

		// Act
		const [node] = toDownloadTreeNodes(downloads, []);

		// Assert
		expect(node).toMatchObject({
			id: 'season-1',
			key: 'season-1',
			title: 'Season 1',
			children: [
				{
					id: 'episode-1',
					key: 'episode-1',
					title: 'Episode 1',
				},
			],
		});
		expect(node.actions.some((action) => action.type === DownloadActions.Details)).toBe(true);
		expect('label' in node).toBe(false);
		expect('label' in node.children![0]).toBe(false);
	});
});
