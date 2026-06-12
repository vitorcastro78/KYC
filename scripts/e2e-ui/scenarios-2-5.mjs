/**
 * E2E UI — cenários 2–5 (docs/E2E_HOMOLOGACAO.md)
 * Evidências: docs/dossier/06-identidade, 05-sar-uif, 08-audit, 04-rpb
 */
import { chromium } from 'playwright';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const BASE = process.env.KYC_APP_URL || 'http://localhost:5299';
const EMAIL = process.env.KYC_E2E_EMAIL || 'admin@kyc.local';
const PASS = process.env.KYC_E2E_PASSWORD || 'Admin@1234';
const stamp = process.env.E2E_STAMP || new Date().toISOString().replace(/[-:T.Z]/g, '').slice(0, 14);

const dirs = {
  id: path.join(repoRoot, 'docs/dossier/06-identidade'),
  sar: path.join(repoRoot, 'docs/dossier/05-sar-uif'),
  audit: path.join(repoRoot, 'docs/dossier/08-audit'),
  rpb: path.join(repoRoot, 'docs/dossier/04-rpb'),
  e2e: path.join(repoRoot, 'docs/dossier/09-e2e'),
};
for (const d of Object.values(dirs)) fs.mkdirSync(d, { recursive: true });

function loadUiCases() {
  const p = path.join(repoRoot, 'docs/dossier/09-e2e/e2e-ui-cases.json');
  if (!fs.existsSync(p)) return null;
  return JSON.parse(fs.readFileSync(p, 'utf8'));
}

async function fillBlazorTextarea(page, textarea, text) {
  await textarea.click();
  await textarea.evaluate((el, value) => {
    el.value = value;
    el.dispatchEvent(new Event('input', { bubbles: true }));
    el.dispatchEvent(new Event('change', { bubbles: true }));
  }, text);
  await page.waitForTimeout(600);
}

const log = [];
function note(msg) {
  const line = `[${new Date().toISOString()}] ${msg}`;
  log.push(line);
  console.log(line);
}

async function shot(page, folder, name) {
  const file = path.join(folder, `${name}-${stamp}.png`);
  await page.screenshot({ path: file, fullPage: true });
  note(`Screenshot: ${path.relative(repoRoot, file)}`);
  return file;
}

async function login(page) {
  await page.goto(`${BASE}/Identity/Account/Login`, { waitUntil: 'networkidle' });
  await page.fill('input[name="Input.Email"]', EMAIL);
  await page.fill('input[name="Input.Password"]', PASS);
  await page.click('button[type="submit"]');
  await page.waitForURL((u) => !u.pathname.includes('/Login'), { timeout: 60000 });
  note(`Login OK → ${page.url()}`);
}

async function waitBlazor(page, ms = 1500) {
  await page.waitForLoadState('networkidle').catch(() => {});
  await page.waitForTimeout(ms);
}

async function scrollToCompliance(page) {
  const h = page.getByRole('heading', { name: /Conformidade BdP/i });
  await h.scrollIntoViewIfNeeded({ timeout: 60000 });
  await waitBlazor(page, 1000);
}

async function addParty(page, name, roleLabel) {
  const btn = page.getByRole('button', { name: 'Adicionar parte' });
  await btn.scrollIntoViewIfNeeded();
  await btn.click();
  await waitBlazor(page, 600);
  const modal = page.locator('.modal.d-block').filter({ hasText: 'Adicionar parte' });
  await modal.waitFor({ state: 'visible', timeout: 15000 });
  await modal.locator('input.form-control').first().fill(name);
  await modal.getByLabel('Papel', { exact: false }).selectOption({ label: roleLabel });
  await modal.getByRole('button', { name: 'Guardar' }).click();
  await waitBlazor(page, 5000);
}

