import {Observable, of} from 'rxjs';
import {map, take} from 'rxjs/operators';

import {BaseService} from '@service';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';
import {JobStatusUpdateDTO, JobTypes} from '@dto/mainApi';

export class BackgroundJobsService extends BaseService {
    public constructor() {
        super('BackgroundJobsService', {
            // Note: Each service file can only have "unique" state slices which are not also used in other service files
            stateSliceSelector: (state: IStoreState) => {
                return {
                    jobStatus: state.jobStatus,
                };
            },
        });
    }

    public setup() {
        super.setup();
        return of({name: this._name, isSuccess: true}).pipe(take(1));
    }

    // region Update
    public setStatusJobUpdate(update: JobStatusUpdateDTO): void {
        this.updateStore('jobStatus', update);
    }

    // endregion

    public getJobs(jobType: JobTypes): Observable<JobStatusUpdateDTO> {
        return this.getStateChanged<JobStatusUpdateDTO[]>('jobStatus').pipe(
            map((jobStatusList) => jobStatusList.filter((jobStatus) => jobStatus.jobType === jobType)),
            map(
                (jobStatusList) =>
                    jobStatusList.sort(function compare(a, b) {
                        const dateA = new Date(a.jobStartTime);
                        const dateB = new Date(b.jobStartTime);
                        // @ts-ignore
                        return dateA - dateB;
                    })[0],
            ),
            // distinctUntilChanged(isEqual),
        );
    }
}

const backgroundJobsService = new BackgroundJobsService();
export default backgroundJobsService;
