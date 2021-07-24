<template>
	<page>
		<p-section>
			<template #header> {{ $t('pages.settings.accounts.header') }} </template>
			<account-overview />
		</p-section>
	</page>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Accounts extends Vue {}
</script>
