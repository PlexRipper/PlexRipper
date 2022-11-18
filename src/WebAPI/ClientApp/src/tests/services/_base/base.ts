import { Context } from '@nuxt/types';
import Log, { LogLevel } from 'consola';
import MockAdapter from 'axios-mock-adapter';
import Axios from 'axios-observable';
import { MockConfig } from '@mock';

export * from '@hirez_io/observer-spy';

export function baseVars(): { ctx: Context; mock: MockAdapter; config: MockConfig } {
	let ctx, mock;
	return {
		ctx,
		mock,
		config: {},
	};
}

export function baseSetup(): { ctx: Context } {
	const ctx: Context = {
		$config: {
			nodeEnv: 'TESTING',
			version: '1.0',
		},
	} as Context;
	process.env.NODE_ENV = 'dev';
	process.client = true;
	// Minimum LogLevel displayed
	Log.level = LogLevel.Debug;
	return {
		ctx,
	};
}

export function getAxiosMock() {
	// @ts-ignore
	return new MockAdapter(Axios, { onNoMatch: 'throwException' });
}
