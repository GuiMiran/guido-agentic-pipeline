'use strict';
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const test = require('node:test');
const { POLICY, validateRun, validateArtifact, verifyArchive, sourceFiles, acquire } = require('../.github/scripts/handoff-acquire.cjs');
const { verdictFor } = require('../.github/scripts/handoff-import.cjs');
const clone = value => JSON.parse(JSON.stringify(value));
const bytes = Buffer.from('archive bytes are never executed');
const repo = { id: POLICY.repositoryId, full_name: `${POLICY.owner}/${POLICY.repo}` };
const run = { id: 42, run_attempt: 2, workflow_id: POLICY.workflowId, path: POLICY.workflowPath,
  name: 'Run Tests', event: 'push', head_branch: 'main', status: 'completed', conclusion: 'success',
  head_sha: 'a'.repeat(40), repository: repo, head_repository: repo };
const artifact = { id: 99, name: 'pipeline-trx-42-2', expired: false, size_in_bytes: bytes.length,
  digest: 'sha256:' + crypto.createHash('sha256').update(bytes).digest('hex'),
  workflow_run: { id: 42, repository_id: repo.id, head_repository_id: repo.id, head_branch: 'main', head_sha: run.head_sha } };

test('only completed same-repository Run Tests main pushes qualify', () => {
  assert.equal(validateRun(run), run);
  for (const patch of [{ id: '42' }, { run_attempt: 0 }, { workflow_id: 1 }, { path: '.github/workflows/evil.yml' },
    { name: 'Same name is insufficient' }, { event: 'pull_request' }, { head_branch: 'feature/evil' },
    { status: 'in_progress' }, { conclusion: 'cancelled' }, { head_sha: 'unknown' },
    { repository: { ...repo, id: 1 } }, { head_repository: { ...repo, full_name: 'attacker/fork' } }]) {
    assert.throws(() => validateRun({ ...run, ...patch }), /provenance/);
  }
  assert.equal(validateRun({ ...run, conclusion: 'failure' }).conclusion, 'failure');
});

test('artifact provenance, attempt, digest and bounded size are mandatory', () => {
  assert.equal(validateArtifact(artifact, run), artifact);
  for (const patch of [{ id: '99' }, { name: 'pipeline-trx-42-1' }, { expired: true }, { digest: null },
    { size_in_bytes: 9 * 1024 * 1024 }, { workflow_run: { ...artifact.workflow_run, head_sha: 'b'.repeat(40) } },
    { workflow_run: { ...artifact.workflow_run, head_repository_id: 1 } },
    { workflow_run: { ...artifact.workflow_run, id: 41 } }]) {
    assert.throws(() => validateArtifact({ ...artifact, ...patch }, run));
  }
  verifyArchive(bytes, artifact);
  assert.throws(() => verifyArchive(Buffer.from('different'), artifact), /mismatch/);
  assert.throws(() => verifyArchive(bytes, { ...artifact, digest: 'sha256:' + '0'.repeat(64) }), /mismatch/);
});

function mocks() {
  const contents = { '.spectra/11-acceptance-criteria.md': '# Scope\n## AC-001 — One\n',
    'specs/spectra-mapping.json': '{"schemaVersion":1,"mappings":[]}' };
  const blobs = {};
  const tree = Object.entries(contents).map(([file, text]) => {
    const content = Buffer.from(text), sha = crypto.createHash('sha1').update(`blob ${content.length}\0`).update(content).digest('hex');
    blobs[sha] = { content: content.toString('base64'), size: content.length, encoding: 'base64' };
    return { path: file, sha, mode: '100644', type: 'blob', size: content.length };
  });
  const calls = [];
  const fn = (name, data) => async args => { calls.push({ name, args }); return { data }; };
  const github = { rest: {
    actions: { getWorkflowRun: fn('run', run), listWorkflowRuns: fn('latest-main', { workflow_runs: [run] }),
      listWorkflowRunArtifacts: fn('artifacts', { total_count: 1, artifacts: [artifact] }), downloadArtifact: fn('download', bytes) },
    git: { getTree: fn('tree', { truncated: false, tree }), getBlob: async args => { calls.push({ name: 'blob', args }); return { data: blobs[args.file_sha] }; } },
  } };
  const context = { repo: { owner: POLICY.owner, repo: POLICY.repo }, eventName: 'workflow_run', payload: { workflow_run: clone(run) } };
  return { github, context, calls, tree };
}

