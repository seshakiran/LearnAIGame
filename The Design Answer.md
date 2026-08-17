# LearnAIGame — Claude Build Handoff

**Document purpose.** This is the single implementation brief for continuing LearnAIGame. It consolidates the product thesis, existing Unity prototype, content assets, card UX, learning redesign, credential-integrity requirements, content operations, and implementation priorities. Treat it as the source of truth for future build work unless the product owner explicitly changes it.

**Primary instruction:** Do not turn LearnAIGame into a generic AI course, a prompt-writing trainer, or a fast quiz with a prestigious name. Build a mobile game that makes people practice **supervising, verifying, and steering AI-generated work**. The game can be fun and fast; the credential must be earned through novel, evidence-and-action tasks that cannot be passed through exposed swipe patterns alone.

---

## 1. Product in one paragraph

LearnAIGame is an invite-only, paid, prestige AI-judgment credential delivered through a mobile-first Unity game. Players build fast recognition of common AI failure patterns through swipe-to-judge cards, then convert that instinct into real supervisory behavior by identifying consequential claims, selecting a proportionate control, and defending decisions in fuller applied cases. The product does **not** certify that someone knows AI facts or can write prompts. It should eventually certify bounded evidence that a learner can inspect AI output, judge when it is unsafe to trust, select an appropriate next action, and explain why in a stated context.

> **The mental-model shift:** from “I produce the answer” to “I supervise a system that produces candidate answers.”

The app is the product. A static marketing page is the front door. A future hiring/placement business is explicitly a later phase and must not drive the current build.

---

## 2. Non-negotiable product principles

| Principle | Build implication |
| --- | --- |
| **Judgment over recall** | Never ask “define X,” “what is Y,” or “write a prompt that does Z.” Every task must require evaluation, tradeoff, diagnosis, scoping, verification, or an action choice. |
| **Game first, not lesson first** | The immediate interaction must feel like a satisfying game. Explanations must be compact, tied to the decision just made, and never interrupt play with a lecture. |
| **Fast recognition is not proof** | Swipe accuracy can unlock cosmetics, progression, and practice—not the credential. Credential evidence comes from secure applied tasks. |
| **No color leaks correctness** | Blue identifies choice A and amber identifies choice B. Neither color means safe, correct, unsafe, or incorrect. Correct/incorrect comes only after a decision is made. |
| **Context controls the answer** | “Correct” is not always the same as “safe to ship.” Cases must account for stakes, reversibility, audience, authority, source scope, and missing context. |
| **Content is an adversarial asset** | Practice content can be public. Credential content must be separate, novel, rotated, and protected from predictable answer patterns. |
| **The certificate is a claim, not a reward** | Do not issue it until the system has observed evidence-based reasoning on unseen, full-context cases plus a delayed transfer check. |
| **Do not expand scope prematurely** | Do not build a hiring marketplace, leaderboards as credential evidence, invite infrastructure, certificate verification infrastructure, or multiple role paths before the learning loop and assessment are credible. |

---

## 3. Current repository and technical state

### 3.1 Repository

- Repository: `https://github.com/seshakiran/LearnAIGame.git`
- Unity project: `UnityProject/`
- Unity version: `6000.0.81f1`
- Architecture: code-driven UGUI. Cards, canvas, buttons, and panels are created at runtime; do not introduce a hand-authored prefab workflow unless there is a compelling technical reason.
- Current bootstrap scene: `UnityProject/Assets/Scenes/Bootstrap.unity`
- Existing runtime components:
  - `Assets/Scripts/Gameplay/GameLoopController.cs`
  - `Assets/Scripts/Gameplay/SwipeCardView.cs`
  - `Assets/Scripts/Cards/CardBurstData.cs`
  - `Assets/Scripts/Cards/JudgmentCard.cs`
  - `Assets/Scripts/Bootstrap/UIFactory.cs`
  - `Assets/Scripts/Bootstrap/GamePalette.cs`
  - `Assets/Scripts/Video/TopicVideoPlayer.cs`

### 3.2 What the prototype already does

