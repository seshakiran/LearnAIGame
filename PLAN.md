# LearnAIGame — Product Spec & Build Plan

## 1. Positioning

An invite-only, paid, prestige credential earned by playing — not a mass-market ed-tech app. Comparable in spirit to an exclusive club membership: hard to get in, real signal once you're in. Success metric is not DAU/growth, it's "does this credential get someone a foot in the door."

## 2. Differentiation Thesis — Why This Isn't Coursera, and Why AI Doesn't Commoditize It

This is the core intellectual property of the certificate and needs to shape every piece of content and every assessment question. Everything else in this doc is downstream of getting this right.

### 2.1 The problem: content and execution are now free

Anyone can ask Claude or ChatGPT to explain a transformer, generate a RAG pipeline, or write a product spec. That means:
- A course that teaches "here's how AI works" or "here's how to prompt a model" has near-zero differentiated value — the content itself is now infinite and free.
- Coursera-style platforms compete on breadth and volume of content. That's a losing game against a world where the content is generated on demand by the same tools the course is trying to teach.
- If the boss-level assessments test recall of facts or ability to produce output ("write a prompt that does X," "define embeddings"), the certificate is testing something AI already does better than a human — it certifies a skill that's being actively devalued.

### 2.2 What doesn't commoditize: judgment

What AI cannot do for you, and what becomes *more* valuable as AI execution gets better:
- **Catching AI when it's confidently wrong.** Models produce plausible, fluent, wrong answers with the same confidence as correct ones. The skill of noticing "this is wrong even though it reads well" does not come from the model — it has to be trained separately.
- **Making the tradeoff calls a model can't make for you.** A model doesn't know your org's risk tolerance, your team's technical debt, your company's specific constraints, or what "good enough" means in your context. It will give you *a* defensible answer, not necessarily *your* correct answer.
- **Critically evaluating output rather than accepting it.** Given two AI-generated solutions (two specs, two architectures, two pieces of code), knowing which is actually better — and why — for a specific situation.
- **Knowing what question to ask.** Scoping and framing a problem correctly is what determines whether the AI's output is even useful; this is upstream of anything the model does.

### 2.3 The paradigm-shift framing

This is analogous to the shift from imperative to functional programming — not "more syntax to learn," but a genuinely different mental model. The shift here is from **"I produce the answer"** to **"I supervise, verify, and steer a system that produces candidate answers."** That is the actual emerging professional skill of the AI era — sometimes called AI oversight, verification judgment, or orchestration-level thinking (as opposed to line-level execution). Teaching this well is what justifies a premium, paid-only positioning: it requires case-study design and hard assessment, not video lecture volume, and it cannot be replicated by "watch more content."

### 2.4 What this means for assessment design (concrete)

Boss-level questions must be scenario- and judgment-based, never recall-based. Examples of the right shape of question:
- Given a plausible-but-flawed AI output (code, spec, analysis), identify the flaw and explain *why* it's wrong, not just *that* it's wrong.
- Given ambiguous or underspecified requirements, make a tradeoff call and justify it — there is no single correct answer, the assessment is of the reasoning.
- Given two AI-generated solutions to the same problem, evaluate which is better for a *specific stated context*, and defend the choice.
- Given a case where the "obvious" AI-suggested path is actually wrong for this situation, argue the correct path and identify what the model missed.

Explicitly avoid: "define X," "what is Y," "write a prompt that does Z" — these test something AI already does better than the person being certified.

### 2.5 Path-specific application

- **PM path**: judgment on scope, prioritization, and product tradeoffs when AI can generate any spec on demand — the test is *which* spec is right for this business context, not whether one can be produced.
- **Engineer path**: judgment on architecture, debugging, and tradeoff calls when AI can generate any code on demand — the test is spotting which AI-suggested approach will cause problems in six months, not whether code can be written.

### 2.6 The hiring pitch this enables (ties to §9)

Companies aren't hiring for "knows AI facts" — that's ubiquitous and worthless as a signal. They're hiring for "will catch the AI's mistakes before they ship," which becomes more role-critical as AI-generated work volume increases across every function. This is the actual product being sold to companies in the platform/hiring layer: not a talent pool, a **judgment-vetting layer** that a resume or a normal interview loop doesn't test for.

## 3. Core Loop

1. **Play** — quick casual round (merge/match style), zero learning content visible. Must stand alone as fun.
2. **Micro-unlock trigger** — hitting a score/streak threshold pauses the game on a win, not a loss.
3. **Feynman moment** — 15s video, simple explanation of the AI concept, visually tied to the mechanic just played. Pre-generated and cached per topic (via Grok, in the content pipeline — not generated live at unlock time, to avoid killing reward-timing with latency).
4. **One-tap checkpoint** — single fast question, styled as a game interaction (drag/tap), not a quiz UI. Gate is felt in seconds, not as homework.
5. **Payoff** — game snippet/skin/level unlocks immediately. Loop back to step 1.
6. **Boss level** (every 3–5 topics) — real multi-question assessment, harder, untimed. This is what the certificate actually vouches for — it needs real rigor, not a rubber stamp.

## 4. Curriculum Architecture

- Multiple **paths** (e.g. Product Manager, Hands-on Engineer, ...). Same core loop, different topic sequences and boss-level content depth.
- Topics organized as a visible **skill tree / map** per path — fills in as mastered, gives tangible "AI-literate PM" / "hands-on ML engineer" progress signal.
- **Rotating limited-time topics** (48–72hr windows, e.g. "RAG week") — genuine FOMO since it reruns later, not lost forever, but urgency is real.

