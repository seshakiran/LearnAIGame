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

1. **Play** — quick casual round of swipe-to-judge cards (§3.1), zero learning framing visible, just fast right/wrong reflex calls. Must stand alone as fun.
2. **Micro-unlock trigger** — hitting a score/streak threshold pauses the game on a win, not a loss.
3. **Feynman moment** — 15s video, simple explanation of the AI concept, tied to the judgment call just made in the swipe burst (e.g., the round was about spotting hallucinations; the video explains why models hallucinate). Pre-generated and cached per topic (via Grok, in the content pipeline — not generated live at unlock time, to avoid killing reward-timing with latency).
4. **One-tap checkpoint** — single fast question, same swipe-card format as gameplay, not a separate quiz UI. Gate is felt in seconds, not as homework.
5. **Payoff** — game snippet/skin/level unlocks immediately. Loop back to step 1.
6. **Boss level** (every 3–5 topics) — real multi-question assessment, harder, untimed, full-context scenarios (not card-constrained). This is what the certificate actually vouches for — it needs real rigor, not a rubber stamp.

### 3.1 Core Mechanic: Swipe-to-Judge

Two-alternative forced choice, swipe left/right, instant reveal-and-explain. One system reused everywhere — only the content slot changes per topic/path. This mechanic is not a reskin: the act of judging right-vs-wrong-fast is a direct, simplified rehearsal of the exact skill the certificate later tests at depth (§2.4) — the casual game trains the fast/intuitive version, boss levels test the slow/deliberate version of the same skill.

**Locked card types:**
- **"AI or Human?"** — a short text/code/image snippet; swipe AI-made vs. human-made, reveal with a one-line tell. Precedent: "Which Face Is Real," "AI or Not" apps/trends — already culturally primed to be shareable ("I got 8/10").
- **"Real answer or confident hallucination?"** — two AI-generated answers to the same question, one correct, one fluent-but-wrong; swipe to pick which you'd trust. Most direct rehearsal of §2.4's core target skill. Precedent: Cambridge's "Bad News" game (peer-reviewed in Palgrave Communications, measurably improved real-world misinformation detection) — the strongest evidence available that this exact mechanic produces a transferable skill, not just entertainment.
- **"Ship it or don't"** — an AI-generated output shown with a specific constraint ("2 days, payments flow"), swipe ship vs. hold. PM-path-weighted; tests tradeoff judgment (§2.4).
- **"Would this bite you later?"** — a code/architecture choice that looks fine now; swipe fine vs. technical-debt bomb. Engineer-path-weighted version of the same tradeoff-judgment test.

**Content-density principle (cards are small, judgment content is not always small — resolve by architecture, not compression):**
- **Hard content budget per card**, enforced in the content pipeline (§5), not fixed later in UI: one sentence, one 3-5 line snippet, or one image. If a scenario needs more context than that to judge fairly, it does not belong on a card — it belongs in a boss level.
- **Recognition over reading**: consistent visual template per card type (same layout every time), syntax highlighting, icons, color-coding — returning users pattern-match the format instantly instead of reading fresh each time.
- **Two-layer depth**: the card is the fast System-1 gut-check; the one-line "why" lives in the reveal-after-swipe, once the user isn't time-pressured anymore (predict-then-explain — testing before teaching improves retention).
- **Genuinely ambiguous/context-heavy judgment calls are deliberately excluded from card format entirely** and reserved for boss levels, which are untimed and full-screen. The swipe layer and the assessment layer must not be made to do the same job.

### 3.2 Worked Example: "Why AI Lies With Confidence" (Hallucinations Topic)

Illustrates the full cadence: micro-loop runs every few minutes per topic; boss level is a separate, slower gate every 3-5 topics; leaderboard/shareable result runs continuously in parallel to both.

