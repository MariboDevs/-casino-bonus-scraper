// Two independent signals, because a scraper fails in two ways: loudly (the run
// errors) and quietly (the run never starts). Both page the same action group.
param name string
param location string
param alertEmail string
param workspaceId string
param jobName string

// The scraper exits 1 when nothing was published and 3 when some casinos failed; the
// Container Apps platform logs every non-zero exit of the job's replica.
var failedRunQuery = '''
ContainerAppSystemLogs_CL
| where JobName_s == "{job}"
| where Reason_s in ("BackoffLimitExceeded", "Error") or (Log_s has "exit code" and Log_s !has "exit code 0")
'''

// The quiet failure: a successful run always logs "published to". None for 26 hours
// (a day plus slack) means the schedule, the image or the platform stopped running it.
var missingRunQuery = '''
let lastSuccess = toscalar(ContainerAppConsoleLogs_CL
    | where ContainerJobName_s == "{job}" and Log_s has "published to"
    | summarize max(TimeGenerated));
print stale = iff(isnull(lastSuccess) or lastSuccess < ago(26h), 1, 0)
| where stale == 1
'''

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'ag-${name}'
  location: 'global'
  properties: {
    groupShortName: 'bonusops'
    enabled: true
    emailReceivers: [
      {
        name: 'owner'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

resource failedRun 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: 'alert-${name}-run-failed'
  location: location
  properties: {
    displayName: 'Scraper run failed or partially failed'
    severity: 1
    enabled: true
    scopes: [workspaceId]
    evaluationFrequency: 'PT15M'
    windowSize: 'PT15M'
    criteria: {
      allOf: [
        {
          query: replace(failedRunQuery, '{job}', jobName)
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: { numberOfEvaluationPeriods: 1, minFailingPeriodsToAlert: 1 }
        }
      ]
    }
    actions: { actionGroups: [actionGroup.id] }
  }
}

resource missingRun 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: 'alert-${name}-no-successful-run'
  location: location
  properties: {
    displayName: 'No successful scrape in 26 hours'
    severity: 2
    enabled: true
    scopes: [workspaceId]
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          query: replace(missingRunQuery, '{job}', jobName)
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: { numberOfEvaluationPeriods: 1, minFailingPeriodsToAlert: 1 }
        }
      ]
    }
    actions: { actionGroups: [actionGroup.id] }
  }
}