## 5. Content Pipeline

- Feynman-explanation videos generated via Grok, produced ahead of time into a content library (per topic, per path where explanations diverge).
- Each topic asset bundle: technical intro copy → Feynman video → one-tap checkpoint question(s) → boss-level question bank entry.
- Content pipeline is a separate authoring workflow from the app itself — needs its own review/QA step before a topic ships, since boss-level rigor determines certificate credibility.

## 6. Certificate & Credential System

- Certificate is issued only after boss-level assessments are passed (not the one-tap checkpoints) — it needs to vouch for something real, specifically the judgment-based skills defined in §2, not fact recall.
- Must be **verifiable**: public verification link/ID, LinkedIn-addable, so an employer/recruiter can check it's real without trusting the user's word.
- Certificate doubles as the **shareable flex artifact** that drives invite demand — this is also your primary marketing engine (see §7).

## 7. Access & Monetization Model

- Paid-only. No free trial. No public signup.
- Invite-only — each member gets a limited number of invites to extend.
- Deliberately small/prestige, not aiming for mass reach — recognition value comes from scarcity + the certificate's visible rigor, not from volume of holders.

## 8. FOMO & Engagement Mechanics

- Daily streaks unlocking limited-time cosmetics/path badges.
- Curriculum-tied leaderboards ("topics mastered this week") per path, not just game score.
- Rotating limited-time topics (§4).
- Invite scarcity + shareable certificate = the growth loop, in place of open marketing.

## 9. Platform & Hiring Layer (Phase 2 — only after core game + certificate proves out)

This is explicitly a second phase. Do not build this in parallel with the core loop — it depends on the certificate already having real credibility (§2, §6) with a working pool of certified people.

### 9.1 Model: placement, not marketplace (Toptal, not Fiverr/Upwork)

- Fiverr/Upwork are open marketplaces — anyone can list themselves, which is why they're noisy and race-to-the-bottom on price. That's the opposite of the prestige/exclusivity positioning already decided in §1/§7.
- Toptal is the right analogue: reject the vast majority of applicants, then sell companies on "skip your screening, we already vetted these people harder than your interview loop would." The certificate *is* the vetting.
- Start with **placement/referral only**: introduce vetted, certified people to hiring companies, take a placement fee. Low operational burden — no escrow, no payment disputes, no contracts infrastructure.
- Explicitly defer any **gig/marketplace** model (companies post projects, certified people bid, platform handles payments/escrow/disputes/1099s) — that is its own separate business with its own build, not a bolt-on feature. Revisit only once placement has proven demand.

### 9.2 Validation before building platform tech

- Concierge-first: manually introduce the first cohort of certified graduates to a small number of pilot companies, before writing any platform/matching code for this layer.
- The question being tested: will companies actually trust and hire off the certificate? If not, no amount of platform tooling fixes that — the credibility problem is upstream (§2, §6).

### 9.3 Company-side value proposition

Sell companies on the differentiation thesis directly (§2.6): not a talent pool, a judgment-vetting layer. The pitch is "we test for catching AI mistakes, making tradeoff calls, and critically evaluating output — the things a normal resume or interview loop doesn't test for, and the things that matter more as AI-generated work volume increases in your org."

## 10. Open Decisions (need your input before implementation planning)

- **Platform**: iOS/Android native, or cross-platform (React Native, given your Syncbook experience)? Web app? Which is primary launch target?
- **Game engine/tech**: native game framework vs. something like Unity, vs. a lighter custom canvas/game-loop built in RN/web?
- **Video generation**: is Grok's video capability confirmed accessible for this use case — has this been tested, or does it need a feasibility spike first?
- **Team/solo**: are you building this solo with me, or is there a team (content, design, backend)?
- **Timeline**: how many weeks realistically available, and is there a target launch trigger (event, cohort, waitlist size)?
- **Invite system infra**: how are invites generated/tracked — simple codes, waitlist + manual approval, referral chains?
- **Certificate verification infra**: hosted verification page, PDF + QR, or integration with an existing credentialing standard (e.g. Open Badges)?

## 11. Proposed Phase Breakdown (to refine once §10 is answered)

- **Phase 0 — Feasibility spikes**: confirm Grok video generation works for the Feynman-clip use case; validate core game loop is fun standalone (paper prototype or minimal build).
- **Phase 1 — Core game loop**: build the standalone casual game (no learning layer yet), get it feeling good.
- **Phase 2 — Learning layer**: wire in Feynman video unlock, one-tap checkpoint, topic content for one path (pilot with ~5-10 topics), all designed against the judgment-based assessment principles in §2.4.
- **Phase 3 — Boss levels + certificate**: judgment-scenario assessment engine, certificate issuance + verification.
- **Phase 4 — Access system**: invite codes, paid gating, waitlist.
- **Phase 5 — Second curriculum path + polish**: expand from pilot path to second path, leaderboards, streaks, rotating topics.
- **Phase 6 — Placement layer (concierge)**: manual introductions of first certified cohort to pilot hiring companies (§9.2), no platform tech yet — validate companies will actually hire off the certificate.
- **Phase 7+ — Platform/hiring tech** *(only if Phase 6 validates)*: build matching/placement tooling for companies to register and find certified people (§9). Marketplace/gig model explicitly out of scope unless revisited later.