**1. Swipe burst (~15 sec, 6-8 cards, "Real answer or confident hallucination?" format)**
- Card: *"When was the Eiffel Tower built?"* — two AI answers flash: **A)** "1889" **B)** "1887, based on Gustave Eiffel's original blueprint filed in 1885." Swipe the one you'd trust.
- Reveal: "A is correct. B *sounds* more credible because it's more specific — that's the trap. Models add plausible-sounding detail, not more truth."
- Following cards repeat the pattern with new examples (a fake citation, a made-up statistic, a real one), timer ticking, combo streak building.
- Burst ends: **"7/8 correct — 4 day streak"** — Wordle-style shareable result card (§8), postable outside the app.

**2. Micro-unlock trigger** — score threshold hit, game pauses on the win screen, not mid-burst.

**3. Feynman video (15 sec)** — *"An AI isn't looking up facts — it's predicting the most likely-sounding next words. That's why 'Gustave Eiffel's blueprint filed in 1885' sounded so believable in the card you just swiped — it's not lying on purpose, it's just really good at sounding right."* Directly references the card just swiped.

**4. One-tap checkpoint** — one new card, same format: *"Real quote or fabricated one?"* — single swipe, instant.

**5. Payoff** — next tile on the skill-tree map lights up, small cosmetic/skin unlock. Loop back to step 1 with the next topic (e.g. "Bias in Training Data").

*...micro-loop repeats for topics 2, 3, 4 (bias, RAG, prompt injection)...*

**6. Boss level (after topics 1-4 are done)** — different screen entirely, no swiping, untimed, full context allowed:
> *"Your team's support bot told a customer their refund was denied, citing 'section 4.2 of the return policy' — a section that doesn't exist. Users didn't catch it and escalated. What's the actual root cause, and what would you change in the system (not just the model) to prevent this recurring?"* — pick the best of 4 detailed options, or short-justify.

Passing it unlocks a certificate milestone ("Foundations of AI Judgment — Certified") that rolls up toward the full path certificate, plus the next skill-tree branch.

**7. Leaderboard** — runs the whole time in the background: daily board shows swipe-burst streaks/scores, weekly board shows "topics mastered" (curriculum-tied, per §8), separate from and slower-cadence than the daily shareable result card.

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
- **Wordle-style shareable result card**: after each swipe burst, a lightweight "you got 7/10, streak: 12 days" result card, shareable outside the app. Distinct from the certificate — this is the *daily* viral loop (low stakes, frequent), while the certificate is the *prestige* viral loop (high stakes, rare). Wordle's virality came from the shareable result grid, not the puzzle itself — same principle applies here.
- **Leaderboards paired with the shareable result**: daily/weekly rank alongside the streak, so sharing a result also implicitly shares standing — gives people a reason to want to be seen playing, not just to have played.

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

## 10. Tech Stack Decisions

### 10.1 Locked

- **Platform**: mobile-first, both iOS and Android. A marketing web page directs prospects to the app; the app itself is where the product lives.
- **Game engine**: **Unity**. Single codebase for iOS + Android, proven for casual match/merge mechanics (particle effects, tweening, physics, juicy game-feel), mature IAP/ads/analytics plugin ecosystem for paid + invite gating. Covers the full learner-facing app (game loop, video unlocks, checkpoints, boss levels, certificate display) through Phase 0-5.
  - Architectural note: Unity is not the right tool for the company/hiring-facing side. §9's Platform & Hiring Layer (Phase 6-7) should be a **separate web dashboard**, not built inside Unity — company users searching for talent are desktop/business users, and that's an admin/search surface, not a game surface. The web page that directs prospects to the app becomes the same web layer's home for the future company portal.
- **Video content pipeline**: Grok video generation is **HITL-controlled, not on-the-fly** — clips are pre-recorded and uploaded to a hosting location ahead of time. This confirms the §5 design assumption (cached, not live-generated at unlock time) — no latency risk to the reward-timing in the core loop. Unity streams/plays clips from wherever they're hosted (CDN/cloud storage) via URL.

### 10.2 Locked (round 2)

- **Team**: solo (user + Claude Code).
- **Timeline**: 1 week for Phase 0 (§12 spikes). Aggressive — scope is deliberately gray-box/minimal per §12, not production-polish.
- **Access model confirmed**: invite-only (reconfirms §7, no change).
- **Video/content hosting infra**: AWS — S3 bucket for hosting the HITL-produced Feynman videos (§12.2), Unity streams from S3 URLs.

