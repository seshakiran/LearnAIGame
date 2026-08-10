# LearnAIGame — Product Spec & Build Plan

## 1. Positioning

An invite-only, paid, prestige credential earned by playing — not a mass-market ed-tech app. Comparable in spirit to an exclusive club membership: hard to get in, real signal once you're in. Success metric is not DAU/growth, it's "does this credential get someone a foot in the door."

## 2. Core Loop

1. **Play** — quick casual round (merge/match style), zero learning content visible. Must stand alone as fun.
2. **Micro-unlock trigger** — hitting a score/streak threshold pauses the game on a win, not a loss.
3. **Feynman moment** — 15s video, simple explanation of the AI concept, visually tied to the mechanic just played. Pre-generated and cached per topic (via Grok, in the content pipeline — not generated live at unlock time, to avoid killing reward-timing with latency).
4. **One-tap checkpoint** — single fast question, styled as a game interaction (drag/tap), not a quiz UI. Gate is felt in seconds, not as homework.
5. **Payoff** — game snippet/skin/level unlocks immediately. Loop back to step 1.
6. **Boss level** (every 3–5 topics) — real multi-question assessment, harder, untimed. This is what the certificate actually vouches for — it needs real rigor, not a rubber stamp.

## 3. Curriculum Architecture

- Multiple **paths** (e.g. Product Manager, Hands-on Engineer, ...). Same core loop, different topic sequences and boss-level content depth.
- Topics organized as a visible **skill tree / map** per path — fills in as mastered, gives tangible "AI-literate PM" / "hands-on ML engineer" progress signal.
- **Rotating limited-time topics** (48–72hr windows, e.g. "RAG week") — genuine FOMO since it reruns later, not lost forever, but urgency is real.

## 4. Content Pipeline

- Feynman-explanation videos generated via Grok, produced ahead of time into a content library (per topic, per path where explanations diverge).
- Each topic asset bundle: technical intro copy → Feynman video → one-tap checkpoint question(s) → boss-level question bank entry.
- Content pipeline is a separate authoring workflow from the app itself — needs its own review/QA step before a topic ships, since boss-level rigor determines certificate credibility.

## 5. Certificate & Credential System

- Certificate is issued only after boss-level assessments are passed (not the one-tap checkpoints) — it needs to vouch for something real.
- Must be **verifiable**: public verification link/ID, LinkedIn-addable, so an employer/recruiter can check it's real without trusting the user's word.
- Certificate doubles as the **shareable flex artifact** that drives invite demand — this is also your primary marketing engine (see §7).

## 6. Access & Monetization Model

- Paid-only. No free trial. No public signup.
- Invite-only — each member gets a limited number of invites to extend.
- Deliberately small/prestige, not aiming for mass reach — recognition value comes from scarcity + the certificate's visible rigor, not from volume of holders.

## 7. FOMO & Engagement Mechanics

- Daily streaks unlocking limited-time cosmetics/path badges.
- Curriculum-tied leaderboards ("topics mastered this week") per path, not just game score.
- Rotating limited-time topics (§3).
- Invite scarcity + shareable certificate = the growth loop, in place of open marketing.

## 8. Open Decisions (need your input before implementation planning)

- **Platform**: iOS/Android native, or cross-platform (React Native, given your Syncbook experience)? Web app? Which is primary launch target?
- **Game engine/tech**: native game framework vs. something like Unity, vs. a lighter custom canvas/game-loop built in RN/web?
- **Video generation**: is Grok's video capability confirmed accessible for this use case — has this been tested, or does it need a feasibility spike first?
- **Team/solo**: are you building this solo with me, or is there a team (content, design, backend)?
- **Timeline**: how many weeks realistically available, and is there a target launch trigger (event, cohort, waitlist size)?
- **Invite system infra**: how are invites generated/tracked — simple codes, waitlist + manual approval, referral chains?
- **Certificate verification infra**: hosted verification page, PDF + QR, or integration with an existing credentialing standard (e.g. Open Badges)?

## 9. Proposed Phase Breakdown (to refine once §8 is answered)

- **Phase 0 — Feasibility spikes**: confirm Grok video generation works for the Feynman-clip use case; validate core game loop is fun standalone (paper prototype or minimal build).
- **Phase 1 — Core game loop**: build the standalone casual game (no learning layer yet), get it feeling good.
- **Phase 2 — Learning layer**: wire in Feynman video unlock, one-tap checkpoint, topic content for one path (pilot with ~5-10 topics).
- **Phase 3 — Boss levels + certificate**: assessment engine, certificate issuance + verification.
- **Phase 4 — Access system**: invite codes, paid gating, waitlist.
- **Phase 5 — Second curriculum path + polish**: expand from pilot path to second path, leaderboards, streaks, rotating topics.
