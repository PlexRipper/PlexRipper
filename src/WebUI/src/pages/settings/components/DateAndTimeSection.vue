<template>
	<p-section>
		<v-row>
			<v-col>
				<h1>Date and Time</h1>
				<v-divider />
			</v-col>
		</v-row>
		<v-row>
			<v-simple-table>
				<tbody>
					<tr>
						<td>Short Date Format:</td>
						<td><v-combobox filled outlined solo></v-combobox></td>
					</tr>
					<tr>
						<td>Long Date Format:</td>
						<td><v-combobox filled outlined solo></v-combobox></td>
					</tr>
					<tr>
						<td>Time Format:</td>
						<td><v-combobox filled outlined solo></v-combobox></td>
					</tr>
					<tr>
						<td>Show Relative Dates:</td>
						<td>
							<p-check-box v-model="showRelativeDates" />
						</td>
					</tr>
				</tbody>
			</v-simple-table>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Section from '@components/General/Section.vue';

@Component<DateAndTimeSection>({
	components: {
		Section,
	},
})
export default class DateAndTimeSection extends Vue {
	showRelativeDates: boolean = false;
}
</script>
