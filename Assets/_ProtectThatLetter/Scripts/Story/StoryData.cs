using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single line of dialogue with all associated visual/audio data
/// </summary>
[Serializable]
public class DialogueLine {
    public string speaker;
    public string content;
    public string avatar;
    public string position;
    public string background;
    public string sound;
}

/// <summary>
/// Container for a full story/dialogue scene containing dialogue lines
/// </summary>
[Serializable]
public class StoryData
{
    public List<DialogueLine> lines;
}
