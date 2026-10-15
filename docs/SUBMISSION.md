# Submission notes

## Five lines on AI

1. I used Claude Code as the coding agent for the whole build, and I reviewed each step before it went in; the scraper itself uses Azure OpenAI (gpt-5.5) to read the bonus terms, with Claude selectable.
2. Where it was wrong: it wrote the check that verifies the model's figures so that it treated a review's article text as just as reliable as the operator's own terms box.
3. On the first live run that flagged Grosvenor as "needs review": its article still described an old 100% match offer, while the terms box said a 30 GBP bonus.
4. I caught it by reading the flagged row against the live page: the model had read the terms box correctly, and it was the check that was wrong.
5. The check now verifies against the operator's terms first and records which text it used, and a golden-master test replays the real model answers so the mistake cannot come back.

## Time taken

About 5.5 hours of hands-on time over two sittings. The core brief (scraper, JSON, page,
README and production plan) took about 2 hours; past the 3-hour mark I chose to keep going
on the parts I wanted to show properly: the verification step, the tests, the daily
publishing job and a redesign of the page.

## Why Magic Red and not Casumo

The first live run scraped Casumo. Its review has no operator terms box, so its figures came
from the review article, which turned out to be out of date: it said a 100 EUR minimum deposit
and 20 free spins, while Casumo's own promotions page (checked 2026-10-15) said "Min. deposit
€20" and 50 bonus spins. The scraper reported the source faithfully and labelled the row
"Checked against review text", but a correct number beats a well-labelled wrong one, so the
third casino is now Magic Red, whose operator terms state every field. 10bet was considered
and rejected for the same reason: its terms box omits the minimum deposit and its article is
stale. Casumo's page stays in the tests as the case where only the article is available.

## What is missing

- Container image build and push for the production job (the Bicep expects an image).
