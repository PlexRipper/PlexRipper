export interface ITvShowSelector {
	id: number;
	selected: boolean;
	seasons: ISeasonSelector[];
}

export interface ISeasonSelector {
	id: number;
	selected: boolean;
	episodes: IEpisodeSelector[];
}

export interface IEpisodeSelector {
	id: number;
	selected: boolean;
}
