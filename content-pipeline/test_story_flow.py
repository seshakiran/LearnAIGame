"""Runs production pure story flow using Unity's bundled .NET runtime.

Unity-specific persistence/rendering is deliberately excluded and needs Play-mode QA.
"""
from pathlib import Path
import json, os, subprocess, tempfile
root = Path(__file__).resolve().parents[1]
unity = Path(os.environ.get('LASTTRAIN_UNITY_APP', '/Applications/Unity/Hub/Editor/6000.0.81f1/Unity.app')) / 'Contents'
runtime = sorted((unity / 'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())[-1]
with tempfile.TemporaryDirectory(prefix='lasttrain-flow-') as directory:
    temp = Path(directory)
    model = (root / 'UnityProject/Assets/Scripts/Story/StoryData.cs').read_text()
    # Pure serializable models are verbatim; exclude PlayerPrefs/JsonUtility adapter.
    model = model.split('    public static class StoryProgress')[0].replace('using UnityEngine;', '') + '}\n'
    (temp / 'Models.cs').write_text(model)
    out = temp / 'FlowTests.dll'
    args = ['-target:exe', '-nostdlib+', '-out:' + str(out)]
    args += ['-r:' + str(p) for p in runtime.glob('*.dll')]
    args += [str(temp / 'Models.cs'), str(root / 'UnityProject/Assets/Scripts/Story/StoryFlow.cs'), str(root / 'content-pipeline/tests/StoryFlowTests.cs')]
    subprocess.run([str(unity / 'NetCoreRuntime/dotnet'), str(unity / 'DotNetSdkRoslyn/csc.dll'), *args], check=True, stdout=subprocess.DEVNULL)
    (temp / 'FlowTests.runtimeconfig.json').write_text(json.dumps({'runtimeOptions': {'tfm': 'net6.0', 'framework': {'name': 'Microsoft.NETCore.App', 'version': runtime.name}}}))
    subprocess.run([str(unity / 'NetCoreRuntime/dotnet'), str(out), str(root / 'UnityProject/Assets/Resources/last_train.json')], check=True)
