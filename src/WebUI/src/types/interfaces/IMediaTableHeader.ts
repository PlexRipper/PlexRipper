import { DataTableHeader } from 'vuetify/types';

export default interface IMediaTableHeader<T extends any = any> extends DataTableHeader<T> {
	type?: string;
}
