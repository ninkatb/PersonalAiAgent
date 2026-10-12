import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { formattingLocale, locales } from './i18n';

const pages = ['overview', 'chat', 'mail', 'calendar', 'documents', 'tasks', 'memory', 'settings'] as const;
type Page = typeof pages[number];
const marks: Record<Page, string> = { overview: '◫', chat: '◇', mail: '✉', calendar: '▦', documents: '▤', tasks: '✓', memory: '○', settings: '⚙' };
const localeLabels = ['settings.languageEkavsk', 'settings.languageIjekavsk', 'settings.languageEnglish'];

export function App() {
  const { t, i18n } = useTranslation();
  const [page, setPage] = useState<Page>('overview');
  useEffect(() => {
    document.title = t('app.title');
    document.documentElement.lang = i18n.language;
  }, [i18n.language, t]);
  const culture = formattingLocale(i18n.language);
  return <div className="app-shell">
    <a className="skip-link" href="#content">{t('common.skipMain')}</a>
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark" aria-hidden="true">✳</span><div><strong>{t('app.title')}</strong><p>{t('app.subtitle')}</p></div></div>
      <nav aria-label={t('app.navigation')}>
        {pages.map(item => <button key={item} aria-current={page === item ? 'page' : undefined} onClick={() => setPage(item)}><span aria-hidden="true">{marks[item]}</span>{t('nav.' + item)}</button>)}
      </nav>
      <div className="language-picker">
        <label htmlFor="locale">{t('settings.uiLanguage')}</label>
        <select id="locale" value={i18n.language} onChange={event => { void i18n.changeLanguage(event.target.value); }}>
          {locales.map((locale, index) => <option key={locale} value={locale}>{t(localeLabels[index]!)}</option>)}
        </select>
        <p>{t('app.languageTemporary')}</p>
      </div>
    </aside>
    <main id="content" tabIndex={-1}>
      <header><p>{t('nav.' + page)}</p><span className="locked" role="status"><span aria-hidden="true">●</span>{t('vault.locked')}</span></header>
      <section className="page-heading"><p className="eyebrow">{t('app.subtitle')}</p><h1>{page === 'overview' ? t('app.welcome') : t('nav.' + page)}</h1><p>{page === 'overview' ? t('app.intro') : t('common.empty')}</p></section>
      {page === 'settings' ? <section className="card settings-card"><h2>{t('settings.uiLanguage')}</h2><p>{t('settings.latinScript')}</p><p>{t('app.languageTemporary')}</p></section> : <>
        <section className="card empty-card"><div className="empty-mark" aria-hidden="true">{marks[page]}</div><h2>{t('app.emptyTitle')}</h2><p>{t('app.emptyDescription')}</p><span>{t('app.items', { count: 0 })}</span></section>
        <div className="details"><section className="card"><h2>{t('app.today')}</h2><time dateTime={new Date().toISOString()}>{new Intl.DateTimeFormat(culture, { dateStyle: 'long' }).format(new Date())}</time></section><section className="card"><h2>{t('vault.locked')}</h2><p>{t('app.lockNotice')}</p></section></div>
      </>}
    </main>
  </div>;
}
