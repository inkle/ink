using System.Collections.Generic;

namespace Ink.Runtime
{
    public interface IStory
    {
        event StoryErrorEventHandler StoryError;

        bool allowExternalFunctionFallbacks { get; set; }
        bool canContinue { get; }
        List<Choice> currentChoices { get; }
        List<string> currentErrors { get; }
        List<string> currentTags { get; }
        string currentText { get; }
        List<string> currentWarnings { get; }
        StoryState state { get; }

        void ChooseChoiceIndex(int choiceIdx);
        void ChoosePathString(string path, bool resetCallstack = true, params object[] arguments);
        string Continue();
        string ToJson();

    }
}