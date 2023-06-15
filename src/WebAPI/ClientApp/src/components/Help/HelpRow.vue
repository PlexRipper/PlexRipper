<template>
	<q-markup-table dark>
		<tbody>
			<tr>
				<td style="width: 30%">
					<help-icon :help-id="helpId" />
				</td>
				<td>
					<slot />
				</td>
			</tr>
		</tbody>
	</q-markup-table>
</template>

<script setup lang="ts">
defineProps<{
	helpId: string;
}>();
</script>
