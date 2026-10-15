# Production: 200 casinos, updated daily

**Where it runs.** An Azure Container Apps **Job** on the consumption plan, triggered by cron at
05:17 UTC ([`infra/modules/scraper-job.bicep`](../infra/modules/scraper-job.bicep)). A daily
batch does not need a server that idles 23 hours a day: the job exists only while it runs, gets
one automatic retry and a hard one-hour timeout, and logs straight to Log Analytics. 200 casinos
at a polite 2 seconds per host is under 7 minutes even single-threaded; spread across several
review sites it parallelises per host. Only changed pages go to the model: each answer is
stored under a hash of the page text, and an unchanged page reuses it. CI already builds the
image on every push; in production it goes to Azure Container Registry and the job pulls it
with a managed identity.

**Database and backups.** Azure SQL Database, serverless General Purpose with 0.5 to 1 vCore,
auto-pausing after an hour ([`infra/modules/database.bicep`](../infra/modules/database.bicep)).
Schema changes ship as numbered DbUp scripts run before each deploy. Backups are the platform's:
point-in-time restore for 14 days on geo-redundant storage, plus long-term retention of weekly
copies for 8 weeks and monthly copies for 12 months. The published JSON is also kept in Blob
storage with versioning, so yesterday's page can be restored without touching the database.
Entra ID only: no SQL passwords exist.

**Domain, DNS and HTTPS.** The page is on Azure Static Web Apps (Free) with a custom domain
([`infra/modules/site.bicep`](../infra/modules/site.bicep)). The zone is in Azure DNS (or
Cloudflare if the domain already lives there), with a CNAME to the Static Web App; it issues
and renews the TLS certificate itself. The page reads `casinos.json` from Blob storage with
CORS limited to the site's origin and a 5-minute cache.

**Scheduling and knowing it failed.** Two alerts, because a scraper fails in two ways
([`infra/modules/alerts.bicep`](../infra/modules/alerts.bicep)). *Loudly*: the job exits
non-zero (`1` nothing published, `3` some casinos failed), which a log query alert picks up
within 15 minutes. *Quietly*: the schedule, the image or the platform stops running it, so a
second alert fires when no run has logged "published to" for 26 hours. Both go to an action
group (email, and a Teams or Slack webhook).

**Rough monthly cost** (West Europe, list prices):

| Item | Per month |
|---|---|
| Container Apps Job: about 7 min a day at 0.5 vCPU, inside the free grant | about $0 |
| Azure SQL serverless, auto-paused most of the day, 2 GB | $5 to $10 |
| Azure OpenAI: at most 200 short extractions a day, only for changed pages | $3 to $15 |
| Container Registry Basic | $5 |
| Log Analytics, under 1 GB a day capped | $2 to $5 |
| Blob storage with versioning, Static Web Apps Free, Azure DNS zone, domain | about $2 |
| **Total** | **about $20 to $40** |
