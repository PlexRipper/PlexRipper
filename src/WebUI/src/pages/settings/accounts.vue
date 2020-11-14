<template>
	<page>
		<account-overview />
	</page>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import AccountOverview from '@overviews/AccountOverview/AccountOverview.vue';
import AccountCard from '@overviews/AccountOverview/AccountCard.vue';

@Component({
	components: {
		AccountCard,
		AccountOverview,
	},
})
export default class Accounts extends Vue {}
</script>
