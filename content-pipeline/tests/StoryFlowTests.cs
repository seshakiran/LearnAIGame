using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LearnAIGame.Story;

class StoryFlowTests
{
    static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    static void Main(string[] args)
    {
        var campaign = JsonSerializer.Deserialize<StoryCampaign>(File.ReadAllText(args[0]), new JsonSerializerOptions { IncludeFields = true });
        var chapter = campaign.chapters[0];
        int Index(string id) => Array.FindIndex(chapter.beats, b => b.id == id);
        string Follow(string id, int choice) => chapter.beats[StoryFlow.NextIndex(chapter, Index(id), new StoryDecision { beatId = id, choice = choice })].id;
        Assert(Follow("first_lead", 0) == "camera_lead", "Camera route");
        Assert(Follow("first_lead", 1) == "witness_lead", "Witness route");
        Assert(chapter.beats[StoryFlow.NextIndex(chapter, Index("camera_lead"), null)].id == "timeline", "Camera must skip witness-only scene");
        Assert(chapter.beats[StoryFlow.NextIndex(chapter, Index("witness_lead"), null)].id == "timeline", "Witness must converge");
        Assert(Follow("claim", 0) == "repair_call", "Mistaken briefing must have consequence");
        Assert(Follow("claim", 1) == "lure", "Supported briefing must skip repair");
        Assert(StoryFlow.NextIndex(chapter, Index("cliff01"), null) == -1, "Chapter exit");
        var puzzle = chapter.beats[Index("timeline")];
        int correct = 0;
        var ids = puzzle.timelineItems.Select(x => x.id).ToArray();
        foreach (var a in ids) foreach (var b in ids) foreach (var c in ids)
        {
            var attempt = new[] { a, b, c };
            if (attempt.Distinct().Count() != 3) continue;
            if (StoryFlow.CorrectOrder(puzzle, attempt)) correct++;
        }
        Assert(correct == 1, "Exactly one of six complete orders is correct");
        Assert(!StoryFlow.CorrectOrder(puzzle, new[] { ids[0] }), "Partial timeline rejected");
        Assert(!StoryFlow.CorrectOrder(puzzle, new[] { ids[0], ids[0], ids[0] }), "Duplicate timeline rejected");
        var save = new StorySave { draftBeatId = "timeline", draftOrder = ids.Take(2).ToList() };
        var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { IncludeFields = true });
        var loaded = JsonSerializer.Deserialize<StorySave>(json, new JsonSerializerOptions { IncludeFields = true });
        Assert(loaded.draftOrder.SequenceEqual(save.draftOrder), "Draft data retains order through JSON roundtrip");
        Console.WriteLine("PASS: lead branches, consequence branch, chapter exit, all timeline permutations, partial/duplicate rejection, draft JSON data roundtrip.");
    }
}
