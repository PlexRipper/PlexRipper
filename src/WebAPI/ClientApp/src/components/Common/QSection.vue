<template>
	<QRow
		no-gutters
		style="width: 100%">
		<QCol cols="12">
			<!-- Header	-->
			<QRow no-gutters>
				<QCol>
					<QText
						size="h5"
						class="q-my-sm q-ml-md"
						align="left"
						bold="medium">
						<slot name="header">
							{{ header }}
						</slot>
					</QText>
					<q-separator />
				</QCol>
			</QRow>
			<!--	Section Content	-->
			<slot />
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
	header?: string;
}>(), {
	header: '',
});
</script>
