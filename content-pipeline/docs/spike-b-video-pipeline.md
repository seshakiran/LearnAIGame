# Spike B: Review-Gated Topic Video Pipeline

**Purpose.** This workflow produces one short, pre-approved Feynman clip per topic. The production asset is deliberately separate from the game runtime: content is generated and reviewed before release, then uploaded as a versioned MP4 that the Unity client can stream. It does not generate video at unlock time.

## Delivery contract

Each approved topic produces one immutable video object and one human-readable production record. The proposed object key is `videos/<topicId>/<version>.mp4`, where `topicId` exactly matches the card-burst JSON and `version` is a UTC timestamp or release label such as `v20260816t174500z`. An immutable key avoids serving an old video from a cache after a replacement. The upload tool returns both the S3 URI and the HTTPS URL that Unity will receive.

| Asset | Location | Required state before release |
| --- | --- | --- |
| Card burst | `UnityProject/Assets/Resources/<topicId>_cards.json` | Approved instructional content. |
| Spoken script | `content-pipeline/scripts/<topicId>.md` | 35–45 spoken words; directly names a card example. |
| Generation brief | `content-pipeline/briefs/<topicId>.md` | Approved visual direction and shot timing. |
| Review record | `content-pipeline/reviews/<topicId>_<version>.md` | Every checklist item marked pass by a named reviewer. |
| Master video | Local reviewed MP4 | Portrait, approximately 15 seconds, final audio and captions. |
| Hosted video | `s3://<bucket>/videos/<topicId>/<version>.mp4` | Upload succeeds and the final stream URL is tested. |

> **Release rule:** A generated clip is a candidate, not an asset. Only a clip with a completed review record may be uploaded to the production keyspace.

## End-to-end workflow

| Stage | Owner | Concrete action | Exit criterion |
| --- | --- | --- | --- |
| 1. Lock source material | Content owner | Select the already-approved card burst, checkpoint, and 15-second Feynman script. Put the exact spoken text in the generation brief; do not ask the generator to improvise facts. | Script is 35–45 words and references at least one exact card pattern. |
| 2. Make the generation brief | Content producer | Specify portrait framing, spoken script, timestamped shots, calm delivery, on-screen wording, and prohibited claims. Create two or three candidates through Grok video generation. | Candidate files are named `<topicId>_<candidate>.mp4`; no candidate is treated as final. |
| 3. Human content review | Designated reviewer | Watch each candidate with the card burst open. Complete the checklist below, choosing one candidate or rejecting all. | Accuracy, card-match, pacing, tone, and technical checks all pass. |
| 4. Finalize master | Content producer | Make only approved corrections, such as a regenerated take or an approved trim. Export one MP4 with embedded audio and optional burned-in captions. | Final runtime is 14.5–16.0 seconds; reviewer signs the review record. |
| 5. Upload immutable object | Release operator | Run `upload_topic_video.py` with the reviewed file, topic ID, bucket, and release version. The tool sets `Content-Type: video/mp4` and a long cache lifetime appropriate for versioned keys. Boto3’s managed `upload_file` interface supports optional upload arguments such as metadata and content settings.[1] | Command prints `s3Uri` and `streamUrl` as JSON and no error is returned. |
| 6. Verify hosted playback | Release operator | Open the returned URL in a clean browser session and test the same URL in the target Unity build on the first supported mobile device. Confirm video, audio, duration, and aspect ratio. | Both tests pass; record the selected version and URL in the topic release manifest. |
| 7. Publish the reference | Unity/content owner | Add the approved URL to the future topic-video manifest or catalog, then switch the relevant topic to that version only after verification. Never overwrite an existing release object. | The client points to an approved, tested URL. |

## Human review checklist

The reviewer must check the clip against the **actual card content**, not merely whether it sounds generally sensible. A clip that is attractive but teaches a generic definition should fail.

