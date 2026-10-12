import { afterEach, describe, expect, it } from 'vitest';
import { cleanup, render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { I18nextProvider } from 'react-i18next';
import { App } from './App';
import { createI18n, locales, resolveInitialLocale, formattingLocale } from './i18n';

afterEach(cleanup);
describe('foundation shell', () => {
  for (const locale of locales) {
    it(`starts locked and exposes all navigation in ${locale}`, async () => {
      const i18n = await createI18n(locale);
      render(<I18nextProvider i18n={i18n}><App /></I18nextProvider>);
      expect(screen.getByRole('status').textContent).toContain(i18n.t('vault.locked'));
      expect(within(screen.getByRole('navigation')).getAllByRole('button')).toHaveLength(8);
      const user = userEvent.setup();
      for (const button of within(screen.getByRole('navigation')).getAllByRole('button')) {
        button.focus();
        await user.keyboard('{Enter}');
        expect(button.getAttribute('aria-current')).toBe('page');
        const heading = screen.getByRole('heading', { level: 1 }).textContent;
        expect(heading).toBe(button.textContent?.includes(i18n.t('nav.overview'))
          ? i18n.t('app.welcome') : button.textContent?.slice(1));
      }
      expect(screen.queryByRole('button', { name: i18n.t('vault.unlock') })).toBeNull();
      expect(document.title).toBe(i18n.t('app.title'));
    });
  }
  it('preserves navigation and changes Serbian variant immediately', async () => {
    const user = userEvent.setup();
    const i18n = await createI18n('sr-Latn-RS-ekavsk');
    render(<I18nextProvider i18n={i18n}><App /></I18nextProvider>);
    const button = screen.getByRole('button', { name: 'Dokumenti' });
    button.focus();
    await user.keyboard('{Enter}');
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Dokumenti');
    await user.selectOptions(screen.getByRole('combobox'), 'sr-Latn-RS-ijekavsk');
    expect(screen.getByText('Ovdje još nema razgovora, povezanih naloga ili dokumenata.')).toBeTruthy();
    expect(button.getAttribute('aria-current')).toBe('page');
    await user.selectOptions(screen.getByRole('combobox'), 'en-US');
    expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('Documents');
    expect(screen.getByRole('status').textContent).toContain('The application is locked.');
    expect(document.documentElement.lang).toBe('en-US');
  });
  it.each([['sr', 'sr-Latn-RS-ekavsk'], ['sr-Cyrl-RS', 'sr-Latn-RS-ekavsk'], ['sr-Latn-RS-ijekavsk', 'sr-Latn-RS-ijekavsk'], ['fr-FR', 'en-US'], ['', 'en-US']])('suggests language for %s', (os, expected) => {
    expect(resolveInitialLocale(os)).toBe(expected);
  });
  it('uses the same regional formats for Serbian variants', () => {
    expect(formattingLocale(locales[0])).toBe('sr-Latn-RS');
    expect(formattingLocale(locales[1])).toBe('sr-Latn-RS');
    expect(formattingLocale(locales[2])).toBe('en-US');
    const date = new Date('2026-10-12T07:05:00Z');
    const format = (locale: string) => new Intl.DateTimeFormat(formattingLocale(locale), { dateStyle: 'long', timeZone: 'Europe/Belgrade' }).format(date);
    expect(format(locales[0])).toBe(format(locales[1]));
    expect(new Intl.NumberFormat(formattingLocale(locales[0])).format(12345.67)).toBe(new Intl.NumberFormat(formattingLocale(locales[1])).format(12345.67));
  });
});
