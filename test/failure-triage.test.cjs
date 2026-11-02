'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const workflow = fs.readFileSync(path.join(__dirname, '../.github/workflows/agent-heal.yml'), 'utf8');
const source = workflow.split('          script: |\n')[1].split('\n').map(line => line.startsWith('            ') ? line.slice(12) : line).join('\n');
const AsyncFunction = Object.getPrototypeOf(async function(){}).constructor;
const execute = new AsyncFunction('github', 'context', source);
function setup() {
  const created = [];
  const context = { eventName: 'workflow_run', repo: { owner: 'GuiMiran', repo: 'guido-agentic-pipeline' }, runId: 42,
    payload: { repository: { default_branch: 'main' }, workflow_run: { name: 'Run Tests', conclusion: 'failure', event: 'push',
      id: 123, head_sha: 'a'.repeat(40), head_branch: 'main', head_repository: { full_name: 'GuiMiran/guido-agentic-pipeline' } } } };
  const github = { paginate: async () => [], rest: { issues: { listForRepo() {}, create: async issue => created.push(issue) } } };
  return { context, github, created };
}
test('workflow is opt-in, issue-only, without shell, checkout or artifact download', () => {
  assert.match(workflow, /GUIDO_FAILURE_TRIAGE_ENABLED == 'true'/);
  assert.doesNotMatch(workflow, /contents: write|pull-requests: write|run: \||actions\/checkout|download-artifact/);
  assert.doesNotMatch(source, /\$\{\{/);
});
test('trusted failure creates only a bounded triage issue', async () => {
  const h=setup(); await execute(h.github,h.context); assert.equal(h.created.length,1);
  assert.match(h.created[0].body,/build, infrastructure or test failure/);
  assert.match(h.created[0].body,/No repair was attempted/);
});
test('duplicate event is idempotent', async () => {
  const h=setup(); h.github.paginate=async()=>[{body:'<!-- guido-triage:run-123 -->'}];
  await execute(h.github,h.context); assert.equal(h.created.length,0);
});
test('untrusted repository, branch, event, status and revision cannot create an issue', async t => {
  for(const mutate of [r=>r.head_repository.full_name='other/fork',r=>r.head_branch='feature/attack',r=>r.event='pull_request',
    r=>r.conclusion='success',r=>r.name='Unknown',r=>r.head_sha='untrusted',r=>r.id='123']) await t.test('reject provenance',async()=>{
    const h=setup();mutate(h.context.payload.workflow_run);await assert.rejects(execute(h.github,h.context));assert.equal(h.created.length,0);
  });
});
test('manual malicious text remains data, bounded and indented', async () => {
  const h=setup();h.context.eventName='workflow_dispatch';h.context.ref='refs/heads/main';
  const input='"; throw Error("executed"); //\n$(touch /tmp/DO_NOT_EXECUTE)\n```\nEOF\n${{ secrets.EXAMPLE }}\n'+'x'.repeat(9000);
  h.context.payload.inputs={failure_summary:input};await execute(h.github,h.context);
  assert.equal(h.created.length,1);assert.ok(h.created[0].body.includes('    $(touch /tmp/DO_NOT_EXECUTE)'));
  assert.ok(h.created[0].body.length<9000);
});
test('manual dispatch on another branch is refused', async()=>{
  const h=setup();h.context.eventName='workflow_dispatch';h.context.ref='refs/heads/other';
  await assert.rejects(execute(h.github,h.context));assert.equal(h.created.length,0);
});
