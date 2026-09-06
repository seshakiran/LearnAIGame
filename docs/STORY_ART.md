# Last Train visual direction

Comic panels are the default. They preserve the whole illustration, separate narration from source records, and keep learning goals on solid surfaces. The optional full-screen scene view crops to a per-beat focal point and adds a dark wash for text contrast. It is for immersion; decisions stay in the comic view.

Six original chapter illustrations appear across all 25 beats with individual scene captions and learning objectives. These are chapter keyframes, not 25 unique illustrations. Artwork is decorative fiction and is explicitly labeled separately from original evidence. The final briefing names the skill being applied but does not expose answer feedback early.

Files: `UnityProject/Assets/Resources/StoryArt/{platform,face,message,voice,chain,morning}.png`. Generated using the built-in imagegen tool. PNG originals retained; Unity imports with no mipmaps and clamp wrapping. `artFocusX` protects the platform object in portrait crops. `learningObjective`, `artCaption`, and `artResource` are authored per beat; no save-version bump is required because decision order and meanings are unchanged.

The side-by-side HTML in `design-reference/last-train-layout-comparison.html` is a design mockup, not the running Unity UI. The six full-size images were visually reviewed. Local browser preview access was unavailable, and Unity runtime layout testing remains pending because of the licensing blocker from the previous session.

## Generation prompts

Shared prompt prefix: Use case: illustration-story. Asset type: production game chapter illustration for LAST TRAIN, a fictional contemporary police mystery teaching AI judgment. Create ONE landscape 1536x1024 image, a single cinematic graphic novel panel, no borders or panel subdivisions. Consistent style: mature hand-inked neo-noir graphic novel, fine crosshatching, restrained painterly shading, deep navy #0a1119, warm amber #d9b26a and muted cyan highlights, believable human anatomy, atmospheric but clearly legible on a mobile screen. Scene:

+- platform: A moody wide New York subway platform at night. A small mysterious cyan illuminated rectangular object stands beside a tiled column in the foreground. Ordinary commuters far away, silver train on the left, amber edge lighting, tension through composition, nobody committing violence.
- face: An intelligence analyst's desk at night. Two grainy subway surveillance images on separate monitors, one showing an anonymous dark jacket with a reflective sleeve, the other a different dark jacket. Physical timestamp notes and magnifier on desk, rain-lit window. No readable numbers. No named suspect, no facial identification boxes.
- message: A fictional Manhattan subway concourse splitting into corridors. In foreground a hand holds a phone displaying a simple glowing directional arrow, with a strange luminous promotional box nearby. In background a worried adult female commuter looks down a distant passage. Rainy-night noir, mystery, no violence.
- voice: Close-up still life on a transit command desk: a radio handset, an audio waveform on a blue monitor, a second amber authenticated communications console, soft reflections. Impersonation and uncertain provenance suggested by doubled reflections. No faces, no readable text or logos.
- chain: A transit analyst at a dark desk viewed from behind, confronting a large monitor with abstract linked document panels leading to an amber approval gate. A file folder and radio on desk, blue amber noir light. Technological tension conveyed through restrained realistic graphic novel art, no readable text, no logos.
- morning: Quiet emotional resolution on a New York subway platform late at night. An adult woman embracing her teenage brother, seen at respectful distance; female detective in plain dark jacket stands nearby with a radio. Station lights reflect in a silver train. Amber and cyan lighting, relief after suspense, no weapons, no police seals.

Shared suffix: Composition: focal action within central 70% so it works both as a wide comic panel and a portrait crop. No text, captions, speech bubbles, watermarks, logos, blood, weapons or depictions of object contents. It must read as fictional illustration, not evidence photography.
