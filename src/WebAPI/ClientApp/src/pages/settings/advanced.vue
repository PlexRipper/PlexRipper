<template>
	<page>
		<download-manager-section />
		<database-section />
		<setup-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';

@Component
export default class AdvancedSettings extends Vue {}
</script>
