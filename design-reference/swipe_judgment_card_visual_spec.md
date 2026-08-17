# LearnAIGame Swipe-Judgment Card — Visual System Specification

## Design intention

The card should read as a **collectible game object with editorial discipline**: brighter than a dark dashboard, tactile enough to grab, and lively enough to reward a fast judgment call. Depth comes from visible slate planes, a crisp warm contour, a colored ambient lift, and a soft grounded shadow. The result should feel like a premium mobile game, not a party quiz. The only directional accents are the **choice identifiers**: blue for A and amber for B. Neither color represents right or wrong.

> **Visual rule:** Treat every accent as navigation or feedback, never as a correctness signal. Correct/incorrect feedback remains a later, separate reveal state.

## Locked color tokens

All color values below use the supplied palette. Alpha is stated separately so the base hue never drifts.

| Token | Hex | Primary use |
| --- | --- | --- |
| `navy-deep` | `#0A1119` | Deep shadows, screen falloff. |
| `navy-screen` | `#101D29` | Canvas and darkest card gradient stop. |
| `navy-card` | `#16232F` | Card face and chip body base. |
| `navy-chip` | `#1F303E` | Raised chip and top-face highlight. |
| `text-primary` | `#F1EDE4` | Prompt, stamp glyph, key labels. |
| `text-muted` | `#BBB6AC` | Metadata and supporting instruction. |
| `choice-a` | `#6EA8FF` | A badge, A contour, A stamp. |
| `choice-b` | `#D9B26A` | B badge, B contour, B stamp. |
| `reserved-lime` | `#CCE761` | Never use in normal card choice states; reserve for progression/unlocks. |
| `reserved-rose` | `#E5A0B2` | Never use in normal card choice states; reserve for later negative/review feedback. |

## 1. Card face treatment

### Dimensions and content-safe area

The card root remains **640 × 760 pt** with a **36 pt corner radius**. Retain a 42 pt horizontal safe inset. Start prompt content at 58 pt from the top; reserve the bottom 210 pt for answer chips. The visible geometry should be based on a 1080 × 1920 reference canvas, as used by the current project.

The face is not a single image. Build it as a stack of rounded `Image` layers within the card root. This keeps it practical for runtime construction and lets the depth work with the existing procedural rounded sprite.

| Layer, back to front | Rect / offset | Radius | Fill / treatment | Unity implementation note |
| --- | --- | --- | --- | --- |
| Rear stack card | `640 × 760`, `y = +20`, scale `0.970` | 36 pt | `#101D29` at 95% | Optional next-card preview. It gives the front card a tangible deck, not an isolated modal. |
| Rear stack edge | Same as rear stack, inset 1 pt | 35 pt | `#F1EDE4` at 7% | Use a nested image to simulate the outline. |
| Ambient lift | `664 × 788`, `y = -4` | 50 pt | `#6EA8FF` at 15%, with an optional `#CCE761` at 5% top-left bloom | Low-opacity blue halo. Use a soft radial material if available; otherwise omit rather than fake a hard rectangle. |
| Far shadow | `660 × 782`, `y = -26` | 46 pt | `#0A1119` at 68% | Simulates a 26 pt downward shadow. For stock UGUI, make two additional expanding shadow layers rather than use a hard shadow component. |
| Near shadow | `650 × 772`, `y = -11` | 41 pt | `#0A1119` at 74% | The darkest contact shadow. |
| Outer contour | `640 × 760`, no offset | 36 pt | `#F1EDE4` at 28% | A 1.5 pt stroke effect made with a full-size outer image. |
| Edge recess | inset 1.5 pt on all sides | 35 pt | `#101D29` at 100% | The dark rim that makes the contour feel like a material edge, not a bright border. |
| Face | inset 3 pt on all sides | 33 pt | Vertical gradient described below | Use a gradient-capable `Image` material or three clipped overlays. |
| Inner highlight | inset 5 pt on all sides | 31 pt | `#F1EDE4` at 4% top-only | 84 pt tall soft top wash. It should be nearly invisible on a static screenshot and felt more than seen. |
| Content | 42 pt horizontal inset | — | — | Keep content flat; do not add decorative illustrations inside the reading zone. |