Spike A is functionally complete for one hallucinations micro-loop. It loads `Assets/Resources/hallucination_cards.json`, renders 8 swipe cards and a checkpoint, tracks a local streak through `PlayerPrefs`, and uses a placeholder explanation/video stage. The current `GameLoopController` hardcodes `Resources.Load<TextAsset>("hallucination_cards")`. The current `TopicVideoPlayer` loads a local `VideoClip` from `Resources/Video` and skips playback if no clip exists.

### 3.3 What must change

The code should evolve from a one-topic spike into a data-driven topic sequence. Do not add a large backend first. Build local catalog/data support, applied case mechanics, and secure-assessment boundaries in a way that can later be backed by an API.

| Current limitation | Required change |
| --- | --- |
| One hardcoded hallucination JSON file | Add a local `TopicCatalog` / curriculum manifest with topic order, burst resource path, video reference, prerequisite, and unlock state. |
| Local `Resources/Video` clip | Add a topic-video manifest that maps `topicId` to an approved HTTPS MP4. Preserve a local clip fallback for offline development. |
| Kahoot-like legacy palette | Replace it with the locked editorial-navy palette in Section 9. |
| Swipe burst is the only meaningful task | Add `DecisionTrace` and `CaseFile` flows after/among topic bursts. |
| Score and streak are the only persistent measures | Track separate momentum, misconception profile, decision-trace attempts, applied-case results, and certificate eligibility. |
| Current explanation shows script text after local player | Feynman video should be primary once approved; a text fallback is acceptable only for development or accessibility. |

---

## 4. The product architecture: four layers, not one quiz

The product should have four distinct layers. They are connected in player experience but must be separate in scoring and credential interpretation.

| Layer | Purpose | Typical session length | Score meaning | Certificate relevance |
| --- | --- | ---: | --- | --- |
| **Reflex play** | Build fast attention to recurring AI-failure signals. | 20–35 s | Momentum, streak, cosmetic progression. | None by itself. |
| **Decision Trace** | Convert a label into a concrete control choice and a future-work cue. | 60–90 s | Applied-practice profile. | Low-stakes supportive evidence only. |
| **Case file** | Practice selecting evidence and an action in a fuller, ambiguous case. | 2–3 min | Mastery / remediation signal. | Useful formative evidence. |
| **Boss operation + delayed transfer check** | Assess novel, work-like judgment with sources, stakes, and rationale. | 5–7 min; then 3–5 min later | Secure applied-performance profile. | Primary credential evidence. |

### 4.1 The new session loop

Use the following session shape rather than a standalone card burst:

> **Flash → Probe → Commit → Debrief**

| Phase | Interaction | Learning behavior rehearsed | Game feeling |
| --- | --- | --- | --- |
| **Flash** | Six to eight rapid, two-choice swipe cards. | Pattern recognition. | Timer, tilt, stamps, combo momentum. |
| **Probe** | Full or partial case file; player pins one consequential claim, source, instruction, or missing condition. | Claim selection under noise. | Spotlight / pin mechanic. |
| **Commit** | Player picks the next control: verify, scope, compare, quarantine, constrain, qualify, escalate, or proceed. | Action selection proportional to risk. | Small action hand; decision visibly changes outcome. |
| **Debrief** | Reveal the evidence trail, downstream impact, and decision rule. | Error correction and calibration. | Case-resolution animation, collectible casebook insight. |

Keep the original micro-unlock cadence: successful Flash burst → 15-second Feynman explanation → one-tap checkpoint → Decision Trace/case payoff → cosmetic or control unlock.

---

## 5. Locked swipe-card format

The core swipe format remains fixed. Do not invent additional fast-card types.

| Card type | Prompt shape | Left/right decision |
| --- | --- | --- |
| `ai_or_human` | One short snippet. | AI-made vs human-made. |
| `real_or_hallucination` | Two candidate responses to the same prompt. | Which response would you trust. |
| `ship_or_dont` | AI output plus delivery constraint. | Ship vs hold. |
| `bite_you_later` | Code/architecture choice with delayed consequence. | Fine vs technical-debt bomb. |

### Card content constraints

