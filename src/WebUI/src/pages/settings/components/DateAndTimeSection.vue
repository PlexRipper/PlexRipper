<template>
	<p-section>
		<v-row no-gutters>
			<v-col>
				<h1>Date and Time</h1>
				<v-divider />
			</v-col>
		</v-row>
		<v-row no-gutters>
			<v-col>
				<v-simple-table class="section-table">
					<tbody>
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.date-and-time.short-date-format" />
							</td>
							<td>
								<v-select
									v-model="shortDateSelect"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="shortDateOptions"
								/>
							</td>
						</tr>
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.long-date-format" />
							</td>
							<td>
								<v-select
									v-model="longDateSelect"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="longDateOptions"
								/>
							</td>
						</tr>
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.time-format" />
							</td>
							<td>
								<v-select
									v-model="timeFormatSelect"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="timeFormatOptions"
								/>
							</td>
						</tr>
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.show-relative-dates" />
							</td>
							<td>
								<p-checkbox v-model="showRelativeDates" />
							</td>
						</tr>
					</tbody>
				</v-simple-table>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';

interface ISelectItem {
	text: string | number | object;
	value: string | number | object;
	disabled?: boolean;
	divider?: boolean;
	header?: string;
}

@Component
export default class DateAndTimeSection extends Vue {
	shortDateSelect!: ISelectItem;
	longDateSelect!: ISelectItem;
	timeFormatSelect!: ISelectItem;
	showRelativeDates: boolean = false;

	get getMenuProps(): any {
		return {
			offsetY: true,
			contentClass: 'menu-background',
		};
	}

	get shortDateOptions(): ISelectItem[] {
		const values: string[] = ['MMM dd yyyy', 'dd MMM yyyy', 'MM/dd/yyyy', 'dd/MM/yyyy', 'yyyy-MM-dd'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x),
			});
		});

		return options;
	}

	get longDateOptions(): ISelectItem[] {
		const values: string[] = ['EEEE, MMMM dd, yyyy', 'EEEE, dd MMMM yyyy'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x),
			});
		});

		return options;
	}

	get timeFormatOptions(): ISelectItem[] {
		const values: string[] = ['HH:MM', 'hh:mm b'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x),
			});
		});

		return options;
	}

	created(): void {
		this.shortDateSelect = this.shortDateOptions[0];
		this.longDateSelect = this.longDateOptions[0];
		this.timeFormatSelect = this.timeFormatOptions[0];
	}
}
</script>
