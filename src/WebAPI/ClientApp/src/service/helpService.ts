import { Observable, of } from 'rxjs';
import IStoreState from '@interfaces/service/IStoreState';
import { switchMap } from 'rxjs/operators';
import { BaseService } from '@service';

export class HelpService extends BaseService {
	public constructor() {
		super({
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					helpIdDialog: state.helpIdDialog,
				};
			},
		});
	}

	public getHelpDialog(): Observable<string> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.helpIdDialog ?? '')));
	}

	public openHelpDialog(helpId: string): void {
		this.setState({ helpIdDialog: helpId }, 'Open Help Dialog with helpId: ' + helpId);
	}
}

const helpService = new HelpService();
export default helpService;