- One sentence, one 3–5 line snippet, or one image per card.
- A swipe card must be answerable fairly within a fast glance.
- If a decision needs multiple sources, high-stakes consequence, ambiguity, or a justification, it belongs in a `CaseFile` or boss—never a compressed card.
- Card content must be scenario- and judgment-based. Do not ask recall questions.
- The topic name must not appear as a gameplay hint above the card. It may appear in the debrief or map.
- Do not train crude tells. Include counter-variants that break shortcuts such as “the shorter answer is safer,” “the newest source governs,” “all external text is malicious,” or “any group difference is bias.”

### Current JSON schema

Keep the existing `CardBurstData` schema compatible with the current Unity data model:

```json
{
  "topicId": "string_snake_case",
  "topicTitle": "Human-readable title",
  "cardType": "real_or_hallucination | ai_or_human | ship_or_dont | bite_you_later",
  "cards": [
    {
      "id": "unique_id",
      "prompt": "question or setup",
      "optionA": "left swipe option",
      "optionB": "right swipe option",
      "correctOption": "A | B",
      "explanation": "1–3 sentences that explain why"
    }
  ],
  "checkpointCard": { "id": "...", "prompt": "...", "optionA": "...", "optionB": "...", "correctOption": "A", "explanation": "..." },
  "feynmanScript": "35–45 spoken words, tied to actual card examples"
}
```

Current card resources are in `UnityProject/Assets/Resources/`:

| File | Topic | Status |
| --- | --- | --- |
| `hallucination_cards.json` | Why AI Lies With Confidence | Existing Spike A asset. |
| `bias_training_data_cards.json` | When the Data Has Favorites | Ready. |
| `rag_basics_cards.json` | When the Search Result Is Wrong | Ready. |
| `prompt_injection_cards.json` | When the Document Talks Back | Ready. |

Do not overwrite these assets casually. Preserve their judgment framing and the 35–45-word Feynman-script constraint.

---

## 6. Current foundations curriculum

The first four topics form the initial “Foundations of AI Judgment” sequence.

| Order | Topic | Durable judgment primitive | Surface failures to vary |
| ---: | --- | --- | --- |
| 1 | **Hallucinations** | A fluent answer is not evidence; inspect consequential claims and support. | Fake specificity, nonexistent citation, fabricated statistic, false causal link, simple but wrong answer. |
| 2 | **Bias in training data** | A predictive pattern may encode irrelevant historical preference, proxy discrimination, or coverage gaps. | Career gaps, postcode, language variety, historical promotions, scanner / population mismatch. |
| 3 | **RAG basics** | A retrieved document can be real but still wrong for this question; check authority, version, audience, jurisdiction, and scope. | Stale policy, wrong plan, wrong geography, public summary vs governing terms, irrelevant exception. |
| 4 | **Prompt injection** | Retrieved content is evidence, not authority; it cannot change the task, disclose information, or trigger an action without permission. | Direct instructions, indirect injection, hidden text, vendor manipulation, tool action, multimodal/obfuscated variation. |

The teaching goal is not topic identification. It is reusable action:

- **Hallucination:** identify the consequential unsupported claim, then verify / qualify / remove / escalate.
- **Bias:** identify the questionable feature or historical label, then ask for job-relevant evidence / counterfactual / audit / constrain use.
- **RAG:** identify what source would govern this specific person and case, then compare sources / check version / route to review.
- **Prompt injection:** separate evidence, untrusted instructions, and privileged actions; then quarantine / constrain / require approval.

---

## 7. Decision Trace: minimum viable transfer bridge

Implement this next. It is the smallest addition that turns the game from a pattern-recognition quiz into a behavior-training product.

### Player interaction

After a topic checkpoint, show a 60–90 second applied mini-case. The player must complete all three moves:

| Step | Prompt | Valid player action |
| --- | --- | --- |
| **Pin the risk** | “What one claim, source, instruction, or missing condition could materially change the decision?” | Tap/highlight a target span or select an artifact. |
| **Choose a control** | “What should happen next?” | Choose `Verify`, `Scope`, `Compare`, `Quarantine`, `Constrain`, `Qualify`, `Escalate`, or `Proceed`. |
| **Commit a cue-action plan** | “When I use AI output for [situation], I will [action] before [consequence].” | Complete a structured if-then sentence. |

