# Last Train — iOS beta handoff

## Status

The campaign and iOS export tooling are prepared in source. **No IPA, verified Xcode export, TestFlight build, or invitation link exists yet.** The owner has not created an Apple Account. Unity Hub reports iOS support installed, but the selected Editor's `PlaybackEngines/iOSSupport` directory is missing in this execution environment. Unity licensing failed during earlier batch validation, but the open Editor was successfully refreshed and entered Play mode in this session. A simultaneous batch run was blocked by the project already being open. Xcode 26.4 is installed.

## Ready in the repository

- A short first-chapter playtest: choose camera or witness first; tap three timeline events into order; send a briefing; recover through a different scene after an unsupported claim; finish at a cliffhanger and optional playtest stopping point.
- Six chapters, comic illustrations, explicit learning goals and plain-language explanations. Chapter 1 permits selecting a source rather than requiring every document to be opened. Its lead choice is unscored; timeline and briefing decisions are scored.
- Local saves include committed answers and unfinished timeline order. Campaign version is now 2 because chapter structure changed; version-1 saves restart the campaign. No backward-compatible migration is claimed.
- iOS export script: IL2CPP, ARM64, device SDK, iPhone/iPad, portrait, minimum iOS 15, marketing version 0.2.0, an explicit build number and app identifier, and a generated app icon.
- An existing export is never deleted automatically. Exporting does not sign/upload by itself.

## Local build steps

1. Repair/add iOS Build Support for **6000.0.81f1** in Unity Hub and resolve the Editor license. Verify the actual module files and ability to enter Play mode, not just the Hub badge.
2. Run `scripts/ios-preflight.sh`. Missing Apple variables are expected until account setup.
3. Open Bootstrap.unity and complete the mobile QA below.
4. Set `LASTTRAIN_BUNDLE_ID` to the identifier registered for the app, `LASTTRAIN_BUILD_NUMBER` to an unused positive integer, and `LASTTRAIN_APPLE_TEAM_ID` to the signing team. These are identifiers, not passwords; keep credentials in Xcode/Keychain.
5. Run `scripts/export-ios.sh`. The Xcode project will be in `UnityProject/Builds/iOS`; the log is `UnityProject/Logs/ios-export.log`.
6. Open the exported Xcode project, verify Unity-iPhone signing and all required app icon slots, choose a physical-device archive destination, and archive. Validate the archive in Organizer before uploading.

## Apple steps, once the owner is ready

- Create an Apple Account and enroll in the Apple Developer Program. The owner handles credentials, identity verification, agreements and payment.
- Register the app identifier and create the App Store Connect app record. Confirm the final display name and bundle ID before the first upload.
- Add the account to Xcode, select the team and provision signing. Archive and upload through Organizer.
- Wait for build processing. Complete beta contact information and applicable export-compliance answers; do not guess the answers.
- Create an internal tester group for eligible App Store Connect users. For people outside that team, prepare an external TestFlight group and any required beta review. Do not create public invitations until the build is approved and the tester scope is agreed.

Official references: [upload builds](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/), [TestFlight overview](https://developer.apple.com/help/app-store-connect/test-a-beta-version/testflight-overview), [internal testers](https://developer.apple.com/help/app-store-connect/test-a-beta-version/add-internal-testers).

## Release checks still required

- Unity Play-mode verification, real-device install, safe areas, scaling, audio pause, background/resume, evidence scrolling and branch outcomes.
- Confirm the generated Xcode asset catalog contains valid opaque icons, including the marketing icon.
- Inspect the actual built SDKs and privacy manifests. The project still includes legacy Analytics and Purchasing packages; do not claim “no data collected” solely because the story code has no network calls. Remove unused SDKs or complete the corresponding privacy/required-reason API declarations based on the final binary.
- Confirm any encryption declaration against the actual binary and libraries.
- Provide the owner’s beta feedback email and review contact details. No login is needed in the current game.

## Five-minute first-chapter playtest

Target duration is a hypothesis, not a measured completion time. Use 5–8 novice players and observe without coaching. Test 320/360/390/430-wide portrait layouts, a notched iPhone, and an iPad. No claim of these tests passing is made yet.

1. Start a fresh case and choose a first lead. On a second run choose the other lead; the dialogue should differ.
2. Open the clock note. Tap a partial timeline, reopen evidence and resume: the partial order should persist. Undo, submit the correct order, then repeat with a wrong order.
3. Try each briefing answer. The unsupported claim should visit “Nina has gone quiet”; the supported claim should skip it.
4. Finish the chapter. Confirm the “Finish this playtest” option keeps progress, and continuing enters The Wrong Face.
5. Ask: “Did you want to take the next call?” and “What made the AI claim unreliable?” Then show a fresh non-police example and ask which claim its source actually supports.
6. Record completion time, points of confusion, optional evidence use and whether players continue voluntarily. The app currently has no experiment analytics; record observations separately with participant consent.
