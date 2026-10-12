import type { BackendTransport, HealthResult } from './contracts';
import { invoke } from '@tauri-apps/api/core';

export const backendTransport: BackendTransport = {
  async health(): Promise<HealthResult> {
    try {
      const result: unknown = await invoke('backend_health', {});
      if (typeof result !== 'object' || result === null ||
          Object.keys(result).length !== 1 || !('status' in result) || result.status !== 'ok') {
        throw new Error('BACKEND_UNAVAILABLE');
      }
      return { status: 'ok' };
    } catch {
      throw new Error('BACKEND_UNAVAILABLE');
    }
  },
};
