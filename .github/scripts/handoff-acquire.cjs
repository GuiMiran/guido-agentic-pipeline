'use strict';

// Trusted host adapter only. Artifact bytes are DATA, never required/evaluated.
const fs = require('node:fs');
const crypto = require('node:crypto');
const path = require('node:path');
const POLICY = Object.freeze({ owner: 'GuiMiran', repo: 'guido-agentic-pipeline', repositoryId: 1190006301,
  workflowId: 250433858, workflowPath: '.github/workflows/run-tests.yml', branch: 'main',
  spectraCommit: 'bb561e388eb3406b1ddd4792c4f8e81769b108c6' });
const sha256 = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
const gitSha = bytes => crypto.createHash('sha1').update(`blob ${bytes.length}\0`).update(bytes).digest('hex');
const integer = value => Number.isSafeInteger(value) && value > 0;

function validateRun(run) {
  if (!run || !integer(run.id) || !integer(run.run_attempt) || run.workflow_id !== POLICY.workflowId ||
      run.path !== POLICY.workflowPath || run.name !== 'Run Tests' || run.event !== 'push' ||
      run.head_branch !== POLICY.branch || run.status !== 'completed' || !['success', 'failure'].includes(run.conclusion) ||
      run.repository?.id !== POLICY.repositoryId || run.head_repository?.id !== POLICY.repositoryId ||
      run.repository?.full_name !== `${POLICY.owner}/${POLICY.repo}` ||
      run.head_repository?.full_name !== `${POLICY.owner}/${POLICY.repo}` || !/^[a-f0-9]{40}$/.test(run.head_sha)) {
    throw new Error('Untrusted source run provenance.');
  }
  return run;
}

function validateArtifact(artifact, run) {
  const origin = artifact?.workflow_run;
  if (!artifact || !integer(artifact.id) || artifact.name !== `pipeline-trx-${run.id}-${run.run_attempt}` ||
      artifact.expired !== false || !integer(artifact.size_in_bytes) || artifact.size_in_bytes > 8 * 1024 * 1024 ||
      !/^sha256:[a-f0-9]{64}$/.test(artifact.digest) || origin?.id !== run.id ||
      origin.repository_id !== POLICY.repositoryId || origin.head_repository_id !== POLICY.repositoryId ||
      origin.head_branch !== POLICY.branch || origin.head_sha !== run.head_sha) {
    throw new Error('Untrusted, expired or oversized source artifact.');
  }
  return artifact;
}

function verifyArchive(bytes, artifact) {
  if (!Buffer.isBuffer(bytes) || bytes.length !== artifact.size_in_bytes ||
      `sha256:${sha256(bytes)}` !== artifact.digest) throw new Error('Source archive digest/size mismatch.');
}

