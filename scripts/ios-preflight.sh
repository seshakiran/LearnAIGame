#!/bin/bash
set -eu
project_root="$(cd "$(dirname "$0")/.." && pwd)"
unity_app="${LASTTRAIN_UNITY_APP:-/Applications/Unity/Hub/Editor/6000.0.81f1/Unity.app}"
status=0
check() { if test "$1" = yes; then printf 'OK: %s\n' "$2"; else printf 'NEEDS SETUP: %s\n' "$2"; status=1; fi; }
check "$(test -x "$unity_app/Contents/MacOS/Unity" && echo yes || echo no)" 'Unity 6000.0.81f1 editor'
check "$(test -d "$unity_app/Contents/PlaybackEngines/iOSSupport" && echo yes || echo no)" 'iOS Build Support files in the selected Editor'
check "$(xcodebuild -version >/dev/null 2>&1 && echo yes || echo no)" 'Xcode command-line tools'
check "$(test -n "${LASTTRAIN_BUNDLE_ID:-}" && echo yes || echo no)" 'LASTTRAIN_BUNDLE_ID (registered app identifier)'
check "$(test -n "${LASTTRAIN_BUILD_NUMBER:-}" && echo yes || echo no)" 'LASTTRAIN_BUILD_NUMBER (unused positive integer)'
check "$(test -n "${LASTTRAIN_APPLE_TEAM_ID:-}" && echo yes || echo no)" 'LASTTRAIN_APPLE_TEAM_ID (required later for signing)'
python3 "$project_root/content-pipeline/validate_story_campaign.py"
printf '%s\n' 'This preflight does not authenticate Apple or prove Unity licensing, signing, or App Store eligibility.'
exit "$status"
