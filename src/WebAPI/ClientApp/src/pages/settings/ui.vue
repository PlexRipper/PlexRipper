<template>
	<page-container>
		<date-and-time-section />
		<confirmation-section />
	</page-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import DateAndTimeSection from '~/pages/settings/components/DateAndTimeSection.vue';
import ConfirmationSection from '~/pages/settings/components/ConfirmationSection.vue';

@Component({
	components: {
		DateAndTimeSection,
		ConfirmationSection,
	},
})
export default class UiSettings extends Vue {}
</script>
