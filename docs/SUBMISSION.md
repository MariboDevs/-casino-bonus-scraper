# Why Magic Red and not Casumo

The first live run scraped Casumo. Its review has no operator terms box, so its figures came
from the review article, which turned out to be out of date: it said a 100 EUR minimum deposit
and 20 free spins, while Casumo's own promotions page (checked 2026-10-15) said "Min. deposit
€20" and 50 bonus spins. The scraper reported the source faithfully and labelled the row
"Checked against review text", but a correct number beats a well-labelled wrong one, so the
third casino is now Magic Red, whose operator terms state every field. 10bet was considered
and rejected for the same reason: its terms box omits the minimum deposit and its article is
stale. Casumo's page stays in the tests as the case where only the article is available.

The five lines on AI, the time taken and what is missing are in the [README](../README.md).