### 10.3 Still open (lower priority, not blocking Phase 0)

- **Invite code generation/tracking mechanics**: simple codes vs. waitlist + manual approval vs. referral chains — needed for Phase 4, not Phase 0.
- **Certificate verification infra**: hosted verification page, PDF + QR, or an existing credentialing standard (e.g. Open Badges) — needed for Phase 3, not Phase 0.
- **Backend for Phase 1+**: Unity is the client — will need an API for content delivery, checkpoint/assessment tracking, invite codes, and certificate issuance eventually. Not required for the Phase 0 spikes themselves (Spike A can run with local/hardcoded data; Spike B only needs S3, not a full backend).

## 11. Proposed Phase Breakdown (to refine once §10.2 is answered)

- **Phase 0 — Feasibility spikes**: set up the Grok HITL video pipeline (record → review → upload → host) end to end for one topic; validate core game loop is fun standalone (paper prototype or minimal Unity build).
- **Phase 1 — Core game loop**: build the standalone casual game in Unity (no learning layer yet), get it feeling good.
- **Phase 2 — Learning layer**: wire in Feynman video unlock, one-tap checkpoint, topic content for one path (pilot with ~5-10 topics), all designed against the judgment-based assessment principles in §2.4.
- **Phase 3 — Boss levels + certificate**: judgment-scenario assessment engine, certificate issuance + verification.
- **Phase 4 — Access system**: invite codes, paid gating, waitlist.
- **Phase 5 — Second curriculum path + polish**: expand from pilot path to second path, leaderboards, streaks, rotating topics.
- **Phase 6 — Placement layer (concierge)**: manual introductions of first certified cohort to pilot hiring companies (§9.2), no platform tech yet — validate companies will actually hire off the certificate.
- **Phase 7+ — Platform/hiring tech** *(only if Phase 6 validates)*: build matching/placement tooling for companies to register and find certified people (§9). Marketplace/gig model explicitly out of scope unless revisited later.

## 12. Phase 0 Spike Spec

Two independent spikes, testing two different risks. Neither depends on the other and they can run in parallel.

### 12.1 Spike A — Core loop feel

**Question it needs to answer:** is the swipe-to-judge micro-loop (§3, §3.1) actually fun standalone, and does the AI content feel integrated rather than bolted-on (the "jarring" risk raised earlier in design)?