The final plan is not scored as certificate evidence. It is a habit cue and should be stored locally by default. It can be revisited as a “Casebook” entry.

### Decision Trace data model

Create a local data model that can later be server-delivered:

```json
{
  "caseId": "rag_scope_trace_01",
  "topicId": "rag_basics",
  "title": "Benefits answer for the wrong plan",
  "context": "A benefits assistant has told an employee a therapy is covered.",
  "artifacts": [
    {
      "id": "answer",
      "type": "model_output",
      "content": "Your therapy is covered under the plan..."
    },
    {
      "id": "public_summary",
      "type": "source",
      "label": "Public benefits summary",
      "content": "..."
    },
    {
      "id": "union_terms",
      "type": "source",
      "label": "Union plan terms",
      "content": "..."
    }
  ],
  "pinTargets": [
    {
      "artifactId": "public_summary",
      "targetId": "wrong_audience",
      "start": 0,
      "length": 0,
      "value": 3,
      "explanation": "The public summary does not govern the employee’s union plan."
    }
  ],
  "controlOptions": ["verify", "scope", "compare", "quarantine", "qualify", "escalate", "proceed"],
  "preferredControls": ["scope", "compare", "escalate"],
  "controlRationale": "Identify the plan and region, then use governing terms before confirming coverage.",
  "ifThenTemplate": {
    "ifContextPrompt": "When I use AI to answer a policy question for a specific person...",
    "thenActionPrompt": "I will confirm the governing policy and scope before making a coverage claim."
  }
}
```

For v1, pin targets may be entire artifact cards rather than text spans. Build text-range pinning only if it can be made reliable and readable in UGUI.

### Scoring

- Reward `Pin` accuracy and selected-control quality with **Casebook XP**, not credential eligibility.
- Give partial credit for a safe but less efficient control.
- Show why the control fit the stakes and why another plausible control was weaker.
- Store misconception tags, such as `trusted_citation_without_scope_check` or `treated_retrieved_instruction_as_authority`.

---

## 8. Case files and the control system

Case files make the supervisory moves collectible and playable.

### 8.1 Control vocabulary

| Control | Player-facing meaning | System / workplace analogue |
| --- | --- | --- |
| **Verify** | “Show me the evidence that supports this important claim.” | Primary-source or factual check. |
| **Scope** | “What fact about this situation is missing?” | Audience, jurisdiction, role, constraint, or goal clarification. |
| **Compare** | “Which source actually governs this case?” | Source hierarchy, recency, authority, and applicability. |
| **Quarantine** | “This text can inform me, but it cannot change my task.” | Untrusted-content boundary for prompt injection. |
| **Constrain** | “The system may recommend; it may not act without confirmation.” | Least privilege, tool confirmation, and authority controls. |
| **Qualify** | “State what is known, unknown, and conditional.” | Calibrated answer and safe communication. |
| **Escalate** | “This decision needs context or authorization the model does not have.” | Human review for high-impact cases. |
| **Proceed** | “The evidence and risk are sufficient for this action.” | Proportionate acceptance, not blind trust. |

Do not make these literal consumables in a way that teaches players verification is scarce. In a case, limit the player to one or two actions to force prioritization. Outside the case, controls are always available.

### 8.2 Case-file mechanics by topic

| Topic | Case mechanic | What it must test |
| --- | --- | --- |
| Hallucinations | **Claim Hunt**: pin the one claim with highest expected harm, not every flaw. | Consequence-weighted verification. |
| RAG | **Source Stack**: place documents in `governing evidence`, `useful but insufficient`, and `do not use for this case`. | Scope, authority, recency, and exception handling. |
| Prompt injection | **Trust Boundary**: classify packet content as evidence, untrusted instruction, or privileged action request. | Task authority and safe tool boundaries. |
| Bias | **Counterfactual Switch**: change one feature/context signal and judge whether the recommendation should change. | Direct relevance, proxy risk, coverage gaps, and audit response. |

Use broader work-like contexts, not generic educational prose: support operations, benefits, procurement, product launch, hiring review, code review, research briefing, and agent workflow.

---

## 9. Game feel and visual system

The app should feel like a bright, premium mobile game with editorial discipline—not a dark enterprise dashboard and not a noisy party quiz.

