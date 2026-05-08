export type FlexDslPrimitive = string | number | boolean | Date | null | undefined;

export type FlexDslOperator
	= | 'eq'
		| 'neq'
		| 'gt'
		| 'gte'
		| 'lt'
		| 'lte'
		| 'contains'
		| 'startswith'
		| 'endswith'
		| 'like'
		| 'isnull'
		| 'isnotnull'
		| 'in'
		| 'notin'
		| 'between';

export interface IFlexDslCondition {
	field: string;
	operator: FlexDslOperator;
	value?: FlexDslPrimitive | readonly FlexDslPrimitive[];
}

export interface IFlexSortDescriptor {
	field: string;
	direction?: 'asc' | 'desc';
}

const toDateString = (value: Date): string => value.toISOString();

const normalizeScalar = (value: FlexDslPrimitive): string => {
	if (value === null) {
		return 'null';
	}

	if (value === undefined) {
		return '';
	}

	if (value instanceof Date) {
		return toDateString(value);
	}

	return String(value);
};

const encodeScalar = (value: FlexDslPrimitive): string => encodeURIComponent(normalizeScalar(value));

const encodeList = (values: readonly FlexDslPrimitive[]): string => values.map((x) => encodeScalar(x)).join(',');

const canHaveEmptyValue = (operator: FlexDslOperator): boolean => operator === 'isnull' || operator === 'isnotnull';

const buildCondition = ({ field, operator, value }: IFlexDslCondition): string | null => {
	if (!field) {
		return null;
	}

	if (operator === 'isnull' || operator === 'isnotnull') {
		return `${field}:${operator}`;
	}

	if (operator === 'between') {
		if (!Array.isArray(value) || value.length < 2) {
			return null;
		}

		const [from, to] = value;
		if (from === undefined || to === undefined) {
			return null;
		}

		return `${field}:${operator}:${encodeScalar(from)},${encodeScalar(to)}`;
	}

	if (operator === 'in' || operator === 'notin') {
		if (!Array.isArray(value) || value.length === 0) {
			return null;
		}

		return `${field}:${operator}:${encodeList(value)}`;
	}

	if (value === undefined || value === null) {
		return canHaveEmptyValue(operator) ? `${field}:${operator}` : null;
	}

	if (Array.isArray(value)) {
		if (value.length === 0) {
			return null;
		}

		return `${field}:${operator}:${encodeList(value)}`;
	}

	return `${field}:${operator}:${encodeScalar(value as FlexDslPrimitive)}`;
};

export class FlexFilterDslBuilder {
	private readonly _conditions: IFlexDslCondition[] = [];

	public where(field: string, operator: FlexDslOperator, value?: IFlexDslCondition['value']): this {
		this._conditions.push({ field, operator, value });
		return this;
	}

	public eq(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'eq', value);
	}

	public neq(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'neq', value);
	}

	public gt(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'gt', value);
	}

	public gte(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'gte', value);
	}

	public lt(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'lt', value);
	}

	public lte(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'lte', value);
	}

	public contains(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'contains', value);
	}

	public startsWith(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'startswith', value);
	}

	public endsWith(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'endswith', value);
	}

	public like(field: string, value: FlexDslPrimitive): this {
		return this.where(field, 'like', value);
	}

	public in(field: string, values: readonly FlexDslPrimitive[]): this {
		return this.where(field, 'in', values);
	}

	public notIn(field: string, values: readonly FlexDslPrimitive[]): this {
		return this.where(field, 'notin', values);
	}

	public between(field: string, from: FlexDslPrimitive, to: FlexDslPrimitive): this {
		return this.where(field, 'between', [from, to]);
	}

	public isNull(field: string): this {
		return this.where(field, 'isnull');
	}

	public isNotNull(field: string): this {
		return this.where(field, 'isnotnull');
	}

	public append(conditions: readonly IFlexDslCondition[]): this {
		for (const condition of conditions) {
			this._conditions.push(condition);
		}

		return this;
	}

	public when(value: unknown, callback: (builder: FlexFilterDslBuilder) => void): this {
		if (value) {
			callback(this);
		}

		return this;
	}

	public build(): string | undefined {
		const tokens = this._conditions
			.map((condition) => buildCondition(condition))
			.filter((token): token is string => Boolean(token));

		if (!tokens.length) {
			return undefined;
		}

		return tokens.join(',');
	}
}

export const DSLBuilder = (): FlexFilterDslBuilder => new FlexFilterDslBuilder();

export const buildFlexSortDsl = (sorts: readonly IFlexSortDescriptor[]): string | undefined => {
	const tokens = sorts
		.filter((x) => x.field)
		.map((x) => `${x.field}:${x.direction ?? 'asc'}`);

	if (!tokens.length) {
		return undefined;
	}

	return tokens.join(',');
};
