using System.Collections.Generic;

namespace Ink.Runtime
{
    public interface IStory
    {
        event StoryErrorEventHandler StoryError;

        bool allowExternalFunctionFallbacks { get; set; }
        bool canContinue { get; }

        List<Choice> currentChoices { get; }

        /// <summary>Gets a value indicating whether this instance has current choices.</summary>
        /// <value>
        ///   <c>true</c> if this instance has current choices; otherwise, <c>false</c>.</value>
        bool HasCurrentChoices { get; }

        List<string> currentErrors { get; }

        List<string> currentTags { get; }

        /// <summary>Gets a value indicating whether this instance has current tags.</summary>
        /// <value>
        ///   <c>true</c> if this instance has current tags; otherwise, <c>false</c>.</value>
        bool HasCurrentTags { get; }

        string currentText { get; }
        List<string> currentWarnings { get; }
        StoryState state { get; }

        void ChooseChoiceIndex(int choiceIdx);
        void ChoosePathString(string path, bool resetCallstack = true, params object[] arguments);
        string Continue();
        string ToJson();

    }
}