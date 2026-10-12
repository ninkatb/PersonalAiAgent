import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
const root = new URL('../src/', import.meta.url);
for (const entry of fs.readdirSync(root, { recursive: true })) {
  if (!/\.tsx?$/.test(entry) || entry.endsWith('.test.tsx') || entry.endsWith('.test.ts')) continue;
  const contents = fs.readFileSync(path.join(root.pathname, entry), 'utf8');
  assert(!/\b(localStorage|sessionStorage|fetch|XMLHttpRequest|eval)\b/.test(contents), entry);
  assert(!/from ['"](?:@tauri-apps\/plugin-|node:|openai|@anthropic-ai\/sdk|better-sqlite3)/.test(contents), entry);
  assert(!/dangerouslySetInnerHTML/.test(contents), entry);
}
console.log('PASS: renderer boundary; no direct HTTP, storage, shell or provider SDK');
