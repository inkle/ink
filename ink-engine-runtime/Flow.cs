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
        }

        public void WriteJson(SimpleJson.Writer writer)
        {
            writer.WriteObjectStart();

            writer.WriteProperty("callstack", callStack.WriteJson);
            writer.WriteProperty("outputStream", w => Json.WriteListRuntimeObjs(w, outputStream));

            writer.WriteProperty("currentChoices", w => {
                w.WriteArrayStart();
                foreach (var c in currentChoices)
                    Json.WriteChoice(w, c);
                w.WriteArrayEnd();
            });

            writer.WriteObjectEnd();
        }
    }
}