**Build:** minimal/gray-box Unity prototype implementing one full micro-loop pass using the §3.2 worked example ("Why AI Lies With Confidence"):
- Swipe burst of 6-8 "Real answer or confident hallucination?" cards, using the Eiffel Tower example plus a few more written to the same pattern.
- Timer + combo streak scoring, win-screen pause on threshold.
- Feynman video slot (placeholder video/voiceover acceptable if Spike B isn't done yet — the point is to test the *loop*, not final video production quality).
- One-tap checkpoint card.
- Payoff screen (can be a stub — a single skill-tree tile lighting up is enough).
- Shareable result card (§8) — even a static mockup of the share image is enough to test reaction.

Art/animation polish is explicitly not required — this is about mechanic feel and pacing, not visual fidelity.

**Explicitly out of scope for this spike:** boss levels, leaderboards (beyond a stub), invite/paid gating, certificate issuance, second topic/path, backend integration. None of these are needed to answer the core question.

**Success criteria:** run with a handful of real external testers (not just the builder — self-testing can't validate "does this feel fun" or "does this feel jarring"). Ask two separate questions after the session: (1) did the swipe burst feel fun on its own, before any AI framing was explained, and (2) did the video/checkpoint feel connected to what was just played, or did it feel like an ad interrupting a game. A "no" on either is a real signal to revise the mechanic or the content-tie-in, not just tune numbers.

### 12.2 Spike B — Grok HITL video pipeline

**Question it needs to answer:** what does the actual production pipeline (record → human review → upload → host) look like end-to-end, and how long does one topic's video realistically take — this determines how many topics can be ready for a real launch.

**Build:** produce one real Feynman video for the hallucinations topic (§3.2, step 3 script) through the full HITL Grok pipeline, upload it to a hosting location, and confirm Unity can stream/play it from that URL.

**Success criteria:** the pipeline works end-to-end without manual workarounds, and there's a real time-per-topic estimate to plan content production against for Phase 2 onward.

## 13. Learning Design & Assessment Architecture (Manus handoff, 2026-08-16)

`The Design Answer.md` (repo root) is a Manus-authored build handoff that extends this plan's learning design and is the source of truth for it going forward — treat it alongside this plan, not as a replacement. It answers a direct challenge raised in review: does swipe-judging isolated cards actually transfer to real-world AI use, or does it just train people to be good at the game? Its answer, in short:

- **Four scoring layers, not one quiz**: Reflex play (the existing swipe burst — momentum/cosmetics only, never credential evidence) → Decision Trace (Pin risk → choose control → commit an if-then cue-action plan, 60-90s) → Case File (fuller ambiguous scenario, 2-3 min) → Boss operation + a delayed transfer check 2-4 weeks later (the only layer that counts as credential evidence).
- **Decision Trace is the next build item** and is the one mechanism that reaches outside the game into a player's own daily AI use (research-backed via implementation-intention studies, cited in the doc's §18) — build one per existing topic before writing more card content.
- **Credential integrity**: practice and assessment card pools must never overlap; boss rationale is scored against a rubric, not by unvalidated LLM auto-grading — plan to hand-grade the first cohort's boss attempts before building any grading pipeline.
- **Locked palette/card format** in the doc's §5 and §9 match what's already implemented in `GamePalette.cs`/`SwipeCardView.cs` — no visual rework needed, this documents the current state as the standard.
- Full build order is in the doc's §14; explicitly out of scope for this build stage: payment/invite codes, certificate issuance, leaderboards-as-credential, multiple curriculum paths, live LLM-generated content, full backend.

**Doc inventory / corrections** (checked 2026-08-16): the visual spec and interactive mockup referenced in the handoff's §16 (`design-reference/swipe_judgment_card_visual_spec.md`, `design-reference/swipe_judgment_card_mockup.html`) did not exist in the repo as standalone files — they were extracted from `Manus.zip` and added at those paths. Two other referenced docs, `research/learning_model_red_team_assessment.md` and `research/game_learning_design_addendum.md`, do not exist anywhere (not in the repo, not in `Manus.zip`) — they were never actually delivered; get them from Manus before relying on anything the handoff attributes to them. The content-pipeline doc paths in §16 (`content-pipeline/SPIKE_B_VIDEO_PIPELINE.md`, `content-pipeline/briefs/hallucinations_video_brief.md`) also don't match their actual locations (`content-pipeline/docs/spike-b-video-pipeline.md`, `content-pipeline/briefs/hallucinations.md`) — same files, just resolve by content not by the literal path in the handoff.

### 13.1 Ideas borrowed from `ai-garden` (sibling project, github.com/seshakiran/ai-garden)

A related but differently-scoped project — it's an open, broad, un-gated "learn all of AI from basics to frontier topics" game, which is the opposite of LearnAIGame's invite-only, narrow, judgment-only positioning (§1-2) and exactly what the Manus handoff's primary instruction above says not to become. Borrow mechanics/UX patterns from it, not its scope or curriculum breadth:

- **World-map progression screen** (`ai-garden-world-map-spec.md`): an Angry-Birds-style zoomable path with themed world islands, standard/review/challenge/boss node types, and side paths for optional review — a much more concrete reference for the "skill-tree" payoff screen than anything currently in this plan or in `TopicPathScreen.cs`.
- **Data-driven concept/level schema**: every concept and level defined as data (id, explanation, analogy, prerequisites, difficulty, mechanic type, world placement, failure patterns, review hooks) so new content doesn't require app-shell changes — same direction as the topic-catalog work already planned in §13 above, worth aligning the schemas if practical.
- **A named spaced-review mode** (their "Tile Archive" — mahjong-style matching of concept/failure-mode/fix): a concrete delivery mechanism for the misconception-resurfacing loop the Manus handoff calls for (`misconception_reappeared`/`misconception_resolved` events, §13.2 of the handoff) but never specifies a UI for.
- **A named adversarial mode** (their "Break the Bot" — deliberately try to fool/stress-test a system): good framing to reuse for the prompt-injection topic's "Trust Boundary" case mechanic (handoff §8.2) — same idea, better player-facing name.
- **Strict no-truncation UX rules** (`ai-garden-mobile-screen-spec.md`): no label/summary text may end in an ellipsis; layouts must stack/wrap/shrink spacing before clipping text. Directly relevant given this session's own off-screen/overlap bugs — worth adopting as an explicit rule here, not just fixing case-by-case.

### 13.2 Grounded-incident framing (Deep Trace fusion, locked 2026-08-16)

Fused in from `deep-trace` (sibling project, `~/Downloads/Projects/deep-trace`, private GitHub backup at `github.com/seshakiran/deep-trace`) — a story-driven mobile game the same author built independently, with its own baseline spec, decision log, and a triage vertical-slice build spec worth reading directly for the full reasoning. Two decisions from it are now **locked as the standing foundation for every LearnAIGame topic going forward**, not a one-time reskin of the 4 existing topics:

- **Every topic is authored as one continuous incident, not a bare card burst with a title slapped on it.** The first pass at this got corrected mid-session: Deep Trace's own decision log explicitly *rejected* a single static text-heavy briefing screen, and the first implementation was exactly that mistake (one paragraph screen, then 8 cards that were still disconnected trivia/vignettes with zero throughline). The corrected version: `CardBurstData` carries `caseCode`/`caseTitle`/`skillLine`/`missionLine`/`incidentHeadline`/`stakesLines`/`meterLabel`/`meterStart`/`memoryLine`/`reflectionPrompt` — all short, scannable lines, no paragraphs — rendered as two terse sequential beats (`CaseBriefingScreen`'s Mission Briefing then Cold Open) before the swipe burst, and every card's `prompt`/`explanation` in all 4 existing topics was rewritten so the 8 cards + checkpoint are sequential items in **one** incident's queue (a research desk's shift, a company-wide fairness audit, a retrieval incident review, a security incident response) rather than 8 unrelated one-off scenarios. This is the standard future topics (agents, multi-agent coordination, tool use, planning, evaluation, etc. — see `deep-trace`'s `docs/deep-trace-memory-lines.md` for a curriculum-scale list of candidate incidents) must follow: pick one incident, one throughline role for the player, and author every card as a queue item inside it — not a title screen bolted onto otherwise-generic content.
- **A live reactive incident meter replaces plain streak-only scoring during the burst** (Deep Trace's "Queue Pressure" pattern, implemented in `BurstHeader.ApplyMeterDelta`) — deltas are asymmetric on purpose (wrong swipe costs more than a right swipe earns back), reinforcing the "you are supervising a system, not taking a quiz" framing that's also the core of the Manus handoff's Decision Trace layer (§13 above). It is a soft, cosmetic HUD signal, not a hard fail state — same non-gating treatment Deep Trace itself uses.
- **Reveal now asks, not just tells** — `reflectionPrompt` (Deep Trace's "post-case prompt" pattern) renders as a short reflective question under the memory line/explanation, e.g. "What made a draft trustworthy — or not?" — instead of only restating facts.
- **The case reacts to the player, it doesn't just track them.** Follow-up correction: a live meter that only sits in the header as a color-coded number is still a themed quiz, not a game — the user's words: "There is no story, there is no game. Disconnected questions that is not related to a story is useless progress." Two additions close that gap, both driven entirely by the player's own swipe outcomes, not a fixed script: (1) `ShowRevealPanel` now prints a per-swipe consequence line ("{meterLabel} holds at {value}." / "{meterLabel} slips to {value}.") under every non-checkpoint explanation, so the meter reads as the case responding to *this* decision, not a static sidebar stat; (2) `CardBurstData.escalationLine` fires exactly once, mid-burst, the first time `BurstHeader.MeterValue` crosses into its danger zone (>=75) — a full-screen "CASE UPDATE" interstitial (`GameLoopController.ShowCaseUpdatePanel`) voiced as an in-world character messaging the player (e.g. hallucinations: "Priya from the desk just messaged: 'Legal is asking why Client Trust is dropping. Slow down and check these.'"). Escalation only fires if the player's own choices actually drove the meter that high — a player judging well the whole burst never sees it, which is what makes it feel reactive rather than scripted.
- **Capture Deep Trace's actual game plot, not just its beat structure.** Third-round correction: the first two passes fixed the screen grammar and the reactivity, but each topic still invented a disconnected one-off corporate identity for the player ("the last reviewer standing between the desk's AI drafts and the client's inbox" — confusing on its face, and the user called it out directly). Read `deep-trace-baseline-spec.md` and `deep-trace-case-01-false-alarm.md` directly (not just the triage build spec) to pull the actual plot device: the player is a single investigator working numbered case files for one ongoing unit, with a persistent rank/XP that carries across cases — not a new job title invented per topic. Fixes: (1) every `missionLine` now opens "You've been assigned to Case 0X: ..." so the four topics read as case files worked by the same investigator, not four unrelated jobs; (2) `RankTracker` (new, `PlayerPrefs`-backed, same pattern as `StreakTracker`) accumulates XP and a rank title (Rookie Analyst → Chief Investigator) across every case closed, session to session; (3) `GameHomeScreen` (new) is a one-time, app-launch-only title beat — LearnAIGame wordmark, tagline, current rank, and a featured-case card with an "Open Case →" CTA — replacing the previous cold start straight into "Your Path" with no sense of arrival, matching Deep Trace's actual Home screen; (4) `ShowPayoffStub` is now a "Case Closed" stamp screen (XP earned, rank total, "Next Case: {title}" teaser) instead of a "Skill-tree tile lit up (stub)" placeholder. **Visual assets**: checked for reusable art from `deep-trace`'s `design/` folder — the only real images that exist are two flattened Figma mockup composites (1536×1024, individual UI elements ~150-250px once cropped) and generic default Expo app icons; there is no production-resolution artwork, and this environment has no image-generation tool to create new art. Rather than ship a blurry cropped screenshot as "the" opening image, `GameHomeScreen` reuses Deep Trace's actual *information layout* (wordmark, tagline, featured-case card, single CTA) in LearnAIGame's own code-driven dark-navy/lime palette instead. If real hero art becomes available later (commissioned, or sourced from the original image-gen prompts in `deep-trace-baseline-spec.md`), it drops into `GameHomeScreen`'s card background without changing the flow.

**Scope decision**: the user explicitly chose to let LearnAIGame's topic scope keep growing over time (agents, multi-agent, etc.) rather than staying fixed at 4 topics. This does not conflict with the Manus handoff's instruction not to become a generic AI recall course (§13 above) — that instruction is about *assessment methodology* (judgment-over-recall, practice/assessment pool separation, rubric-graded transfer), which is unaffected by *topic breadth*. Both `ai-garden` and `deep-trace` independently converged on broad AI curricula; LearnAIGame can grow its topic list the same way as long as every new topic still goes through the four-layer scoring architecture and the grounded-incident framing above, rather than reverting to plain fact-recall cards.

## 14. Backlog — Deferred Features (not started, revisit later)

- **Persona-driven card content**: user selects/builds a persona and profile on first login; swipe-card question selection (and eventually boss-level scenarios) should be filtered/weighted by that persona rather than a single fixed topic set. Persona definitions (role-based vs. skill-level vs. something else), how many personas to start with, and how profile data maps to card selection are all still open — needs its own design pass before implementation. Interacts with §4 (curriculum paths) — may end up being the same mechanism as "paths," or a finer-grained layer on top of them.
- **Login & auth via Firebase**: first-run login screen supporting Google, Apple, X, Yahoo, and Email/Password, backed by Firebase Auth. Requires: a Firebase project (not yet created), provider credentials/app registrations for Apple (Apple Developer account) and X/Yahoo (developer portal app registration), and a decision on whether this replaces Spike A's "no backend" scope (§10.3, §12.1) or starts a new spike/phase. Profile data collected post-login likely needs Firestore (or similar) for storage, tied to the persona work above.
- **Adaptive path inferred from performance**: instead of (or ahead of) asking the user to self-select a persona, infer their path/skill emphasis from actual swipe-burst behavior — per-topic and per-card-type accuracy, response latency, streak — and use that signal to pick the next topic, adjust difficulty (subtler decoys as accuracy rises), and weight card-type mix within a burst. AI-literacy judgment is positioned as for everyone regardless of background (§2), so behavior is a better signal than a self-reported role. Needs: a `difficulty` field added to the card JSON schema, per-card `cardType` (currently only set at burst level), and a lightweight local signal model (same pattern as `StreakTracker.cs`'s `PlayerPrefs` usage) before any backend exists. Can be built and tested entirely within Spike A's current no-backend scope. Complements rather than replaces the persona-at-login item above — both stay in the backlog.

## 15. Next Steps (session end 2026-08-16, pick up here)

- **Play-test the last two commits in the Editor.** Everything in this session was reviewed by eye only — Unity was already open, so no batch compile was possible. Before building anything further, hit Play once and confirm: `GameHomeScreen` renders once at launch (title, tagline, rank line, featured-case card) and only once per app run; `RankTracker` XP/rank actually persists across a full topic loop (play two topics back to back, confirm XP accumulates rather than resetting); the "Case Closed" payoff screen's five stacked labels don't clip or overlap on the actual Editor Game View aspect; the `ShowCaseUpdatePanel` escalation interstitial actually fires when a burst is played badly on purpose (intentionally answer wrong until the meter clears 75) and never fires on a clean run.
- **Sanity-check the new `missionLine` copy out loud** ("You've been assigned to Case 0X: ...") across all 4 topics for the same awkward-phrasing problem that triggered this round's correction — read each one as if hearing it cold, not just checking it parses grammatically.
- **Decide on real hero art for `GameHomeScreen`.** Confirmed this session: `deep-trace`'s only existing images are two low-res Figma mockup composites and generic default Expo icons — nothing production-usable, and this environment has no image-generation tool. Options to raise with the user: (a) commission/source real illustration for the featured-case card background, (b) reuse the image-gen prompts already written in `deep-trace-baseline-spec.md`'s audio/visual sections as a starting point if an image tool becomes available, (c) stay code-driven indefinitely and lean harder into the palette/motion system instead of photographic art. Don't silently pick one — this was explicitly left open last session.
- **Extend the same investigator-plot consistency pass to the remaining screens** that weren't touched this round: `ShowLeaderboardScreen` and `ShowExplanationScreen` still speak in generic "Leaderboard"/"You learned: {topic}" voice rather than case-file voice (e.g. a leaderboard framed as "Other analysts on this case" rather than an unrelated global leaderboard). Not started — worth a pass once the core plot fix above is confirmed working in Play mode.
- **Once the above is verified, continue the topic-scope backlog** (§14) — persona-driven content, Firebase login, adaptive path inference — none of which are blocked by this session's work, but none should start until the Play-mode verification above is done, since several of them build on `RankTracker`/`GameHomeScreen` staying stable.


## 16. Story campaign implementation — 2026-09-06

The user approved contemporary thriller-led learning, beginning with Last Train: an NYPD transit-command analyst helps a team investigate objects luring commuters into a coercive offer. The Unity launch flow now opens this six-chapter authored campaign. See `docs/STORY_CAMPAIGNS.md` for implemented scope, content schema, validation limitations, and the planned progression from foundational to advanced AI stories. This supersedes the old repeating swipe-burst launch experience. Cinematic video production, new advanced campaigns, backend credentials and live AI are not implemented.
