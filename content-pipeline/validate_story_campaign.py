"""Validate authored campaigns, evidence references and all possible ending routes."""
import json
from pathlib import Path
from itertools import product

ROOT = Path(__file__).resolve().parents[1]

def validate(path):
    data = json.loads(path.read_text())
    assert data['id'] and data['version'] > 0
    assert len(data['chapters']) > 0
    beat_ids, evidence_ids, assessment = set(), set(), []
    decisions = 0
    for chapter in data['chapters']:
        assert chapter['title'] and chapter['concept'] and chapter['beats']
        for beat in chapter['beats']:
            assert beat['id'] not in beat_ids, f"Duplicate beat: {beat['id']}"
            beat_ids.add(beat['id'])
            assert beat['body'] and beat['speaker']
            assert beat.get('learningObjective'), f"Missing learning goal: {beat['id']}"
            assert beat.get('artCaption'), f"Missing scene caption: {beat['id']}"
            art = beat.get('artResource', '')
            assert art.startswith('StoryArt/') and '..' not in art
            assert (ROOT / 'UnityProject/Assets/Resources' / (art + '.png')).is_file(), f"Missing artwork: {art}"
            assert 0 <= beat.get('artFocusX', .5) <= 1
            for evidence in beat['evidence']:
                assert evidence['id'] not in evidence_ids
                evidence_ids.add(evidence['id'])
                assert all(evidence.get(key) for key in ['title', 'source', 'text'])
            if beat['choices']:
                decisions += 1
                assert beat['question'] and beat['concept'] and beat['lesson']
                assert beat.get('simpleExplanation'), f"Missing plain-language explanation: {beat['id']}"
                assert beat['evidence'], f"Decision lacks evidence: {beat['id']}"
                assert len(beat['choices']) >= 2
                assert sum(c['supported'] for c in beat['choices']) == 1
                for choice in beat['choices']:
                    assert all(choice.get(k) for k in ['text', 'response', 'consequence'])
                if beat.get('assessment'):
                    assessment.append(beat)
    # The first campaign's final briefing promises exactly three held-feedback calls.
    assert len(assessment) == 3
    endings = set()
    for choices in product(*[b['choices'] for b in assessment]):
        score = sum(c['supported'] for c in choices)
        endings.add('supported' if score == 3 else 'partial' if score == 2 else 'recalled')
    assert endings == {'supported', 'partial', 'recalled'}
    print(f"{data['title']}: {len(data['chapters'])} chapters, {len(beat_ids)} beats, "
          f"{decisions} decisions, {len(evidence_ids)} unique records; all 3 endings reachable.")

if __name__ == '__main__':
    validate(ROOT / 'UnityProject/Assets/Resources/last_train.json')
