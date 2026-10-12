// Provjera dokumentacionih artefakata; nije aplikacioni test niti WCAG audit.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const copy = JSON.parse(fs.readFileSync(path.join(__dirname, 'ux-copy.json'), 'utf8'));
const tokens = JSON.parse(fs.readFileSync(path.join(__dirname, 'ux-tokens.json'), 'utf8'));
const spec = fs.readFileSync(path.join(__dirname, 'ux-design.md'), 'utf8');
const locales = ['sr-Latn-RS-ekavsk', 'sr-Latn-RS-ijekavsk', 'en-US'];
const keys = Object.keys(copy.catalogs[locales[0]]).sort();
assert.deepEqual(Object.keys(copy.catalogs).sort(), [...locales].sort());
const placeholders = value => [...new Set([...value.matchAll(/\{\{([a-zA-Z][a-zA-Z0-9]*)\}\}/g)].map(match => match[1]))].sort();
for (const locale of locales) {
  assert.deepEqual(Object.keys(copy.catalogs[locale]).sort(), keys, locale);
  assert.equal(copy.formattingLocales[locale], locale === 'en-US' ? 'en-US' : 'sr-Latn-RS');
  for (const key of keys) {
    const value = copy.catalogs[locale][key];
    const reference = copy.catalogs[locales[0]][key];
    assert.equal(typeof value, typeof reference, key);
    const variants = typeof value === 'string' ? [value] : Object.values(value);
    if (typeof value !== 'string') {
      assert.deepEqual(Object.keys(value).sort(), new Intl.PluralRules(copy.formattingLocales[locale]).resolvedOptions().pluralCategories.sort(), locale + ':' + key);
    }
    const base = typeof reference === 'string' ? reference : reference.other;
    for (const text of variants) {
      assert.equal(typeof text, 'string');
      assert(text.trim().length > 0, key);
      assert(!/<[^>]+>/.test(text), key);
      assert.deepEqual(placeholders(text), placeholders(base), locale + ':' + key);
      if (locale.startsWith('sr-')) assert(!/[\u0400-\u04ff]/u.test(text), locale + ':' + key);
      if (locale.endsWith('ijekavsk')) assert(!/\b(?:nijesam|nijesi|nijesmo|nijeste|nijesu|sjutra)\b/u.test(text), key);
    }
  }
}
const luminance = hex => {
  const rgb = [1,3,5].map(start => parseInt(hex.slice(start, start + 2), 16) / 255);
  const linear = rgb.map(value => value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4);
  return linear[0] * 0.2126 + linear[1] * 0.7152 + linear[2] * 0.0722;
};
const contrasts = [];
for (const [theme, palette] of Object.entries(tokens.themes)) {
  for (const color of Object.values(palette)) assert(/^#[0-9A-F]{6}$/i.test(color));
  for (const pair of tokens.contrastPairs) {
    assert(Object.hasOwn(palette, pair.foreground) && Object.hasOwn(palette, pair.background));
    const values = [luminance(palette[pair.foreground]), luminance(palette[pair.background])].sort((a,b) => b-a);
    const ratio = (values[0] + 0.05) / (values[1] + 0.05);
    assert(ratio >= pair.minimum, `${theme}:${pair.foreground}/${pair.background} ${ratio}`);
    contrasts.push({theme, pair: pair.foreground + '/' + pair.background, ratio: Number(ratio.toFixed(2)), minimum: pair.minimum});
  }
}
for (const [prefix, count] of [['FR',28], ['NFR',12]]) {
  const rows = [...spec.matchAll(new RegExp('^\\| (' + prefix + '-\\d{3}) \\|', 'gm'))].map(match => match[1]);
  assert.deepEqual(rows.sort(), Array.from({length:count}, (_,i) => prefix + '-' + String(i+1).padStart(3,'0')));
}
for (const match of spec.matchAll(/`((?:common|nav|settings|vault|onboarding|models|cloud|integrations|chat|mail|actions|calendar|tasks|documents|memory|digest|notifications|backup|diagnostics|startup|release)\.[A-Za-z][A-Za-z0-9]*)`/g)) {
  assert(keys.includes(match[1]), 'Nedostaje ključ: ' + match[1]);
}
for (const file of ['ux-design.md', '../../README.md', 'prd.md', 'architecture.md', 'gate-check.md', '../codex-plan/DECISIONS.md', '../codex-plan/I18N_SPEC.md', '../architecture/ADR-001_LOCALIZATION.md', '../architecture/ADR-005_THEME_PERSISTENCE.md']) {
  const contents = fs.readFileSync(path.join(__dirname, file), 'utf8');
  for (const match of contents.matchAll(/\]\(([^)]+)\)/g)) {
    const target = match[1].split('#')[0];
    if (!target || /^[a-z]+:/i.test(target)) continue;
    assert(fs.existsSync(path.resolve(path.dirname(path.join(__dirname,file)), target)), 'Nevažeći link: ' + target);
  }
}
const formatting = locales.map(locale => {
  const culture = copy.formattingLocales[locale];
  const time = new Intl.DateTimeFormat(culture, {dateStyle:'medium',timeStyle:'short',timeZone:'Europe/Belgrade'}).format(new Date('2026-10-12T07:05:00Z'));
  const number = new Intl.NumberFormat(culture, {minimumFractionDigits:2}).format(12345.67);
  const plural = new Intl.PluralRules(culture);
  const counts = [0,1,2,5,11,21,22,101,1.5].map(count => {
    const category = plural.select(count);
    const value = copy.catalogs[locale]['digest.items'][category];
    assert(value, locale + ':' + category);
    return value.replace('{{count}}', new Intl.NumberFormat(culture).format(count));
  });
  return {locale, culture, time, number, counts};
});
assert.equal(formatting[0].time, formatting[1].time);
assert.equal(formatting[0].number, formatting[1].number);
console.log(JSON.stringify({result:'PASS',catalogs:locales.length,semanticKeys:keys.length,contrastPairs:contrasts.length,fr:28,nfr:12,formatting,contrasts}, null, 2));