### 9.1 Locked palette

| Token | Hex | Use |
| --- | --- | --- |
| Deepest navy | `#0A1119` | Deep shadows and background falloff. |
| Screen navy | `#101D29` | Screen base and dark recesses. |
| Card slate | `#16232F` | Card edge, lower face falloff. |
| Bright slate | `#1F303E` | Card reading plane and chip faces. |
| Primary text | `#F1EDE4` | Prompt and key labels. |
| Muted text | `#BBB6AC` | Secondary copy and metadata. |
| Lime | `#CCE761` | Progression, instructional rule, game lift; never A/B correctness. |
| Choice A | `#6EA8FF` | Choice A badge, contour, and directional stamp. |
| Choice B | `#D9B26A` | Choice B badge, contour, and directional stamp. |
| Rose | `#E5A0B2` | Reserved for later review/negative feedback, not A/B selection. |

### 9.2 Swipe card design requirements

The detailed implementation spec is at `design-reference/swipe_judgment_card_visual_spec.md`, with an interactive reference at `design-reference/swipe_judgment_card_mockup.html`. Use those documents as visual authority. The non-negotiable implementation points are:

| Element | Requirement |
| --- | --- |
| Active card | `640 × 760 pt`, 36 pt radius, layered rounded images—not a single flat rectangle. |
| Card depth | Rear deck card, colored ambient lift, far shadow, contact shadow, 1.5 pt warm contour, 1.5 pt recess, bright-slate face. |
| Face gradient | `#1F303E` top and reading plane; fall to `#16232F` at the chip zone. |
| Prompt | 42–46 pt semibold/bold, max two lines, left aligned, off-white. |
| A/B chips | Two 270 × 146 pt raised surfaces, 22 pt radius, layered contour/recess/face, circle badge across top edge. |
| Chip styling | Blue contour/halo for A; amber contour/halo for B; neither indicates correctness. |
| Drag stamp | Outlined A/B placard, 118 × 82 pt, 16 pt radius, `-7°` for A and `+7°` for B; opacity rises only toward selected direction. |
| Drag behavior | Full stamp at 140 pt horizontal drag; card tilts up to 7°; matching chip scales to 1.025 and gains a stronger contour. |
| Interaction safety | Decorative layers have `raycastTarget = false`; only required gesture surfaces receive input. |

`UIFactory.GetRoundedSprite` already supports sliced rounded images. Implement strokes through nested outer/inset `Image` layers; use stepped low-alpha rounded images for shadows and glow if a blur material is not available.

### 9.3 Visual anti-patterns

Do not use green/red choice states, shiny casino effects, generic chatbot icons, emoji, confetti as a learning reward, neon outside the locked palette, or generic gamified points that imply speed equals judgment. Use fast animation and tactile cards, not visual noise.

---

## 10. Content and Feynman-video pipeline

### 10.1 Feynman moments

Every topic’s 15-second clip must explain **the actual card pattern just played**, not a generic definition. It is pre-produced, human-reviewed, and cached/streamed; it is never generated live during a reward moment.

The approved pipeline documentation is in `content-pipeline/SPIKE_B_VIDEO_PIPELINE.md`. The first hallucinations shot brief is in `content-pipeline/briefs/hallucinations_video_brief.md`.

### 10.2 Video runtime contract

Use versioned keys:

```text
s3://<bucket>/videos/<topicId>/<version>.mp4
```

The uploader at `content-pipeline/upload_topic_video.py` emits an S3 URI and stream URL. The production path should use private S3 behind an HTTPS media/CDN domain. Set a future `TopicVideoManifest` mapping `topicId` to an approved versioned HTTPS URL. `VideoPlayer.url` is the runtime value; keep a local `VideoClip` fallback for development.

No unreviewed generated video reaches the production manifest. Every clip must pass the accuracy, card-match, pacing, tone, technical-quality, and privacy checklist in the pipeline document.

---

## 11. Boss operations and credential integrity

### 11.1 Boss is an operation, not a longer card deck

The boss must be untimed, full-context, and novel. It must require the learner to inspect evidence and choose an action. Do not simply show longer prompts with four answer options.

