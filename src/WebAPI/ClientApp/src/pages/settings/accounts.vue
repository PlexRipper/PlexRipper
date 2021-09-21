<template>
	<page-container>
		<p-section>
			<template #header> {{ $t('pages.settings.accounts.header') }} </template>
			<account-overview />
		</p-section>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Accounts extends Vue {}
</script>