### Face gradient

Use this restrained vertical gradient inside the 3 pt face inset.

| Stop | Color | Intended effect |
| --- | --- | --- |
| `0%` | `#1F303E` at 100% | A gentle top-plane catchlight. |
| `18%` | `#1F303E` at 100% | The brighter, stable reading surface. |
| `72%` | `#1F303E` at 100% | Preserve a readable, game-like slate plane across the prompt. |
| `100%` | `#16232F` at 100% | A gentle falloff behind the chip zone without swallowing the play surface. |

If the project stays on stock UGUI with no gradient material, approximate the face with the solid `#16232F` image plus a clipped 150 pt-top overlay in `#1F303E` at 48% alpha and a clipped 220 pt-bottom overlay in `#16232F` at 38% alpha. This is visually closer to the specified face than a single flat color.

### Prompt and minimal metadata

The prompt uses `#F1EDE4` at 100%, 42–46 pt, semibold/bold, left aligned, 1.12–1.18 line height. Constrain it to two lines. A small optional category label above it uses `#BBB6AC` at 72%, 16 pt, uppercase with 0.12 em tracking. Avoid lime or decorative icons in this area.

## 2. A/B answer-chip treatment

Each answer chip is a raised decision surface, not a button pasted onto the card. On the 640 pt card, place two chips **42 pt from each side**, with a **16 pt inter-chip gap**. Each chip is **270 × 146 pt** and has a **22 pt corner radius**. Place the chip group with its bottom edge 42 pt from the face bottom.

| Chip layer, back to front | Geometry | Fill / effect | Purpose |
| --- | --- | --- | --- |
| Chip shadow | `274 × 152 pt`, `y = -8 pt` | `#0A1119` at 74% | Gives the choice a distinct raised plane. |
| Choice glow | `278 × 156 pt`, no offset | A: `#6EA8FF` at 16%; B: `#D9B26A` at 16% | Soft identity halo. It must remain subtle at rest. |
| Accent contour | `270 × 146 pt` | A: `#6EA8FF` at 64%; B: `#D9B26A` at 64% | 1.5 pt simulated border. Keep accents to the perimeter. |
| Chip recess | inset 1.5 pt | `#101D29` | Creates a dark edge below the contour. |
| Chip face | inset 3 pt | vertical gradient: `#1F303E` at top → `#16232F` at bottom | The raised dark slate surface. |
| Chip top glint | clipped 32 pt top strip | `#F1EDE4` at 5% | A faint material highlight. |

Answer text uses `#F1EDE4` at 96%, 24–26 pt, semibold, centered vertically but left aligned inside the chip. Use a 24 pt left inset so text never collides with the badge. Limit text to three short lines; do not reduce below 22 pt to rescue an overlong option.

### Badge

The badge sits across the chip’s upper outer edge, centered 20 pt from the inner side of each chip. The badge is a **52 pt circle**; its center is **6 pt above** the chip’s top edge. Use these layers:

| Badge layer | Size | Fill / treatment |
| --- | --- | --- |
| Badge shadow | 58 pt circle, `y = -5 pt` | `#0A1119` at 88% |
| Badge halo | 60 pt circle | Choice accent at 18% |
| Badge edge | 52 pt circle | `#F1EDE4` at 16% |
| Badge fill | 48 pt circle | A `#6EA8FF` at 100%; B `#D9B26A` at 100% |
| Badge glyph | centered `A` or `B`, 25 pt bold | `#101D29` at 100% |

The dark glyph creates contrast without making either choice look affirmative or dangerous. Do not use checkmarks, crosses, green, or red during the choice phase.

### Drag-side response

As the card is dragged toward a choice, increase only that chip’s accent contour from 64% to 100%, its halo from 16% to 28%, and its local scale from 1.00 to **1.025**. Dim the opposite chip to 72% overall alpha. At the drag midpoint, the selected chip should feel more available—not "correct."

