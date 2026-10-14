# 2. Crawl politely and stop at refusals

**Status:** accepted

**Decision.** robots.txt is parsed per RFC 9309 and checked before every request, and an
unreachable robots.txt (5xx or network failure) means "do not crawl". Requests to one host are
at least 2 seconds apart, or the site's crawl-delay if longer; a crawl-delay above 30 seconds
skips the host. The User-Agent names the crawler and links to this repository. A 401, 403, 429
or 451, or a page that is only a bot challenge, ends the attempt for that page and is recorded
as a failure. Retries cover transient faults only.

**Consequences.** Some runs will publish fewer casinos. That is reported (exit code 3, a
failures list on the page, an alert), never worked around.