function sourceFiles(tree) {
  if (tree.truncated || !Array.isArray(tree.tree)) throw new Error('Incomplete source tree.');
  const selected = tree.tree.filter(entry => entry.type === 'blob' &&
    (['specs/spectra-mapping.json', '.spectra/11-acceptance-criteria.md'].includes(entry.path) ||
     entry.path.startsWith('specs/') && /(?:\.feature|\.context\.md)$/.test(entry.path) ||
     entry.path.startsWith('src/GUIDO.Agentic.Tests/') && entry.path.endsWith('.cs') && !/(?:^|\/)(?:bin|obj)\//.test(entry.path)));
  if (!selected.length || selected.length > 100 || selected.some(entry => entry.mode !== '100644' ||
      !/^[A-Za-z0-9_./-]+$/.test(entry.path) || entry.path.split('/').includes('..') ||
      !/^[a-f0-9]{40}$/.test(entry.sha) || !integer(entry.size) || entry.size > 2 * 1024 * 1024)) {
    throw new Error('Invalid source dependency inventory.');
  }
  if (!selected.some(entry => entry.path === 'specs/spectra-mapping.json') ||
      !selected.some(entry => entry.path === '.spectra/11-acceptance-criteria.md')) throw new Error('Missing canonical source documents.');
  return selected.sort((a, b) => a.path < b.path ? -1 : a.path > b.path ? 1 : 0);
}

async function acquire({ github, context, outputDir }) {
  if (context.repo.owner !== POLICY.owner || context.repo.repo !== POLICY.repo) throw new Error('Unexpected receiving repository.');
  let runId;
  if (context.eventName === 'workflow_run') {
    const event = validateRun(context.payload.workflow_run);
    runId = event.id;
  } else if (context.eventName === 'pull_request' &&
      context.payload.pull_request?.head?.repo?.full_name === `${POLICY.owner}/${POLICY.repo}`) {
    // Read-only PR smoke: consume the latest COMPLETED main push, never a PR artifact.
    const response = await github.rest.actions.listWorkflowRuns({ owner: POLICY.owner, repo: POLICY.repo,
      workflow_id: POLICY.workflowId, branch: POLICY.branch, event: 'push', status: 'completed', per_page: 1 });
    runId = response.data.workflow_runs?.[0]?.id;
  } else throw new Error('Unsupported or fork receiving event.');
  if (!integer(runId)) throw new Error('Missing source run.');
  const repo = { owner: POLICY.owner, repo: POLICY.repo };
  const run = validateRun((await github.rest.actions.getWorkflowRun({ ...repo, run_id: runId })).data);
  if (context.eventName === 'workflow_run' && (run.head_sha !== context.payload.workflow_run.head_sha ||
      run.run_attempt !== context.payload.workflow_run.run_attempt || run.conclusion !== context.payload.workflow_run.conclusion)) {
    throw new Error('Source event is stale or changed.');
  }
  const listing = (await github.rest.actions.listWorkflowRunArtifacts({ ...repo, run_id: run.id, per_page: 100 })).data;
  if (listing.total_count > 100) throw new Error('Artifact listing exceeds bounded page.');
  const matches = listing.artifacts.filter(item => item.name === `pipeline-trx-${run.id}-${run.run_attempt}`);
  if (matches.length !== 1) throw new Error('Missing or duplicate source artifact.');
  const artifact = validateArtifact(matches[0], run);
  const bytes = Buffer.from((await github.rest.actions.downloadArtifact({ ...repo, artifact_id: artifact.id, archive_format: 'zip' })).data);
  verifyArchive(bytes, artifact);
  const tree = (await github.rest.git.getTree({ ...repo, tree_sha: run.head_sha, recursive: '1' })).data;
  const entries = sourceFiles(tree), files = [], documents = {};
  for (const entry of entries) {
    const blob = (await github.rest.git.getBlob({ ...repo, file_sha: entry.sha })).data;
    if (blob.encoding !== 'base64' || blob.size !== entry.size) throw new Error('Invalid source blob metadata.');
    const content = Buffer.from(blob.content, 'base64');
    if (content.length !== entry.size || gitSha(content) !== entry.sha) throw new Error('Source Git blob mismatch.');
    files.push({ file: entry.path, sha256: sha256(content) });
    if (['specs/spectra-mapping.json', '.spectra/11-acceptance-criteria.md'].includes(entry.path)) {
      documents[entry.path] = new TextDecoder('utf-8', { fatal: true }).decode(content);
    }
  }
  const attestation = { schemaVersion: 1, acquisition: 'authenticated-github-actions-api', mode: context.eventName,
    source: { repository: `${POLICY.owner}/${POLICY.repo}`, repositoryId: POLICY.repositoryId,
      workflowId: POLICY.workflowId, runId: run.id, attempt: run.run_attempt, commit: run.head_sha, conclusion: run.conclusion },
    artifact: { id: artifact.id, name: artifact.name, size: artifact.size_in_bytes, digest: artifact.digest },
    spectraCommit: POLICY.spectraCommit, files, documents };
  fs.mkdirSync(outputDir, { recursive: false });
  fs.writeFileSync(path.join(outputDir, 'source.zip'), bytes, { flag: 'wx' });
  fs.writeFileSync(path.join(outputDir, 'custody.json'), `${JSON.stringify(attestation, null, 2)}\n`, { flag: 'wx' });
  return { sourceRunId: run.id, sourceCommit: run.head_sha, artifactId: artifact.id, mode: context.eventName };
}

module.exports = { POLICY, validateRun, validateArtifact, verifyArchive, sourceFiles, acquire };
