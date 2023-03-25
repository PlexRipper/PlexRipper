<template>
	<span v-if="size > 0">{{ formattedString }}{{ speed ? $t('general.units.per-second') : '' }}</span>
	<span v-else> - </span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class FileSize extends Vue {
	@Prop({ required: true, type: Number, default: 0 })
	readonly size!: number;

	@Prop({ required: false, type: Boolean, default: false })
	readonly speed!: boolean;

	/*
	 * Filesize filters
	 * Source: https://github.com/sainf/vue-filter-pretty-bytes
	 */
	get formattedString(): string {
		const bytes = this.size;
		const kib = false;
		const decimals = 2;

		if (bytes === 0) return '0 Bytes';
		if (isNaN(bytes) && !isFinite(bytes)) return 'Unknown';
		const k = kib ? 1024 : 1000;
		const dm = !isNaN(decimals) && decimals >= 0 ? decimals : 2;
		const sizes = kib
			? ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB', 'BiB']
			: ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB', 'BB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));

		return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
	}
}
</script>
