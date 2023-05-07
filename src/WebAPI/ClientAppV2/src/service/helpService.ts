import { Observable, of } from 'rxjs';
import { map, switchMap, take } from 'rxjs/operators';

import BaseService from './baseService';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';

export class HelpService extends BaseService {
	public constructor() {
		super('HelpService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					helpIdDialog: state.helpIdDialog,
				};
			},
		});
	}

	setup(): Observable<ISetupResult> {
		super.setup();
		return of({ name: this._name, isSuccess: true }).pipe(take(1));
	}

	public getHelpDialog(): Observable<string> {
		return this.stateChanged.pipe(map((x) => x?.helpIdDialog ?? ''));
	}

	public openHelpDialog(helpId: string): void {
		this.setState({ helpIdDialog: helpId }, 'Open Help Dialog with helpId: ' + helpId);
	}
}

export default new HelpService();
