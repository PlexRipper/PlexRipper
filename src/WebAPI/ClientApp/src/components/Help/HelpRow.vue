<template>
	<q-markup-table dark>
		<tbody>
			<tr>
				<td style="width: 30%">
					<HelpIcon :help-id="helpId" />
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
