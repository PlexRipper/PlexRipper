import { Context } from 'vm';
import Log from 'consola';
import MockAdapter from 'axios-mock-adapter';
import Axios from 'axios-observable';
import { MockConfig } from '@mock';
import IAppConfig from '@class/IAppConfig';

export * from '@hirez_io/observer-spy';

export function baseVars(): { ctx: Context; mock: MockAdapter; config: Partial<MockConfig>; appConfig: IAppConfig } {
	let ctx, mock;
	return {
		ctx,
		mock,
		config: {},
		appConfig: {} as IAppConfig,
	};
}

export function baseSetup(): { ctx: Context; appConfig: IAppConfig } {
	const ctx: Context = {
		$config: {
			nodeEnv: 'TESTING',
			version: '1.0',
		},
	} as Context;

	const appConfig: IAppConfig = {
		baseUrl: 'http://localhost:3030/',
		nodeEnv: 'TESTING',
		version: '1.0',
		isProduction: false,
	};
	process.env.NODE_ENV = 'dev';
	process.client = true;

	// Minimum LogLevel displayed
	Log.level = 2;
	return {
		ctx,
		appConfig,
	};
}

export function getAxiosMock() {
	// @ts-ignore
	return new MockAdapter(Axios, { onNoMatch: 'throwException' });
}
