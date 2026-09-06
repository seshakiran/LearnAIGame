# Story campaigns

## Play Last Train

Open `UnityProject` in Unity 6000.0.81f1, open `Assets/Scenes/Bootstrap.unity`, and press Play. The existing GameLoop component starts the story player. No scene rewiring is needed.

Last Train is a complete text-led first campaign: six chapters, 25 story beats, 18 decisions (15 coached, 3 held-feedback assessment decisions), original evidence records, character responses, three briefing outcomes, and a persistent evidence desk. Camera footage and voice messages are represented as authored transcripts and observations; they are not filmed cinematics. Existing local music is optional. All locations, records, and events are fictional.

The player opens every source before committing a choice. An answer is locked and saved before feedback, so closing the app cannot erase it. Wrong calls receive an in-world correction and an explanation. Final briefing feedback is held until the ending, including in the evidence notebook. Restarting requires confirmation. The campaign ends rather than cycling back to topic one.

## Architecture

- `StoryData.cs`: serializable campaign, chapter, beat, evidence, choice and save models.
- `StoryGameController.cs`: scrollable responsive UI, evidence review, progress, debrief and endings.
- `Resources/last_train.json`: all story copy and choices, separate from the Unity UI.
- `StoryProgress`: versioned, campaign-specific PlayerPrefs save. Incompatible/corrupt saves restart safely. This is local storage, not a secure credential record.
- `validate_story_campaign.py`: checks unique IDs, usable evidence, decision completeness and ending reachability.

Current branch scope: authored immediate consequences, a conditional follow-up in chapter one, a practice-dependent closing response, and three final-briefing outcomes. The six-chapter spine converges; this is not a fully branching cinematic production. Later content can extend the schema with explicit prerequisite flags and graph navigation once needed. Existing prototype card content is retained but no longer the launch flow.

## Authoring another story

1. Start with a human problem, recurring characters, an escalating threat, and an actual resolution.
2. Write the truth timeline before writing AI outputs. Keep character knowledge separate from author knowledge.
3. Select learning goals appropriate to the player's prerequisites. Each question must be answerable using supplied evidence.
4. Create a `StoryCampaign` JSON with stable IDs. A chapter has `title`, `time`, `location`, `concept`, `summary`, and ordered `beats`.
5. A scene-only beat has empty `choices`. A decision beat includes `question`, original `evidence`, choices, a concept, and a lesson. Each choice has its own response and consequence. Use `afterSuccess` / `afterError` for a follow-up to the previous decision.
6. A supported answer must be defensible from the original records. Never use ethnicity, clothing alone, writing style, or a synthetic-media “tell” as proof of responsibility or origin.
7. Reserve fresh records for assessment. `assessment: true` seals feedback until the final debrief. The current ending screen assumes three assessment decisions; generalize this before adding campaigns with a different assessment design.
8. Bump `version` when changing ordering or answer meaning; old saved decisions must not be silently applied to new content.
9. Validate data, compile in Unity, play supported and mistaken routes, test restart/resume, and inspect mobile layout. Add a campaign selector when the second authored campaign is available; the current entry loads Last Train.

## Curriculum expansion (planned, not implemented)

Keep the premise accessible. Introduce a concept after the player experiences the problem it helps explain. Do not claim mastery of a technical subject from one binary decision.

| Stage | Story premise | Concepts and deeper mechanics |
| --- | --- | --- |
| Foundations / Last Train | A transit incident and misleading evidence | Generated claims, task framing, confidence, source verification, bias, RAG, injection, permissions |
| Retrieval / The Missing Archive | A journalist reconstructs a vanished investigation | Embeddings, chunking, retrieval recall, reranking, source conflicts; rebuild and test an evidence search |
| Agents / The Silent Handover | An overnight operations team inherits an autonomous workflow | Tools, state, memory, planning, approval; inspect and repair an action trace |
| Multi-agent / The Echo Room | Multiple investigators reach the same false conclusion | Shared-source errors, coordination, independent checks; compare agent traces and resolve conflicts |
| Evaluation / Redline | A product team faces a launch deadline after a hidden failure | Datasets, leakage, metrics, red teaming, distribution shift; design tests and weigh measured results |
| Advanced / The Last Release | A consequential model rollout begins to drift | Fine-tuning versus retrieval, uncertainty, monitoring, rollback, cost/latency and governance; defend deployment tradeoffs |

Advanced episodes need richer interactions and authored rubric-based explanations rather than simply harder two-choice questions. Certificates, accounts, delayed transfer checks, and professional assessment remain separate future work.

## Validation in this implementation session

- Story content validation passed: 6 chapters, 25 beats, 18 decisions, 32 evidence records, all three final outcomes reachable.
- All runtime C# scripts compiled against the installed Unity 6000.0.81f1 assemblies with Roslyn. One existing unused-field warning remains in SwipeCardView.
- Full Unity batch / Play-mode validation is blocked by the local Unity Licensing Client failing to initialize. Mobile layout, real input, audio, and save/resume need an Editor playthrough after licensing is restored.
- The older `validate_learn_ai_game.py` reports pre-existing 35–45-word-budget failures in three legacy topic scripts; those unused topic assets were not changed by this campaign.
