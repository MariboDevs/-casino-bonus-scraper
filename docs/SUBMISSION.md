# Submission notes

> Draft for the candidate to edit. The brief asks for these in your own words.

## Five lines on AI

1. Built with Claude Code (CLI agent), reviewing every step; the extraction itself uses Azure OpenAI, with Claude selectable.
2. It configured the .NET build with `InvariantGlobalization`, which `Microsoft.Data.SqlClient` cannot run under.
3. Local unit tests passed; the SQL Server integration tests in CI failed with "Globalization Invariant Mode is not supported", which is how it was caught.
4. It also first flagged `challenge-platform` as a bot-challenge marker; checking the real pages showed the marker is not a reliable signal, so detection now also requires the review content to be missing.
5. The model's own output is never trusted alone: a deterministic cross-check re-reads the source and flags any figure it cannot find.

## Time taken

TODO: fill in honestly (the brief caps the task at 3 hours).

## What is missing

- Golden-master test that replays the first live recordings through the full pipeline against
  frozen page fixtures (the recordings only exist after the first live run).
- Container image build and push for the production job (the Bicep expects an image).
