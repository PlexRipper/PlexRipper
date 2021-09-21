<template>
	<page-container>
		<date-and-time-section />
		<confirmation-section />
	</page-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';

@Component
export default class UiSettings extends Vue {}
</script>
