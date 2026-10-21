import { useElementVisibility } from '@vueuse/core';
import type { MaybeRefOrGetter } from 'vue';
import { PhotoAssetKind, PhotoItemKind, type PhotoAssetBrowseDTO } from '@dto';
import { usePhotoBrowsingStore } from '@store';
import type { PhotoOverviewItem } from '@/store/photoBrowsingStore';

type VisibilityTarget = Parameters<typeof useElementVisibility>[0];

export function usePhotoImagePreloadCandidate(
	target: VisibilityTarget,
	item: MaybeRefOrGetter<PhotoOverviewItem | null | undefined>,
): void {
	const photoBrowsingStore = usePhotoBrowsingStore();
	const owner = {};
	const visible = useElementVisibility(target);
	const candidate = computed<PhotoAssetBrowseDTO | null>(() => {
		const overviewItem = toValue(item);
		if (overviewItem?.kind !== PhotoItemKind.Asset || overviewItem.asset.assetKind !== PhotoAssetKind.Image)
			return null;
		return overviewItem.asset;
	});
	let registeredAsset: PhotoAssetBrowseDTO | null = null;

	function unregister(): void {
		if (registeredAsset)
			photoBrowsingStore.setImagePreloadCandidate(registeredAsset, false, owner);
		registeredAsset = null;
	}

	watch([candidate, visible], ([asset, isVisible]) => {
		unregister();
		if (!asset || !isVisible)
			return;
		photoBrowsingStore.setImagePreloadCandidate(asset, true, owner);
		registeredAsset = asset;
	}, { immediate: true });

	onUnmounted(unregister);
}
