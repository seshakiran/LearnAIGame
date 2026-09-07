#!/bin/bash
set -eu
project_root="$(cd "$(dirname "$0")/.." && pwd)"
unity_app="${LASTTRAIN_UNITY_APP:-/Applications/Unity/Hub/Editor/6000.0.81f1/Unity.app}"
: "${LASTTRAIN_BUNDLE_ID:?Set your registered bundle ID first}"
: "${LASTTRAIN_BUILD_NUMBER:?Set a positive unused build number first}"
python3 "$project_root/content-pipeline/validate_story_campaign.py"
mkdir -p "$project_root/UnityProject/Logs"
exec "$unity_app/Contents/MacOS/Unity" -batchmode -quit -projectPath "$project_root/UnityProject" \
  -executeMethod LearnAIGame.EditorTools.LastTrainBuild.ExportIOS \
  -logFile "$project_root/UnityProject/Logs/ios-export.log"
