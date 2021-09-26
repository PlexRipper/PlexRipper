<template>
	<page-container>
		<language-section />
		<date-and-time-section />
		<confirmation-section />
	</page-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
@Component<UiSettings>({})
export default class UiSettings extends Vue {}
</script>
