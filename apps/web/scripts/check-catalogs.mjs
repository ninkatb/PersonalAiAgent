import assert from 'node:assert/strict';
import fs from 'node:fs';
const locales = ['sr-Latn-RS-ekavsk', 'sr-Latn-RS-ijekavsk', 'en-US'];
const catalogs = locales.map(locale => JSON.parse(fs.readFileSync(new URL(`../src/locales/${locale}/common.json`, import.meta.url))));
const suffix = /_(zero|one|two|few|many|other)$/;
const semanticKeys = catalog => [...new Set(Object.keys(catalog).map(key => key.replace(suffix, '')))].sort();
const placeholders = value => [...value.matchAll(/\{\{(\w+)\}\}/g)].map(match => match[1]).sort();
for (const [index, catalog] of catalogs.entries()) {
  assert.deepEqual(semanticKeys(catalog), semanticKeys(catalogs[0]));
  for (const [key, value] of Object.entries(catalog)) {
    assert.equal(typeof value, 'string');
    assert(value.trim());
    assert(!/<[^>]+>/.test(value));
    const base = key.replace(suffix, '');
    const reference = catalogs[0][key] ?? catalogs[0][base + '_other'];
    assert.deepEqual(placeholders(value), placeholders(reference));
  }
  const culture = index === 2 ? 'en-US' : 'sr-Latn-RS';
  const categories = new Intl.PluralRules(culture).resolvedOptions().pluralCategories.sort();
  const actual = Object.keys(catalog).filter(key => key.startsWith('app.items_')).map(key => key.split('_').at(-1)).sort();
  assert.deepEqual(actual, categories);
}
console.log(`PASS: ${locales.length} catalogs; ${semanticKeys(catalogs[0]).length} semantic keys; placeholders and CLDR`);
