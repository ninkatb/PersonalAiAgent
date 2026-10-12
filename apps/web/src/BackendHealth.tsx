import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { listen } from '@tauri-apps/api/event';
import { backendTransport } from './transport';

export function BackendHealth() {
  const { t } = useTranslation();
  const [state, setState] = useState<'idle' | 'checking' | 'ok' | 'failed'>('idle');
  const epoch = useRef({ value: 0 });
  useEffect(() => {
    const generation = epoch.current;
    let disposed = false;
    const subscription = listen('backend-unavailable', () => {
      if (!disposed) { generation.value++; setState('failed'); }
    }).catch(() => undefined);
    return () => {
      disposed = true;
      generation.value++;
      void subscription.then(unlisten => { unlisten?.(); });
    };
  }, []);
  async function check() {
    const operationEpoch = epoch.current.value;
    setState('checking');
    try { await backendTransport.health(); if (operationEpoch === epoch.current.value) setState('ok'); }
    catch { if (operationEpoch === epoch.current.value) setState('failed'); }
  }
  return <section className="card backend-health">
    <h2>{t('health.title')}</h2>
    <p aria-live="polite">{t(`health.${state}`)}</p>
    <button disabled={state === 'checking'} onClick={() => { void check(); }}>{t('health.check')}</button>
  </section>;
}
