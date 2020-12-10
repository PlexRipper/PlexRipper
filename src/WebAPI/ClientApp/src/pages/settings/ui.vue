<template>
	<page>
		<date-and-time-section />
		<confirmation-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import Page from '@components/General/Page.vue';
import DateAndTimeSection from '~/pages/settings/components/DateAndTimeSection.vue';
import ConfirmationSection from '~/pages/settings/components/ConfirmationSection.vue';

@Component({
	components: {
		Page,
		DateAndTimeSection,
		ConfirmationSection,
	},
})
export default class UiSettings extends Vue {}
</script>
