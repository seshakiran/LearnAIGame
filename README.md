# LearnAIGame

Learn AI judgment through contemporary interactive thrillers built in Unity.

The first playable story, **Last Train**, puts you at NYPD transit command during a fictional incident involving mysterious objects and misleading messages. Investigate original records, brief the team, learn from consequences, and complete a final evidence-based assessment.

Open `UnityProject/Assets/Scenes/Bootstrap.unity` in Unity 6000.0.81f1 and press **Play**.

See [Story campaigns](docs/STORY_CAMPAIGNS.md) for gameplay, implementation, current limitations and the basic-to-advanced curriculum roadmap. [PLAN.md](PLAN.md) retains the earlier product design history.

Validate story content with `python3 content-pipeline/validate_story_campaign.py`.

The Chapter 1 playtest now includes branching leads and a timeline puzzle. Run `python3 content-pipeline/test_story_flow.py` for core flow tests. See [iOS beta handoff](release/TESTFLIGHT.md) for TestFlight preparation and remaining setup.
