export type VaultState = 'Locked' | 'Unlocked';
export type ResponseLanguage = 'follow-user' | 'sr-Latn-RS-ekavsk' | 'sr-Latn-RS-ijekavsk' | 'en-US';
export interface HealthResult { status: 'ok' }
export interface BackendTransport { health(): Promise<HealthResult> }
// Only the fixed health operation is exposed; no credential reaches this layer.
