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

        public Flow(string name, Story story, Dictionary<string, object> jObject) {
            this.name = name;
            this.callStack = new CallStack(story);
            this.callStack.SetJsonToken ((Dictionary < string, object > )jObject ["callstack"], story);
            this.outputStream = Json.JArrayToRuntimeObjList ((List<object>)jObject ["outputStream"]);
			this.currentChoices = Json.JArrayToRuntimeObjList<Choice>((List<object>)jObject ["currentChoices"]);

            // choiceThreads is optional
            object jChoiceThreadsObj;
            jObject.TryGetValue("choiceThreads", out jChoiceThreadsObj);
            LoadFlowChoiceThreads((Dictionary<string, object>)jChoiceThreadsObj, story);
        }

        public void WriteJson(SimpleJson.Writer writer)
        {
            writer.WriteObjectStart();

            writer.WriteProperty("callstack", callStack.WriteJson);
            writer.WriteProperty("outputStream", w => Json.WriteListRuntimeObjs(w, outputStream));

            // choiceThreads: optional
            // Has to come BEFORE the choices themselves are written out
            // since the originalThreadIndex of each choice needs to be set
            bool hasChoiceThreads = false;
            foreach (Choice c in currentChoices)
            {
                c.originalThreadIndex = c.threadAtGeneration.threadIndex;

                if (callStack.ThreadWithIndex(c.originalThreadIndex) == null)
                {
                    if (!hasChoiceThreads)
                    {
                        hasChoiceThreads = true;
                        writer.WritePropertyStart("choiceThreads");
                        writer.WriteObjectStart();
                    }

                    writer.WritePropertyStart(c.originalThreadIndex);
                    c.threadAtGeneration.WriteJson(writer);
                    writer.WritePropertyEnd();
                }
            }

            if (hasChoiceThreads)
            {
                writer.WriteObjectEnd();
                writer.WritePropertyEnd();
            }


            writer.WriteProperty("currentChoices", w => {
                w.WriteArrayStart();
                foreach (var c in currentChoices)
                    Json.WriteChoice(w, c);
                w.WriteArrayEnd();
            });


            writer.WriteObjectEnd();
        }

        // Used both to load old format and current
        public void LoadFlowChoiceThreads(Dictionary<string, object> jChoiceThreads, Story story)
        {
            foreach (var choice in currentChoices) {
				var foundActiveThread = callStack.ThreadWithIndex(choice.originalThreadIndex);
				if( foundActiveThread != null ) {
                    choice.threadAtGeneration = foundActiveThread.Copy ();
				} else {
					var jSavedChoiceThread = (Dictionary <string, object>) jChoiceThreads[choice.originalThreadIndex.ToString()];
					choice.threadAtGeneration = new CallStack.Thread(jSavedChoiceThread, story);
				}
			}
        }
    }
}