| Check | Pass condition | Reject when |
| --- | --- | --- |
| Script fidelity | The voiceover matches the approved script in meaning and does not add claims, dates, sources, or advice. | It invents a fact, names an unapproved source, or changes the lesson. |
| Card-content match | The explanation directly refers to a card example or its exact pattern, such as extra fabricated detail on a true fact. | It could be swapped into any AI explainer without the player noticing. |
| Judgment framing | The clip teaches how to evaluate an output, source, or instruction; it does not ask the player to memorize a definition. | It turns into a textbook description or a recall quiz. |
| Accuracy and scope | Every factual claim is supported by the approved script/card set, and uncertainty is not erased. | It makes a broad promise or a stronger conclusion than the source supports. |
| Tone | Delivery is plain, dry, and mildly wry; it is credible rather than excitable or classroom-like. | It feels hype-driven, condescending, or like generic ed-tech narration. |
| Pacing | Speech remains understandable at normal volume, includes natural pauses, and finishes in approximately 15 seconds. | It rushes the final clause, has long dead air, or exceeds 16 seconds. |
| Visual usefulness | On-screen text is minimal, legible in portrait mobile framing, and supports the spoken idea rather than duplicating a transcript. | Text is unreadable, distracting, or implies a fact not spoken and reviewed. |
| Technical quality | Video is a playable MP4 with audio, correct portrait framing, no clipped captions, no accidental watermarks, and no visible generation artifact that changes meaning. | Audio is missing, text is cut off, a face/mouth distracts from narration, or playback visibly fails. |
| Rights and privacy | No private user data, copyrighted third-party footage, or unlicensed brand material is included without explicit clearance. | The clip exposes confidential information or unreviewed third-party assets. |

## Hosting and Unity streaming design

The recommended production route keeps the S3 bucket private and exposes the video through an HTTPS media domain, typically a CDN distribution whose origin is the bucket. Set `S3_VIDEO_BASE_URL` to that media domain, for example `https://media.learnaigame.example`; the uploader will then return `https://media.learnaigame.example/videos/hallucinations/v20260816t174500z.mp4`. If a temporary prototype instead uses a publicly readable S3 endpoint, set the same variable to the bucket endpoint and grant only read access for the `videos/` prefix. Access policy still applies independently of any CORS configuration.[2]

For Unity, Spike B replaces the existing local-clip assignment with the approved HTTPS URL: set the player’s source to URL and set `VideoPlayer.url` to the release-manifest value. Unity’s `VideoPlayer.url` accepts web URLs, and the last-set clip or URL takes precedence.[3] The current prototype has no topic-video manifest, so this plan intentionally leaves the release URL out of the card-burst schema. A subsequent, small runtime change should load a topic-to-video mapping rather than hardcode a single clip.

The suggested media export is an H.264/AAC MP4 at 1080×1920, 30 fps, portrait, with one audio track. Before opening a WebGL build, configure the video origin for only the required web origins and `GET`/`HEAD` requests. Amazon S3 evaluates CORS by matching the request origin, method, and request headers; it does not make an object public by itself.[2]

## Review record template

```markdown
# Video review — <topicId> / <version>

- Candidate reviewed: `<filename>`
- Reviewer: `<name>`
- Date (UTC): `<YYYY-MM-DD>`
- Card burst version: `<git commit or release>`
- Script version: `<git commit or release>`

| Check | Pass/Fail | Notes |
| --- | --- | --- |
| Script fidelity |  |  |
| Card-content match |  |  |
| Judgment framing |  |  |
| Accuracy and scope |  |  |
| Tone |  |  |
| Pacing (14.5–16.0 s) |  |  |
| Visual usefulness |  |  |
| Technical quality |  |  |
| Rights and privacy |  |  |

**Decision:** Approved / Rework / Rejected

**Required changes before approval:**
```

## Operating estimate per topic

Once the script and visual template are stable, the expected **human active time is 18–30 minutes per topic**: 3–5 minutes to adapt the approved script into a shot brief, 6–10 minutes to review two or three candidates, 5–8 minutes for a re-generation or approved trim, and 4–7 minutes for upload and device playback verification. Generation time can run while the producer does other work; the expected **elapsed time is 30–50 minutes** when a first or second candidate passes. Budget **60–90 minutes elapsed** for a topic needing factual rework, pacing correction, or a second review round.

This is the Spike B throughput metric: count a topic only when its review record and tested immutable URL both exist. The first production batch should time five topics separately so the estimate can be replaced with an observed median and a rework rate.

## References

[1]: https://docs.aws.amazon.com/boto3/latest/guide/s3-uploading-files.html "AWS Boto3: Uploading files"
[2]: https://docs.aws.amazon.com/AmazonS3/latest/userguide/cors.html "AWS S3: Using cross-origin resource sharing (CORS)"
[3]: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Video.VideoPlayer-url.html "Unity 6: VideoPlayer.url"
