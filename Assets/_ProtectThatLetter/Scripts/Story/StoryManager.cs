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

    public static string GetCurrentStoryFileName() {
        return CurrentStoryType switch {
            StoryType.Intro => "IntroStory",
            StoryType.Outro => "OutroStory",
            _ => "IntroStory"
        };
    }

    //Will change to right scene later
    public static string GetNextSceneName() {
        return CurrentStoryType switch {
            StoryType.Intro => SceneController.LOGIN_SCENE,
            StoryType.Outro => SceneController.LOGIN_SCENE,
            _ => SceneController.LOGIN_SCENE
        };
    }
}