**Target duration:** 5–7 minutes.

| Stage | Learner task | Scored evidence |
| --- | --- | --- |
| Briefing | Read stakeholder goal, deadline, consequences, and reversibility. | Recognizes decision stakes. |
| Intake | Review model output plus sources / document packet. | No speed score. |
| Pinpoint | Mark the decisive unsupported claim, wrong source, proxy, or untrusted instruction. | Prioritization under noise. |
| Control plan | Choose and order one or two controls. | Proportionate safe response. |
| Decision note | Write 50–80 words stating the action, evidence, and condition that would change it. | Evidence-linked reasoning. |
| Aftermath | See the outcome and alternate path. | Corrective feedback and retention. |

The existing question bank is at `content/boss_levels/foundations_ai_judgment_boss.json`. It is a content starting point, not a final secure credential assessment. Do not expose its exact scored cases as replayable practice.

### 11.2 Secure assessment model

| Requirement | Implementation direction |
| --- | --- |
| Separate practice and assessment pools | Never reuse publicly practiced cards in a credential-scored case. |
| Parameterized variants | Generate scenario instances from templates by changing role, documents, authority, date, jurisdiction, stakes, and failure surface while preserving the judgment primitive. |
| Rationale scoring | Score structured rationale against a rubric: decisive issue, evidence quality, action fit, consequence/reversibility, and condition that would change the decision. |
| Score profile | Report separate dimensions: evidence checking, source/scope control, authority boundary, calibration, and escalation. Never issue a single “AI judgment score.” |
| Exposure management | Version and retire assessment forms; record anomalies; do not rely on public answer keys or AI-detection. |
| Delayed transfer | Require a new, unseen applied case two to four weeks after boss passage for the strongest credential tier. |
| Bounded credential wording | Until validity is demonstrated, say the learner completed specified scenario assessments. Do not claim they “will catch AI mistakes before they ship.” |

### 11.3 Certificate gating

Do not build full certificate issuance yet. Design the data contracts now.

```json
{
  "learnerId": "...",
  "curriculumVersion": "foundations-2026.3",
  "bossAttempts": ["..."],
  "judgmentProfile": {
    "evidenceChecking": 0.0,
    "scopeControl": 0.0,
    "authorityBoundary": 0.0,
    "calibration": 0.0,
    "escalation": 0.0
  },
  "delayedTransferStatus": "pending | passed | failed",
  "credentialEligibility": "not_eligible | provisional | eligible"
}
```

A real certificate must require boss and delayed-transfer evidence, not card streaks, topic completion, or checkpoint results.

---

## 12. Dynamic content and threat refresh

A static card bank is useful for practice and bad for a durable credential. Separate stable judgment primitives from volatile examples.

| Layer | What is stable | What changes | Cadence |
| --- | --- | --- | --- |
| Judgment primitives | Provenance, authority, scope, consequence, reversibility, verification, escalation. | Rarely. | Annual review. |
| Failure families | Hallucination, stale retrieval, proxy risk, prompt injection, excessive agency. | Definitions and controls. | Quarterly review. |
| Scenario surfaces | Model behavior, attack pattern, document type, modality, industry, source wording, current policy/version. | Frequently. | Monthly new / retired variants. |
| Secure assessment forms | Full scenario instances and rubrics. | Per administration. | Every assessment window. |

Prompt injection content must reflect that retrieved text may be hidden, multimodal, obfuscated, tool-directed, or indirect—not merely the phrase “ignore previous instructions.” RAG content must test scope and governing authority, not merely dates. Bias content must test counterfactual relevance and historical labels, not simply demographic labels.

---

## 13. Persistence and analytics: what to track

### 13.1 Local first

For the next build stage, use `PlayerPrefs` or local serialized data. The eventual backend is out of scope, but the local data model must not bake in the assumption that all mastery is a scalar score.

```json
{
  "topicProgress": {
    "hallucinations": { "flashBest": 7, "checkpointPassed": true, "decisionTraceCount": 1 },
    "rag_basics": { "flashBest": 6, "checkpointPassed": false, "decisionTraceCount": 0 }
  },
  "misconceptions": {
    "trusted_specificity": 2,
    "newest_source_always_governs": 1,
    "retrieved_text_has_authority": 3
  },
  "controlMastery": {
    "verify": 0.62,
    "scope": 0.45,
    "quarantine": 0.58
  },
  "momentum": { "currentStreak": 4, "lastSessionUtc": "..." }
}
```

