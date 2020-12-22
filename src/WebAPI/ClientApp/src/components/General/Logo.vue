<template>
	<v-img :height="size" :width="size" :src="require('@img/logo/full-logo-256.png')" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class Logo extends Vue {
	@Prop({ required: true, type: Number, default: 128 })
	readonly size!: number;
}
</script>
