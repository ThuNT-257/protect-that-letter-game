using UnityEngine;

/// <summary>
/// Manages story types and provides file names and scene transitions for stories
/// </summary>
public static class StoryManager
{
    
    public enum StoryType {
        Intro,
        Outro
    }
    public static StoryType CurrentStoryType = StoryType.Intro;

    /// <summary>
    /// Returns the JSON file name for the current story type
    /// </summary>
    /// <returns>File name of the story JSON</returns>
    public static string GetCurrentStoryFileName() {
        return CurrentStoryType switch {
            StoryType.Intro => "IntroStory",
            StoryType.Outro => "OutroStory",
            _ => "IntroStory"
        };
    }

    /// <summary>
    /// Returns the scene to load after the current story ends
    /// </summary>
    /// <returns>Name of the next scene</returns>
    //Will change to right scene later
    public static string GetNextSceneName() {
        return CurrentStoryType switch {
            StoryType.Intro => SceneController.PLAY_SCENE,
            StoryType.Outro => SceneController.LOGIN_SCENE,
            _ => SceneController.LOGIN_SCENE
        };
    }
}
