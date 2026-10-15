# Submission notes

> Draft for the candidate to edit. The brief asks for these in your own words.

## Five lines on AI

1. Built with Claude Code (CLI agent), reviewing every step; the extraction itself uses Azure OpenAI, with Claude selectable.
2. It configured the .NET build with `InvariantGlobalization`, which `Microsoft.Data.SqlClient` cannot run under.
3. Local unit tests passed; the SQL Server integration tests in CI failed with "Globalization Invariant Mode is not supported", which is how it was caught.
4. On the first live run it flagged Grosvenor as "needs review" because its check read the review's stale article (an old 100% match offer) as equal to the operator's terms box; the model had followed the "operator terms win" rule and was right.
5. I caught it by reading the flagged text against the page; the check now verifies against the operator's terms first, and a golden-master test replays the real model answers to lock it in.

## Time taken

TODO: fill in honestly (the brief caps the task at 3 hours).

## Known data caveat

Casumo's review on casinoguide.co.uk has no operator terms box, so its figures come from the
article, which is out of date: it says a 100 EUR minimum deposit and 20 free spins, while
Casumo's own promotions page (checked 2026-10-15) says "Min. deposit €20" and 50 bonus spins.
The scraper reports the source faithfully and the page labels the row "Checked against review
text" so a reader knows the weaker basis. Switching the third casino to one with an operator
terms box (10bet, for example) would remove the caveat.

## What is missing

- Container image build and push for the production job (the Bicep expects an image).
