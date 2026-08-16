#!/usr/bin/env python3
"""Validate required schema details for the LearnAIGame deliverables in this change."""

from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
RESOURCE_DIR = ROOT / "UnityProject" / "Assets" / "Resources"
TOPICS = {
    "bias_training_data_cards.json": "bias_training_data",
    "rag_basics_cards.json": "rag_basics",
    "prompt_injection_cards.json": "prompt_injection",
}
REQUIRED_CARD_FIELDS = {"id", "prompt", "optionA", "optionB", "correctOption", "explanation"}


def load_json(path: Path) -> dict:
    with path.open(encoding="utf-8") as file:
        return json.load(file)


def validate_topic(filename: str, topic_id: str) -> list[str]:
    errors: list[str] = []
    data = load_json(RESOURCE_DIR / filename)
    if data.get("topicId") != topic_id:
        errors.append(f"{filename}: unexpected topicId")
    if data.get("cardType") not in {"real_or_hallucination", "ai_or_human", "ship_or_dont", "bite_you_later"}:
        errors.append(f"{filename}: unsupported cardType")
    cards = data.get("cards")
    if not isinstance(cards, list) or not 6 <= len(cards) <= 8:
        errors.append(f"{filename}: cards must contain 6–8 entries")
    else:
        seen_ids = set()
        for index, card in enumerate(cards, start=1):
            missing = REQUIRED_CARD_FIELDS - card.keys()
            if missing:
                errors.append(f"{filename}: card {index} missing {sorted(missing)}")
            if card.get("correctOption") not in {"A", "B"}:
                errors.append(f"{filename}: card {index} has invalid correctOption")
            if card.get("id") in seen_ids:
                errors.append(f"{filename}: duplicate card ID {card.get('id')}")
            seen_ids.add(card.get("id"))
    checkpoint = data.get("checkpointCard")
    if not isinstance(checkpoint, dict) or REQUIRED_CARD_FIELDS - checkpoint.keys():
        errors.append(f"{filename}: checkpointCard does not match card schema")
    elif checkpoint.get("correctOption") not in {"A", "B"}:
        errors.append(f"{filename}: checkpointCard has invalid correctOption")
    word_count = len(re.findall(r"\b[\w’'-]+\b", data.get("feynmanScript", "")))
    if not 35 <= word_count <= 45:
        errors.append(f"{filename}: feynmanScript is {word_count} words; expected 35–45")
    return errors


def main() -> int:
    errors: list[str] = []
    for filename, topic_id in TOPICS.items():
        errors.extend(validate_topic(filename, topic_id))

    bank = load_json(ROOT / "content" / "boss_levels" / "foundations_ai_judgment_boss.json")
    questions = bank.get("questions", [])
    if not 4 <= len(questions) <= 6:
        errors.append("boss bank: expected 4–6 questions")
    for question in questions:
        if question.get("format") not in {"best_of_four", "short_justify"}:
            errors.append(f"boss bank: {question.get('id')} has unsupported format")
        if not question.get("modelAnswer") or not question.get("rubric"):
            errors.append(f"boss bank: {question.get('id')} lacks model answer or rubric")
        if question.get("format") == "best_of_four":
            options = question.get("options", [])
            if len(options) != 4 or question.get("correctOption") not in {"A", "B", "C", "D"}:
                errors.append(f"boss bank: {question.get('id')} must provide four options and a valid correctOption")

    html = (ROOT / "marketing" / "index.html").read_text(encoding="utf-8")
    for needle in ("<form", "Request an invite", "another AI course"):
        if needle not in html:
            errors.append(f"marketing page: missing expected content {needle!r}")

    uploader = (ROOT / "content-pipeline" / "upload_topic_video.py").read_text(encoding="utf-8")
    for needle in ("videos/{args.topic_id}/{args.version}.mp4", "S3_VIDEO_BUCKET", "streamUrl"):
        if needle not in uploader:
            errors.append(f"uploader: missing expected implementation detail {needle!r}")

    if errors:
        print("Validation failed:")
        for error in errors:
            print(f"- {error}")
        return 1
    print("Validation passed: all content schemas, boss-bank requirements, landing-page markers, and uploader conventions are present.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
