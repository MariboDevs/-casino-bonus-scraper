// Starts the dev server on the port Aspire assigns through PORT (4200 when run by hand).
// `ng serve` does not read PORT itself, so without this Aspire's link points at nothing.
import { spawn } from 'node:child_process';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const ng = require.resolve('@angular/cli/bin/ng.js');
const port = process.env.PORT ?? '4200';

const server = spawn(process.execPath, [ng, 'serve', '--port', port, ...process.argv.slice(2)], {
  stdio: 'inherit',
});

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => server.kill(signal));
}

server.on('exit', (code) => process.exit(code ?? 0));
