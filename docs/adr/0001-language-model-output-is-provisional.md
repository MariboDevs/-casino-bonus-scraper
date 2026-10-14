# 1. Language model output is provisional until the source text confirms it

**Status:** accepted

**Context.** The brief requires a language model to turn free-text terms into fields. Models
are good at this and also occasionally confidently wrong: a misread currency, a wagering figure
from a different offer, a typical value instead of a stated one.

**Decision.** Every offer is cross-checked by a deterministic reader over the same text the
model saw. A figure counts as confirmed only when the text states it independently. Offers that
fail the check are still published, flagged "Needs review" with the reason. The raw text is
stored with every offer.

**Consequences.** A reader can always tell a checked number from an unchecked one. The regular
expressions are deliberately conservative, so an unusual phrasing yields "Needs review" rather
than a false confirmation.
