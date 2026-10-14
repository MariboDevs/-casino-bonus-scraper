# 4. A run that scraped nothing never overwrites the published data

**Status:** accepted

**Decision.** If no casino produced an offer, the run is saved for history but the JSON file and
blob are left as they were, and the run fails (exit code 1). A partial run publishes what it
has and lists the rest as failures. The file is written to a temporary sibling and moved into
place, so readers never see half a file.

**Consequences.** A source outage shows as a stale "Updated" time and an alert, not an empty
page.