test('authenticated acquisition writes inert archive plus custody and hashes only', async t => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'handoff-acquire-')); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const m = mocks(), outputDir = path.join(root, 'bundle');
  const result = await acquire({ ...m, outputDir });
  assert.equal(result.sourceRunId, 42);
  assert.deepEqual(fs.readFileSync(path.join(outputDir, 'source.zip')), bytes);
  const custody = JSON.parse(fs.readFileSync(path.join(outputDir, 'custody.json')));
  assert.equal(custody.acquisition, 'authenticated-github-actions-api');
  assert.equal(custody.files.length, 2);
  assert.equal(custody.spectraCommit, POLICY.spectraCommit);
  assert.deepEqual(fs.readdirSync(outputDir).sort(), ['custody.json', 'source.zip']);
  assert.deepEqual(new Set(m.calls.map(call => call.name)), new Set(['run', 'artifacts', 'download', 'tree', 'blob']));
});

test('fork and unsupported events cannot acquire data; source event drift is rejected', async () => {
  const bad = mocks(); bad.context.eventName = 'pull_request'; bad.context.payload = { pull_request: { head: { repo: { full_name: 'attacker/fork' } } } };
  await assert.rejects(() => acquire({ ...bad, outputDir: '/unused' }), /fork/);
  assert.equal(bad.calls.length, 0);
  const stale = mocks(); stale.context.payload.workflow_run.run_attempt = 1;
  await assert.rejects(() => acquire({ ...stale, outputDir: '/unused' }), /stale/);
  const unsupported = mocks(); unsupported.context.eventName = 'issue_comment';
  await assert.rejects(() => acquire({ ...unsupported, outputDir: '/unused' }), /Unsupported/);
});

test('PR smoke selects only the latest completed main push, never PR-supplied run IDs', async t => {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'handoff-smoke-')); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const m = mocks(); m.context.eventName = 'pull_request';
  m.context.payload = { pull_request: { head: { repo: { full_name: repo.full_name } } }, workflow_run: { id: 999 } };
  await acquire({ ...m, outputDir: path.join(root, 'bundle') });
  assert.equal(m.calls[0].name, 'latest-main');
  assert.equal(m.calls[0].args.workflow_id, POLICY.workflowId);
  assert.equal(m.calls[0].args.event, 'push');
  assert.equal(m.calls.find(call => call.name === 'run').args.run_id, 42);
});

test('missing/duplicate archives, truncated trees and invalid Git blobs fail closed', async () => {
  for (const artifacts of [[], [artifact, artifact]]) {
    const m = mocks(); m.github.rest.actions.listWorkflowRunArtifacts = async () => ({ data: { total_count: artifacts.length, artifacts } });
    await assert.rejects(() => acquire({ ...m, outputDir: '/unused' }), /Missing or duplicate/);
  }
  const m = mocks(); m.github.rest.git.getBlob = async () => ({ data: { content: Buffer.from('tampered').toString('base64'), size: m.tree[0].size, encoding: 'base64' } });
  await assert.rejects(() => acquire({ ...m, outputDir: '/unused' }), /blob mismatch/);
  assert.throws(() => sourceFiles({ truncated: true, tree: m.tree }), /Incomplete/);
  assert.throws(() => sourceFiles({ tree: m.tree.map(item => ({ ...item, mode: '120000' })) }), /Invalid/);
});

test('failed source runs, unknown evidence and unassessed criteria never yield PASS', () => {
  assert.equal(verdictFor('PASS', 'success', []), 'PASS');
  assert.equal(verdictFor('PASS', 'failure', []), 'UNKNOWN');
  assert.equal(verdictFor('PASS', 'success', ['AC-025']), 'UNKNOWN');
  assert.equal(verdictFor('UNKNOWN', 'success', []), 'UNKNOWN');
  assert.equal(verdictFor('FAIL', 'success', []), 'FAIL');
});

test('workflow separates token acquisition and clean pinned-code consumption without writes', () => {
  const yaml = fs.readFileSync(path.join(__dirname, '../.github/workflows/spectra-handoff.yml'), 'utf8');
  assert.ok(!yaml.includes('pull_request_target'));
  assert.ok(!/\b(?:issues|pull-requests|contents|actions|id-token): write/.test(yaml));
  assert.ok(!/secrets\./.test(yaml));
  const consume = yaml.split('\n  consume:')[1];
  assert.ok(!consume.includes('actions: read'));
  assert.ok(consume.includes(`ref: ${POLICY.spectraCommit}`));
  assert.ok(consume.includes('persist-credentials: false'));
  assert.ok(consume.includes('env -i PATH="$PATH" node'));
  assert.ok(!yaml.includes('ref: ${{ github.event.workflow_run.head_sha }}'));
  assert.ok(yaml.includes('include-hidden-files: true'));
  assert.ok(!/npm (?:install|ci)|dotnet test|eval\(/.test(yaml));
});
