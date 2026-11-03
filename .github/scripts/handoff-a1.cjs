'use strict';

// Consumer host only: runs after same-workflow custody and TRX validation.
const fs = require('node:fs');
const path = require('node:path');
const { createHash } = require('node:crypto');
const { execFileSync } = require('node:child_process');
const { POLICY } = require('./handoff-acquire.cjs');
const hash = bytes => createHash('sha256').update(bytes).digest('hex');
const positive = n => Number.isSafeInteger(n) && n > 0;
function read(file) {
  const stat = fs.lstatSync(file);
  if (!stat.isFile() || !stat.size || stat.size > 1024 * 1024) throw new Error('Expected bounded regular input.');
  return fs.readFileSync(file);
}
const parse = bytes => JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(bytes));
function prepare({ custody, receiptBytes, snapshotBytes, expectedBytes, handoffRunId }) {
  const receipt = parse(receiptBytes);
  if (custody?.schemaVersion !== 1 || custody.acquisition !== 'authenticated-github-actions-api' ||
      custody.source?.repository !== `${POLICY.owner}/${POLICY.repo}` ||
      custody.source.repositoryId !== POLICY.repositoryId || custody.source.workflowId !== POLICY.workflowId ||
      !/^[a-f0-9]{40}$/.test(custody.source.commit) || !positive(custody.source.runId) || !positive(custody.source.attempt) ||
      !['workflow_run', 'pull_request'].includes(custody.mode) || custody.spectraCommit !== POLICY.spectraCommit ||
      receipt.snapshotSha256 !== hash(snapshotBytes) || !positive(handoffRunId)) throw new Error('A1 custody or snapshot mismatch.');
  return { schemaVersion: 1, kind: 'RecordedA1InputPins', source: custody.source, artifact: custody.artifact,
    mode: custody.mode, handoffRunId, spectraCommit: POLICY.spectraCommit, auditorCommit: POLICY.auditorCommit,
    hashes: { receiptSha256: hash(receiptBytes), snapshotSha256: hash(snapshotBytes), expectedInventorySha256: hash(expectedBytes) } };
}

function run(workspacePath, tempPath, handoffRunId, handoffAttempt) {
  if (!positive(handoffRunId) || !positive(handoffAttempt)) throw new Error('Invalid receiver identity.');
  const workspace = fs.realpathSync(workspacePath), temp = fs.realpathSync(tempPath);
  if (temp === workspace || temp.startsWith(workspace + path.sep)) throw new Error('Output must be outside checkout workspace.');
  const [target, scale, spectra] = ['target', 'scale', 'spectra'].map(name => {
    const repo = path.join(workspace, name);
    if (fs.realpathSync(repo) !== repo) throw new Error('Unexpected checkout path.');
    return repo;
  });
  const output = path.join(temp, 'handoff-output');
  if (fs.realpathSync(output) !== output) throw new Error('Unexpected handoff output path.');
  const custodyBytes = read(path.join(temp, 'handoff-bundle', 'custody.json'));
  const custody = parse(custodyBytes);
  const { runId, attempt } = custody.source;
  if (!positive(runId) || !positive(attempt)) throw new Error('Invalid source identity.');
  const inputs = { receiptBytes: read(path.join(output, 'receipt.json')),
    snapshotBytes: read(path.join(output, '.spectra/evidence/test-runs', `pipeline-${runId}-${attempt}.json`)),
    expectedBytes: read(path.join(temp, 'handoff-input', 'spectra-expected.json')) };
  const pins = prepare({ custody, ...inputs, handoffRunId });
  const env = { PATH: process.env.PATH, PYTHONDONTWRITEBYTECODE: '1' };
  function clean(repo, commit) {
    const git = (...args) => execFileSync('git', ['-C', repo, ...args], { encoding: 'utf8', env,
      timeout: 10000, maxBuffer: 1024 * 1024 }).trim();
    if (git('rev-parse', 'HEAD') !== commit || git('status', '--porcelain=v1', '--untracked-files=all', '--ignored=matching')) {
      throw new Error('Audit checkout is dirty or at the wrong commit.');
    }
  }
  clean(target, pins.source.commit);
  clean(scale, pins.auditorCommit);
  clean(spectra, pins.spectraCommit);
  const a1 = path.join(output, 'a1');
  fs.mkdirSync(a1);
  const audit = path.join(a1, 'audit');
  execFileSync('python3', [path.join(scale, 'src/guido_auditor/cli.py'), 'audit', target, '--output-dir', audit],
    { env, timeout: 60000, maxBuffer: 1024 * 1024 });
  clean(target, pins.source.commit);
  clean(scale, pins.auditorCommit);
  const auditBytes = read(path.join(audit, 'audit.json'));
  const { projectA1GapPlan } = require(path.join(spectra, 'lib/verification/a1-gap-plan.js'));
  const generatedAt = new Date().toISOString();
  const { projection, ...review } = projectA1GapPlan({ ...inputs, auditBytes }, pins,
    { generatedAt, correlationId: `a1-handoff-${handoffRunId}-${handoffAttempt}` });
  function write(name, value) {
    const bytes = Buffer.from(JSON.stringify(value, null, 2) + '\n');
    fs.writeFileSync(path.join(a1, name), bytes, { flag: 'wx' });
    return hash(bytes);
  }
  const contractsFileSha256 = write('contracts.json', projection.contracts);
  const gapReviewFileSha256 = write('gap-review.json', review);
  const receipt = { schemaVersion: 1, kind: 'authenticated-a1-handoff-review', generatedAt, handoffAttempt, pins,
    custodySha256: hash(custodyBytes), auditSha256: hash(auditBytes), contractsFileSha256, gapReviewFileSha256,
    contractsSha256: projection.provenance.contractsSha256, gapReportSha256: review.gapReportSha256,
    counts: projection.contracts.evidenceBundle.counts, outcome: review.outcome, measuredGaps: review.gapReport.gaps.length,
    targetCleanBefore: true, targetCleanAfter: true, targetCodeExecutedByConsumer: false,
    requiresHumanApproval: true, executionAuthorized: false, a1CycleComplete: false };
  write('receipt.json', receipt);
  console.log(JSON.stringify(receipt));
}
if (require.main === module) {
  const [workspace, temp, runId, attempt] = process.argv.slice(2);
  run(workspace, temp, Number(runId), Number(attempt));
}
module.exports = { prepare };