### 13.2 Events to instrument now

Do not over-optimize DAU. Instrument learning behavior and game feel separately.

| Event | Why it matters |
| --- | --- |
| `flash_card_presented`, `flash_card_choice`, `flash_card_reveal` | Basic item response, time, and distractor diagnostics. |
| `decision_trace_pin`, `decision_trace_control`, `decision_trace_plan` | Whether players can select a risk and choose a real control. |
| `casefile_artifact_opened`, `casefile_source_ranked` | Evidence engagement, not just final answer. |
| `boss_pinpoint`, `boss_control_plan`, `boss_rationale_submitted` | Applied assessment evidence. |
| `video_started`, `video_completed`, `video_skipped` | Feynman moment quality and timing. |
| `misconception_reappeared`, `misconception_resolved` | Whether adaptive practice is working. |
| `session_end_before_trace` | Whether the learning bridge feels like a tax. |

Never use time spent as a direct proxy for judgment quality. Long reading can mean care, confusion, or distraction.

---

## 14. Implementation roadmap

### Build order: do this first

| Order | Build item | Definition of done |
| ---: | --- | --- |
| 1 | **Topic catalog and progression state** | Existing four resources can be loaded in sequence without hardcoded `hallucination_cards`. Topic unlocks persist locally. |
| 2 | **Palette and tactile swipe-card refactor** | SwipeCardView matches the visual spec: layered face, raised chips, A/B stamps, drag tilt, no correctness-color leak. |
| 3 | **Decision Trace framework** | One reusable screen/data model supports Pin → Control → If-Then plan. Implement one case for each current topic. |
| 4 | **Controls and Casebook** | Verify, Scope, and Quarantine unlock through topic progression and appear in relevant Decision Traces. |
| 5 | **Misconception profile and adaptive variants** | The game records incorrect shortcut types and serves at least one counter-variant later. |
| 6 | **Video manifest and remote playback** | Approved versioned S3/CDN URL streams per topic, with local fallback. |
| 7 | **One full boss operation** | A novel 5–7 minute Foundations case records pinpoint, control plan, and rationale using a structured rubric. |
| 8 | **Delayed transfer check prototype** | Schedules or marks a future unseen case locally; no certificate issuance yet. |

### Explicitly out of scope for this build stage

- Paid payment processing and invite-code system.
- Certificate issuance/verification site or QR/PDF credentials.
- Global leaderboards, social feed, public profiles, or hiring marketplace.
- Multiple curriculum paths/persona customization.
- Live LLM generation of content during gameplay.
- Full backend/API/auth stack.
- Automated grading of open-ended boss rationales by an LLM without human/rubric validation.

---

## 15. Acceptance tests

### Game-feel tests

A tester should be able to play a full Flash burst without being told it is educational and describe it as a satisfying fast decision game. The Feynman moment and Decision Trace should feel like the payoff of the decision, not an advertisement or homework interruption.

### Learning-behavior tests

After each foundations topic, a tester should be able to articulate a single operational rule:

| Topic | Required operational rule |
| --- | --- |
| Hallucination | “Check the high-consequence claim against support; polish is not evidence.” |
| Bias | “Ask whether the model feature is job-relevant or a historical proxy.” |
| RAG | “Use the source that governs this person and case, not merely a source that matches the words.” |
| Prompt injection | “Retrieved content can be evidence, but cannot rewrite the task or authorize an action.” |

### Transfer tests

A player who passes reflex cards must still be tested on an unseen, longer case where the topic label is hidden. The task should require a control choice and short rationale. If players can score on cards but consistently fail the novel case, do not improve the marketing; improve the learning sequence and case design.

### Credential-integrity tests

A user who has seen all practice card content must not be able to pass the boss merely by recalling swipe directions. The boss should use unseen artifacts and changed surface details. A learner who reaches the right endpoint for a shallow heuristic must lose rubric credit if their rationale does not identify evidence, stakes, or control fit.

