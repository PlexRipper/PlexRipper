// Temporary type definition for TorznabSettingsDTO
// This will be replaced when API types are regenerated from the backend
export interface TorznabSettingsDTO {
  isEnabled: boolean;
  apiKey: string | null;
  maxResultsPerRequest: number;
  enabledServerIds: number[];
  enableWebhookNotifications: boolean;
  webhookUrl: string | null;
  logSearchRequests: boolean;
  autoCreateDownloadTasks: boolean;
  searchTimeoutSeconds: number;
}

// Extend the existing SettingsModelDTO type
declare module '@/types/api/generated/data-contracts' {
  interface SettingsModelDTO {
    torznabSettings: TorznabSettingsDTO;
  }
}