/** Cenário 2: identidade API + webhook + aprovação bloqueada */
async function scenario2(page, ui) {
  note('=== Cenário 2 — Identidade ===');
  const caseId = ui?.IdentityCaseId;
  if (!caseId) throw new Error('Correr dotnet test --filter E2E-UI-PREP primeiro');
  await page.goto(`${BASE}/cases/${caseId}`, { waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 3000);
  await scrollToCompliance(page);
  await page.getByRole('heading', { name: /Verificação de identidade/i }).waitFor({ timeout: 60000 });

  const verifiedPanel = page.locator('.party-identity-panel').filter({ hasText: 'UBO Verificado UI' });
  const verifyBtn = verifiedPanel.getByRole('button', { name: 'Verificar identidade' });
  let sessionId = `e2e-ui-${stamp}`;
  const partyId = ui.UboVerifiedPartyId;
  if (await verifyBtn.isVisible().catch(() => false)) {
    await verifyBtn.click();
    await waitBlazor(page, 500);
    await page.locator('.modal select').first().selectOption('VideoConference');
    await page.getByRole('button', { name: 'Iniciar' }).click();
    await waitBlazor(page, 4000);
    await shot(page, dirs.id, '02-verificar-identidade-modal');
    const sessionText = await page.locator('text=/Sessão:/i').first().textContent().catch(() => '');
    const sessionMatch = sessionText?.match(/Sessão:\s*(\S+)/i);
    sessionId = sessionMatch?.[1]?.replace(/…$/, '') || sessionId;
  } else {
    note('Sessão já iniciada/verificada — webhook directo');
  }
  note(`PartyId webhook: ${partyId}, session: ${sessionId}`);
  const body = JSON.stringify({
    partyId,
    sessionId,
    verified: true,
    eidasLevel: 'High',
  });
  const wh = await page.request.post(`${BASE}/api/identity/webhook`, {
    data: body,
    headers: { 'Content-Type': 'application/json' },
  });
  const whLog = [
    `POST ${BASE}/api/identity/webhook`,
    `Status: ${wh.status()}`,
    `Body: ${body}`,
    await wh.text(),
  ].join('\n');
  fs.writeFileSync(path.join(dirs.id, `webhook-ui-${stamp}.txt`), whLog, 'utf8');
  note(`Webhook: ${wh.status()}`);

  await page.reload({ waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 3000);
  await shot(page, dirs.id, '02-badge-verificado');

  const approveBtn = page.getByRole('button', { name: 'Aprovar' });
  const disabled = await approveBtn.isDisabled();
  note(`Botão Aprovar desactivado: ${disabled}`);
  await shot(page, dirs.id, '02-aprovar-bloqueado-ubo-pendente');

  return { ok: wh.ok(), disabled };
}

/** Cenário 3: SAR submissão + não aplicável + lista */
async function scenario3(page, ui) {
  note('=== Cenário 3 — SAR ===');
  const sarId = ui?.SarCaseId;
  if (!sarId) throw new Error('Correr dotnet test --filter E2E-UI-PREP primeiro');
  await page.goto(`${BASE}/cases/${sarId}`, { waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 2500);
  await scrollToCompliance(page);

  await page.getByRole('button', { name: 'Comunicar à UIF' }).first().click();
  await waitBlazor(page, 500);
  const narrative =
    'Narrativa E2E homologação UI cenário 3: operação suspeita simulada com fundamento legal, entidades envolvidas, montantes, canais utilizados e descrição detalhada dos factos relevantes para comunicação à UIF conforme Lei 83/2017 e regulamento interno do banco. Inclui timeline e ligações identificadas. ';
  const sarTa = page.locator('.modal textarea').first();
  await fillBlazorTextarea(page, sarTa, narrative.repeat(2));
  await page.waitForFunction(
    () => {
      const btn = document.querySelector('.modal .btn-danger');
      return btn && !btn.hasAttribute('disabled');
    },
    { timeout: 15000 },
  );
  await shot(page, dirs.sar, '03-sar-modal-narrativa');
  await page.getByRole('button', { name: 'Submeter SAR' }).click();
  await waitBlazor(page, 4000);
  await shot(page, dirs.sar, '03-sar-submetido');

  await page.goto(`${BASE}/cases/${ui.IdentityCaseId}`, { waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 2500);
  await page.getByRole('button', { name: 'SAR não aplicável' }).click();
  await waitBlazor(page, 500);
  const just =
    'Sem indícios de branqueamento; relação comercial ordinária documentada. Homologação E2E UI cenário 3.';
  await fillBlazorTextarea(page, page.locator('.modal textarea').first(), just);
  await page.waitForFunction(
    () => {
      const btn = [...document.querySelectorAll('.modal .btn-primary')].find((b) => b.textContent?.includes('Confirmar'));
      return btn && !btn.hasAttribute('disabled');
    },
    { timeout: 15000 },
  );
  await page.getByRole('button', { name: 'Confirmar' }).click();
  await waitBlazor(page, 3000);
  await shot(page, dirs.sar, '03-sar-nao-aplicavel');

  await page.goto(`${BASE}/cases`, { waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 2000);
  await shot(page, dirs.sar, '03-lista-badges-sar-ddc');
  return { ok: true };
}

/** Cenário 4: EDD 4-eyes */
async function scenario4(page, ui) {
  note('=== Cenário 4 — EDD 4-eyes ===');
  const caseId = ui?.EddCaseId;
  if (!caseId) throw new Error('Correr dotnet test --filter E2E-UI-PREP primeiro');
  await page.goto(`${BASE}/cases/${caseId}`, { waitUntil: 'domcontentloaded' });
  const caseUrl = page.url();
  note(`Caso EDD: ${caseId}`);
  await waitBlazor(page, 5000);

  await scrollToCompliance(page);
  const fundsArea = page.locator('text=Origem dos fundos (EDD obrigatório)').locator('xpath=following::textarea[1]');
  await fundsArea.waitFor({ timeout: 120000 });
  await fundsArea.fill(
    'Fundos próprios da actividade comercial; proveniência documentada em extractos bancários 2024–2025.',
  );
  await page.getByRole('button', { name: 'Guardar origem' }).click();
  await waitBlazor(page, 2000);
  await shot(page, dirs.audit, '04-edd-origem-fundos');

  const verifyButtons = page.getByRole('button', { name: 'Verificar identidade' });
  const count = await verifyButtons.count();
  for (let i = 0; i < count; i++) {
    await verifyButtons.nth(i).click();
    await waitBlazor(page, 400);
    await page.locator('.modal select').first().selectOption('Presential');
    await page.locator('.modal input.form-control').fill(`BI-E2E-${stamp}-${i}`);
    await page.getByRole('button', { name: 'Iniciar' }).click();
    await waitBlazor(page, 2500);
  }
  await shot(page, dirs.audit, '04-edd-verificacao-presencial');

  await page.getByRole('button', { name: 'Aprovar' }).click();
  await waitBlazor(page, 800);
  const supervisorSelect = page.locator('.modal select').first();
  if (await supervisorSelect.isVisible().catch(() => false)) {
    await supervisorSelect.selectOption({ index: 1 });
  } else {
    await page.locator('.modal input').fill('supervisor@kyc.local');
  }
  await shot(page, dirs.audit, '04-edd-segundo-aprovador');
  await page.locator('.modal-footer button.btn-primary').click();
  await waitBlazor(page, 4000);
  await shot(page, dirs.audit, '04-edd-aprovado');

  fs.writeFileSync(
    path.join(dirs.audit, `04-edd-case-${stamp}.txt`),
    `CaseId: ${caseId}\nUrl: ${caseUrl}\nSecondApprover: supervisor@kyc.local\n`,
    'utf8',
  );
  return { ok: true, caseId };
}

/** Cenário 5: RPB Admin */
async function scenario5(page) {
  note('=== Cenário 5 — RPB ===');
  await page.goto(`${BASE}/admin/aml-report`, { waitUntil: 'domcontentloaded' });
  await waitBlazor(page, 2000);
  await shot(page, dirs.rpb, '05-rpb-antes');

  await page.getByRole('button', { name: 'Gerar RPB' }).click();
  await waitBlazor(page, 5000);
  await shot(page, dirs.rpb, '05-rpb-gerado');

  const exportXml = page.getByRole('link', { name: /Exportar XML BdP/i });
  if (await exportXml.isVisible().catch(() => false)) {
    const href = await exportXml.getAttribute('href');
    if (href) {
      const res = await page.request.get(href.startsWith('http') ? href : `${BASE}${href}`);
      fs.writeFileSync(path.join(dirs.rpb, `05-rpb-export-bdp-${stamp}.xml`), await res.text(), 'utf8');
      note(`XML BdP exportado (${res.status()})`);
    }
  }

  await page.getByRole('button', { name: 'Marcar submetido à BdP' }).click();
  await waitBlazor(page, 3000);
  await shot(page, dirs.rpb, '05-rpb-submetido');
  return { ok: true };
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();
  const results = {};

  try {
    await login(page);
    const ui = loadUiCases();
    if (!ui) note('AVISO: e2e-ui-cases.json em falta — executar E2E-UI-PREP');
    for (const [key, fn] of [
      ['s2', () => scenario2(page, ui)],
      ['s3', () => scenario3(page, ui)],
      ['s4', () => scenario4(page, ui)],
      ['s5', () => scenario5(page)],
    ]) {
      try {
        results[key] = await fn();
      } catch (err) {
        note(`ERRO ${key}: ${err.message}`);
        results[key] = { ok: false, error: err.message };
        await shot(page, dirs.e2e, `ui-error-${key}`).catch(() => {});
      }
    }
  } catch (err) {
    note(`ERRO fatal: ${err.message}`);
    results.fatal = err.message;
  } finally {
    await browser.close();
  }

  const registro = [
    `# E2E UI cenários 2–5 — ${stamp}`,
    '',
    `App: ${BASE}`,
    `Utilizador: ${EMAIL}`,
    '',
    '| Cenário | Resultado | Evidência |',
    '|---|---------|-----------|',
    `| 2 Identidade | ${results.s2?.ok ? 'OK' : 'PARCIAL'} | 06-identidade/*-${stamp}.png |`,
    `| 3 SAR | ${results.s3?.ok ? 'OK' : 'FALHA'} | 05-sar-uif/*-${stamp}.png |`,
    `| 4 EDD 4-eyes | ${results.s4?.ok ? 'OK' : 'FALHA'} | 08-audit/*-${stamp}.png |`,
    `| 5 RPB | ${results.s5?.ok ? 'OK' : 'FALHA'} | 04-rpb/*-${stamp}.png |`,
    '',
    '## Log',
    ...log,
    '',
    '## Resultados JSON',
    '```json',
    JSON.stringify(results, null, 2),
    '```',
  ].join('\n');

  fs.writeFileSync(path.join(dirs.e2e, `REGISTO_UI_CENARIOS_2-5_${stamp}.md`), registro, 'utf8');
  note('Registo: docs/dossier/09-e2e/' + `REGISTO_UI_CENARIOS_2-5_${stamp}.md`);

  const failed = ['s2', 's3', 's4', 's5'].some((k) => results[k] && results[k].ok === false);
  if (results.fatal || failed) process.exit(1);
}

main();
