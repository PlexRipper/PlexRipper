import type { IError, PlexAccountDTO } from '@dto';

export interface IPlexAccount extends PlexAccountDTO {
	isInputValid: boolean;
	hasValidationErrors: boolean;
	validationErrors: IError[];
}
