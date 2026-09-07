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
                assert beat['evidence'] or beat.get('interaction') == 'lead', f"Decision lacks evidence: {beat['id']}"
                assert len(beat['choices']) >= 2
                assert sum(c['supported'] for c in beat['choices']) == (len(beat['choices']) if beat.get('interaction') == 'lead' else 1)
                for choice in beat['choices']:
                    assert all(choice.get(k) for k in ['text', 'response', 'consequence'])
                if beat.get('assessment'):
                    assessment.append(beat)
    # Walk all authored branches, fail on cycles, dangling targets, unreachable beats.
    for chapter in data['chapters']:
        beats = chapter['beats']
        index = {b['id']: i for i, b in enumerate(beats)}
        visited = set()
        def walk(i, path):
            if i >= len(beats):
                return
            assert i not in path, f"Cycle in {chapter['id']}"
            visited.add(i)
            b = beats[i]
            if b.get('interaction') == 'timeline':
                items = [e['id'] for e in b['timelineItems']]
                assert len(items) == len(set(items))
                assert sorted(items) == sorted(b['correctOrder'])
                assert b['choices'][0]['supported'] and not b['choices'][1]['supported']
            for c in b['choices'] or [{}]:
                target = c.get('nextBeatId') or b.get('nextBeatId')
                assert not target or target in index, f"Missing branch {target}"
                walk(index[target] if target else i + 1, path | {i})
        walk(0, set())
        assert len(visited) == len(beats), f"Unreachable scene in {chapter['id']}"
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
