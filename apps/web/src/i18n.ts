import { createInstance } from 'i18next';
import { initReactI18next } from 'react-i18next';
import ekavian from './locales/sr-Latn-RS-ekavsk/common.json';
import ijekavian from './locales/sr-Latn-RS-ijekavsk/common.json';
import english from './locales/en-US/common.json';

export const locales = ['sr-Latn-RS-ekavsk', 'sr-Latn-RS-ijekavsk', 'en-US'] as const;
export type Locale = typeof locales[number];
export function resolveInitialLocale(language: string): Locale {
  if (locales.includes(language as Locale)) return language as Locale;
  return /^sr(?:-|$)/i.test(language) ? 'sr-Latn-RS-ekavsk' : 'en-US';
}
export function formattingLocale(locale: string): string {
  return locale.startsWith('sr-') ? 'sr-Latn-RS' : 'en-US';
}
export async function createI18n(locale: Locale) {
  const instance = createInstance();
  await instance.use(initReactI18next).init({
    resources: { 'sr-Latn-RS-ekavsk': { common: ekavian },
      'sr-Latn-RS-ijekavsk': { common: ijekavian }, 'en-US': { common: english } },
    lng: locale, supportedLngs: [...locales], fallbackLng: 'en-US',
    load: 'currentOnly', defaultNS: 'common', keySeparator: false,
    interpolation: { escapeValue: false },
  });
  return instance;
}