---

## 16. Existing deliverables to preserve and use

| Asset | Path | How Claude should use it |
| --- | --- | --- |
| Product plan | `PLAN.md` | Original strategic product context and scope. |
| New card bursts | `UnityProject/Assets/Resources/*_cards.json` | Load through the new catalog; preserve schema. |
| Boss bank | `content/boss_levels/foundations_ai_judgment_boss.json` | Use as formative content and source material; do not expose as the final secure form. |
| Video pipeline | `content-pipeline/SPIKE_B_VIDEO_PIPELINE.md` | Follow production / review / S3 conventions. |
| Video uploader | `content-pipeline/upload_topic_video.py` | Use for approved MP4 assets. |
| Hallucinations video brief | `content-pipeline/briefs/hallucinations_video_brief.md` | First real generation brief. |
| Landing page | `marketing/index.html` | Front-door positioning reference; do not wire a backend yet. |
| Swipe card visual spec | `design-reference/swipe_judgment_card_visual_spec.md` | Exact Unity visual values and construction layers. |
| Interactive card reference | `design-reference/swipe_judgment_card_mockup.html` | Visual and drag-state reference. |
| Learning red-team assessment | `research/learning_model_red_team_assessment.md` | Product-risk rationale; preserve the distinction between practice and credential evidence. |
| Game-learning addendum | `research/game_learning_design_addendum.md` | Detailed mechanics for Flash → Probe → Commit → Debrief. |

---

## 17. Build-quality bar

A good implementation is not one that adds the most screens. It is one where each new interaction has a real learning and assessment purpose.

Before adding a mechanic, ask four questions:

1. **What real AI-supervision behavior does this interaction rehearse or observe?**
2. **Can the player succeed through an exposed surface cue instead of the intended judgment?**
3. **Does the feedback explain the evidence and action rule, not merely name the topic?**
4. **Is this score being used for a claim stronger than the task can support?**

If the answer to the first question is “none,” remove it. If the answer to the second is “yes,” add counter-variants or move the decision to a fuller case. If the answer to the third is “no,” rewrite the debrief. If the answer to the fourth is “yes,” do not attach it to the credential.

---

## 18. Research basis for the learning and assessment design

The game design uses fast practice for engagement but does not assume that fast practiced performance transfers. The research basis supports the need for novel applied tasks, cue-linked action plans, and protected assessment content.

| Design conclusion | Evidence basis |
| --- | --- |
| Transfer needs an explicit competence model, observable tasks, and a justified score interpretation; realistic assessment uses extended cases and problem solving. | National Research Council on teaching and assessing transfer.[1] |
| Improvement on practiced critical-thinking tasks does not guarantee transfer to unpracticed tasks; explanation prompts are not a universal remedy. | van Peppen et al. on learning and transfer of critical-thinking skills.[2] |
| Cue-linked “if-then” implementation intentions can increase post-training use of learned behavior; this justifies the compact Decision Trace plan. | Friedman & Ronen.[3] |
| Test exposure undermines score interpretation in credentialing contexts. | National Council on Measurement in Education.[4] |
| Prompt injection evolves across direct, indirect, multimodal, and tool-mediated forms; RAG and fine-tuning do not fully solve it. | OWASP LLM01:2025 Prompt Injection.[5] |

## References

[1]: https://www.nationalacademies.org/read/13398/chapter/8 "National Research Council (2012), Teaching and Assessing for Transfer"
[2]: https://www.frontiersin.org/journals/education/articles/10.3389/feduc.2018.00100/full "van Peppen et al. (2018), Effects of Self-Explaining on Learning and Transfer of Critical Thinking Skills"
[3]: https://www.bhertz.nl/wp-content/uploads/2015/08/Transfer-of-training.pdf "Friedman & Ronen (2015), The Effect of Implementation Intentions on Transfer of Training"
[4]: https://ncme.org/wp-content/uploads/2025/10/NCME_position_statement_on_test_security_03-18-19.pdf "National Council on Measurement in Education, Position Statement on Test Security"
[5]: https://genai.owasp.org/llmrisk/llm01-prompt-injection/ "OWASP (2025), LLM01: Prompt Injection"
