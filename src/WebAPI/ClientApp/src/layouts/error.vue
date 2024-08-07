<template>
	<q-layout
		view="hHh lpR fFf"
		dark
		class="no-background">
		<q-page-container>
			<QPage>
				<QRow justify="center">
					<QCol cols="auto">
						<QText
							size="h3"
							align="center">
							{{ $t('pages.error.header') }}
						</QText>
						<QText
							size="h3"
							align="left">
							<pre>
								<code class="no-background">{{ errorFormatted }}</code>
							</pre>
						</QText>
					</QCol>
				</QRow>
				<QRow justify="center">
					<QCol cols="auto">
						<QText
							size="h3"
							align="center">
							<NuxtLink to="/">
								{{ $t('pages.error.return-link') }}
							</NuxtLink>
						</QText>
					</QCol>
				</QRow>
			</QPage>
		</q-page-container>
	</q-layout>
</template>

<script setup lang="ts">
const props = defineProps<{ error: Error }>();

const errorFormatted = computed(() => {
	return JSON.stringify(props.error, null, 4);
});
</script>
