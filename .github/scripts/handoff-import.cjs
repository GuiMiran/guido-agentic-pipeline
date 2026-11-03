'use strict';

const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { POLICY } = require('./handoff-acquire.cjs');

function verdictFor(qualification, conclusion, unassessed) {
  return qualification === 'FAIL' ? 'FAIL'
    : qualification === 'PASS' && conclusion === 'success' && unassessed.length === 0 ? 'PASS' : 'UNKNOWN';
}

function run({ bundle, input, spectra, output }) {
  const custody = JSON.parse(fs.readFileSync(path.join(bundle, 'custody.json'), 'utf8'));
  if (custody.spectraCommit !== POLICY.spectraCommit) throw new Error('SPECTRA pin mismatch.');
  const gitHead = require('node:child_process').execFileSync('git', ['-C', spectra, 'rev-parse', 'HEAD'],
    { encoding: 'utf8', env: { PATH: process.env.PATH } }).trim();
  if (gitHead !== POLICY.spectraCommit) throw new Error('SPECTRA checkout revision mismatch.');
  const expected = JSON.parse(fs.readFileSync(path.join(input, 'spectra-expected.json'), 'utf8'));
  const ledger = JSON.parse(fs.readFileSync(path.join(input, 'acceptance-links.json'), 'utf8'));
  const { aggregateTrx, writeTrxEvidence } = require(path.join(spectra, 'lib/evidence/trx-aggregator.js'));
  const runId = `pipeline-${custody.source.runId}-${custody.source.attempt}`;
  const aggregation = aggregateTrx(path.join(input, 'trx'), { expected, runId,
    commit: custody.source.commit, ref: 'refs/heads/main' });
  fs.mkdirSync(output, { recursive: false });
  const files = writeTrxEvidence(output, aggregation);
  const verdict = verdictFor(aggregation.snapshot.qualification.verdict, custody.source.conclusion, ledger.unassessedCriteria);
  const hash = file => crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
  const receipt = { schemaVersion: 1, acquisition: custody.acquisition, mode: custody.mode,
    source: custody.source, artifact: custody.artifact, spectraCommit: POLICY.spectraCommit,
    verdict, testQualification: aggregation.snapshot.qualification.verdict,
    counts: aggregation.snapshot.counts, unassessedCriteria: ledger.unassessedCriteria,
    sourceDigest: ledger.sourceDigest, snapshotSha256: hash(files.snapshotFile), coverageMapSha256: hash(files.mapFile),
    limitations: ['LOCAL_SCOPE_NOT_EXHAUSTIVE', 'NO_COLLECTOR_OR_ASSERTION_INDEPENDENCE_PROOF',
      'NO_SOURCE_EXECUTION_OR_REPAIR', 'NO_CROSS_REPOSITORY_WRITES'] };
  fs.writeFileSync(path.join(output, 'receipt.json'), `${JSON.stringify(receipt, null, 2)}\n`, { flag: 'wx' });
  fs.copyFileSync(path.join(bundle, 'custody.json'), path.join(output, 'custody.json'), fs.constants.COPYFILE_EXCL);
  fs.copyFileSync(path.join(input, 'acceptance-links.json'), path.join(output, 'acceptance-links.json'), fs.constants.COPYFILE_EXCL);
  console.log(JSON.stringify({ verdict, runId, counts: receipt.counts, mode: receipt.mode }));
  return verdict === 'PASS' ? 0 : 1;
}

if (require.main === module) {
  const [bundle, input, spectra, output] = process.argv.slice(2);
  if (![bundle, input, spectra, output].every(value => value && path.isAbsolute(value))) throw new Error('Four absolute paths required.');
  process.exitCode = run({ bundle, input, spectra, output });
}
module.exports = { run, verdictFor };
