// Copies the scraper's output into the app's public folder so the build ships the
// exact JSON the scraper wrote. The page is generated from that file and nothing else.
import { copyFileSync, existsSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const source = resolve(process.env.OFFERS_JSON ?? resolve(root, '..', 'data', 'casinos.json'));
const target = resolve(root, 'public', 'data', 'casinos.json');

if (!existsSync(source)) {
  console.error(
    `No offers file at ${source}. Run the scraper first: dotnet run --project src/BonusScraper.Cli -- scrape`,
  );
  process.exit(1);
}

mkdirSync(dirname(target), { recursive: true });
copyFileSync(source, target);
console.log(`Copied ${source} to ${target}`);