## 3. Drag-feedback stamp treatment

Stamps confirm **direction**, never outcome. They are editorials marks: a sturdy outlined placard with one huge letter and no "like/nope" language.

| Property | Choice A stamp | Choice B stamp |
| --- | --- | --- |
| Position within front face | `x = 34 pt`, `y = -34 pt` from top-left | `x = -34 pt`, `y = -34 pt` from top-right |
| Size | 118 × 82 pt | 118 × 82 pt |
| Corner radius | 16 pt | 16 pt |
| Rotation at full visibility | `-7°` | `+7°` |
| Outer halo | Accent at 18%, +12 pt spread | Accent at 18%, +12 pt spread |
| Main border | 3 pt accent at 100% | 3 pt accent at 100% |
| Interior fill | `#101D29` at 76% | `#101D29` at 76% |
| Glyph | `A`, 58 pt bold, accent at 100% | `B`, 58 pt bold, accent at 100% |
| Secondary label | `CHOOSE A`, 12 pt uppercase, `#F1EDE4` at 78% | `CHOOSE B`, 12 pt uppercase, `#F1EDE4` at 78% |

Build each stamp as an empty root with a `CanvasGroup`, a halo image, a border image, an interior image, and two text labels. The 3 pt border is a full-size accent outer image with a 3 pt inset interior image.

### Drag mapping

Let `t = clamp(abs(horizontalDrag) / 140, 0, 1)`. Apply a smoothstep curve `t × t × (3 − 2t)` for the visible response.

| Property | Mapping |
| --- | --- |
| Matching stamp `CanvasGroup.alpha` | `0 → 0.96` using smoothstep. |
| Non-matching stamp alpha | Fixed at `0`. Never show both stamps. |
| Matching stamp scale | `0.92 → 1.00`. |
| Matching stamp local Y | `+8 pt → 0 pt`; use a small settle, not a bounce. |
| Matching stamp halo alpha | `0 → 18%` accent alpha. |
| Card rotation Z | `clamp(horizontalDrag / 240, -1, 1) × 7°`. Left drag is counter-clockwise; right drag is clockwise. |
| Card Y offset while actively dragged | `+0 → +8 pt`, optionally. This supplies a slight lift before release. |
| Return-to-center | 220 ms, ease-out; clear both stamp alphas by the end. |
| Dismiss fly-out | 180–220 ms; maintain rotation, translate 1.25 screen widths, then reset/replace. |

## 4. Runtime construction notes

The existing `UIFactory.GetRoundedSprite` is sufficient for every hard-edged layer. The look depends on **nesting 1–3 pt inset images** to mimic contour lines, not on importing a fixed card asset. Use a top-level card root for movement; all visual layers, content, chips, and stamps sit under it so they travel and tilt together.

| Concern | Runtime recommendation |
| --- | --- |
| Borders | Simulate strokes with an outer rounded image and an inset rounded image. This works with the existing sliced rounded sprite. |
| Soft shadows / halos | Use 2–3 stepped, enlarged rounded images at low alpha if no blur material exists. Keep them behind the card root and disable raycasts. |
| Face gradient | Prefer a small UI gradient material. If that is intentionally out of scope, use the top and bottom clipped overlays specified above. |
| Raycasts | Set `raycastTarget = false` on all decorative layers; chips and card root retain interaction as required by the current gesture model. |
| Dynamic color | Put the A/B accent values in named palette tokens, not inline at each child image. The assignment must be stable for left/right placement. |
| Text | Keep typography on a real bundled font before production. The current fallback has known weight limitations, so do not use a faux-bold outline effect as a substitute. |
| Accessibility | Pair color with the A/B glyph and placement. Do not rely on hue alone to distinguish a choice. |

## Do not use

Avoid noisy paper textures, colors outside the supplied palette, neon edge lights, large emoji-like symbols, glossy plastic reflections, confetti, or green/red success language. The deck should feel like a bright premium game object with the clarity of an editorial system—not a party quiz tile or a dark enterprise dashboard.
