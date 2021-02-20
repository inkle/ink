using System.Collections.Generic;

namespace Ink.Runtime
{
    public class Flow {
        public string name;
        public CallStack callStack;
        public List<Runtime.Object> outputStream;
        public List<Choice> currentChoices;

        public Flow(string name, Story story) {
            this.name = name;
            this.callStack = new CallStack(story);
            this.outputStream = new List<Object>();
            this.currentChoices = new List<Choice>();
        }
    }
}