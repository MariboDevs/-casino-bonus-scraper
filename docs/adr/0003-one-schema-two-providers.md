# 3. One prompt and one strict schema, more than one model provider

**Status:** accepted

**Decision.** The prompt and the JSON schema live in one place and are shared by the Azure
OpenAI and Claude extractors. Both use their provider's structured-output mode, so the answer is
schema-valid by construction. Absence is `null`, never a missing key. Answers can be recorded and
replayed; the recording key hashes the prompt version and the exact input.

**Consequences.** Switching provider is configuration. Tests run on real model answers without a
key. Changing the prompt means bumping its version, which invalidates old recordings on purpose.
