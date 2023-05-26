<template>
	<page-container>
		<download-manager-section />
		<database-section />
		<setup-section />
	</page-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';

@Component
export default class AdvancedSettings extends Vue {}
</script>
