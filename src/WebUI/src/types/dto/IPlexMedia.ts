export default interface IPlexMedia {
	id: number;
	ratingKey: number;
	key: string;
	guid: any | undefined;
	studio: any | undefined;
	title: any | undefined;
	contentRating: any | undefined;
	summary: any | undefined;
	index: number;
	rating: number;
	year: number;
	thumb: any | undefined;
	art: any | undefined;
	banner: any | undefined;
	duration: number;
	originallyAvailableAt: Date;
	leafCount: number;
	viewedLeafCount: number;
	childCount: number;
	addedAt: Date;
	updatedAt: Date;
	viewCount: any | undefined;
	lastViewedAt: any | undefined;
	theme: any | undefined;
	plexLibraryId: number;
}
