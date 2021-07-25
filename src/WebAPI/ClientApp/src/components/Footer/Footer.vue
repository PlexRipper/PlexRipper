<template>
	<v-footer app>
		<span>&copy; {{ currentYear }} - {{ getServerStatus }}</span>
	</v-footer>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Footer extends Vue {
	status: boolean = false;

	get currentYear(): number {
		return new Date().getFullYear();
	}

	get getServerStatus(): string {
		return this.status ? 'Server is online!' : 'Server is offline';
	}
}
</script>
