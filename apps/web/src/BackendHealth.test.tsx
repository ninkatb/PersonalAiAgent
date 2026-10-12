import { afterEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { I18nextProvider } from 'react-i18next';
import { createI18n, locales } from './i18n';
import { BackendHealth } from './BackendHealth';
import { backendTransport } from './transport';

declare global { interface Window { __TAURI_INTERNALS__?: { invoke(command: string, args: unknown): Promise<unknown> } } }

const events = vi.hoisted(() => ({ callback: () => {}, unlisten: vi.fn() }));
vi.mock('@tauri-apps/api/event', () => ({
  listen: vi.fn((_name: string, callback: () => void) => { events.callback = callback; return Promise.resolve(events.unlisten); }),
}));

afterEach(() => { cleanup(); delete window.__TAURI_INTERNALS__; });
describe('typed health bridge', () => {
  for (const locale of locales) {
    it(`rejects a stale success after lifecycle revocation in ${locale}`, async () => {
      const i18n = await createI18n(locale);
      let resolve!: (value: unknown) => void;
      window.__TAURI_INTERNALS__ = { invoke: vi.fn().mockImplementation(() => new Promise(done => { resolve = done; })) };
      render(<I18nextProvider i18n={i18n}><BackendHealth /></I18nextProvider>);
      await userEvent.click(screen.getByRole('button'));
      act(() => { events.callback(); });
      expect(await screen.findByText(i18n.t('health.failed'))).toBeTruthy();
      await act(async () => { resolve({ status: 'ok' }); });
      expect(screen.queryByText(i18n.t('health.ok'))).toBeNull();
    });
    it(`shows success, progress and safe errors in ${locale}`, async () => {
      const i18n = await createI18n(locale);
      let resolve!: (value: unknown) => void;
      const invoke = vi.fn().mockImplementationOnce(() => new Promise(done => { resolve = done; }))
        .mockRejectedValueOnce('SYNTHETIC_PRIVATE_ERROR');
      window.__TAURI_INTERNALS__ = { invoke };
      render(<I18nextProvider i18n={i18n}><BackendHealth /></I18nextProvider>);
      expect(screen.getByText(i18n.t('health.idle'))).toBeTruthy();
      await userEvent.click(screen.getByRole('button'));
      expect(screen.getByText(i18n.t('health.checking'))).toBeTruthy();
      expect((screen.getByRole('button') as HTMLButtonElement).disabled).toBe(true);
      resolve({ status: 'ok' });
      expect(await screen.findByText(i18n.t('health.ok'))).toBeTruthy();
      expect(invoke).toHaveBeenCalledWith('backend_health', {}, undefined);
      await userEvent.click(screen.getByRole('button'));
      expect(await screen.findByText(i18n.t('health.failed'))).toBeTruthy();
      expect(document.body.textContent).not.toContain('SYNTHETIC');
    });
  }
  it('keeps the result and retranslates it when the locale changes', async () => {
    const i18n = await createI18n('sr-Latn-RS-ekavsk');
    window.__TAURI_INTERNALS__ = { invoke: vi.fn().mockResolvedValue({ status: 'ok' }) };
    render(<I18nextProvider i18n={i18n}><BackendHealth /></I18nextProvider>);
    await userEvent.click(screen.getByRole('button'));
    expect(await screen.findByText(i18n.t('health.ok'))).toBeTruthy();
    await i18n.changeLanguage('en-US');
    expect(await screen.findByText('The local service is available.')).toBeTruthy();
  });
  it('fails safely outside the desktop', async () => {
    await expect(backendTransport.health()).rejects.toThrow('BACKEND_UNAVAILABLE');
  });
  it('removes the native subscription and ignores a late event after unmount', async () => {
    const i18n = await createI18n('en-US');
    const view = render(<I18nextProvider i18n={i18n}><BackendHealth /></I18nextProvider>);
    await act(async () => { await Promise.resolve(); });
    events.unlisten.mockClear();
    view.unmount();
    await act(async () => { events.callback(); await Promise.resolve(); });
    expect(events.unlisten).toHaveBeenCalledOnce();
  });
  it.each([null, {}, { status: 'unknown' }, { status: 'ok', token: 'SYNTHETIC' }, 'SYNTHETIC', undefined])('rejects unexpected IPC results %j', async result => {
      window.__TAURI_INTERNALS__ = { invoke: vi.fn().mockResolvedValue(result) };
      await expect(backendTransport.health()).rejects.toThrow('BACKEND_UNAVAILABLE');
    });
});
