using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Ink.Runtime
{
	/// <summary>
	/// Simple ink profiler that logs every instruction in the story and counts frequency and timing.
	/// To use:
	///  
	///   var profiler = story.StartProfiling(), 
	/// 
	///   (play your story for a bit)
	/// 
	///   var reportStr = profiler.Report();
	/// 
	///   story.EndProfiling();
	/// 
	/// </summary>
	public class Profiler
	{
        /// <summary>
        /// The root node in the hierarchical tree of recorded ink timings.
        /// </summary>
		public ProfileNode rootNode {
			get {
				return _rootNode;
			}
		}

		internal Profiler() {
			_rootNode = new ProfileNode();
		}

        /// <summary>
        /// Generate a printable report based on the data recording during profiling.
        /// </summary>
		public string Report() {
			var sb = new StringBuilder();
			sb.AppendFormat("{0} CONTINUES / LINES:\n", _numContinues);
			sb.AppendFormat("TOTAL TIME: {0}\n", FormatMillisecs(_continueTotal));
			sb.AppendFormat("SNAPSHOTTING: {0}\n", FormatMillisecs(_snapTotal));
			sb.AppendFormat("OTHER: {0}\n", FormatMillisecs(_continueTotal - (_stepTotal + _snapTotal)));
			sb.Append(_rootNode.ToString());
			return sb.ToString();
		}

		internal void PreContinue() {
			_continueWatch.Reset();
			_continueWatch.Start();
		}

		internal void PostContinue() {
			_continueWatch.Stop();
			_continueTotal += Millisecs(_continueWatch);
			_numContinues++;
		}

		internal void PreStep() {
			_currStepStack = null;
			_stepWatch.Reset();
			_stepWatch.Start();
		}

		internal void Step(CallStack callstack) 
		{
			_stepWatch.Stop();

			var stack = new string[callstack.elements.Count];
			for(int i=0; i<stack.Length; i++) {
				var objPath = callstack.elements[i].currentPointer.path;
				string stackElementName = "";

				for(int c=0; c<objPath.length; c++) {
					var comp = objPath.GetComponent(c);
					if( !comp.isIndex ) {
						stackElementName = comp.name;
						break;
					}
				}

				stack[i] = stackElementName;
			}
				
			_currStepStack = stack;

			var currObj = callstack.currentElement.currentPointer.Resolve();

			string stepType = null;
			var controlCommandStep = currObj as ControlCommand;
			if( controlCommandStep )
				stepType = controlCommandStep.commandType.ToString() + " CC";
			else
				stepType = currObj.GetType().Name;

			_currStepDetails = new StepDetails {
				type = stepType,
				obj = currObj
			};

			_stepWatch.Start();
		}

		internal void PostStep() {
			_stepWatch.Stop();

			var duration = Millisecs(_stepWatch);
			_stepTotal += duration;

			_rootNode.AddSample(_currStepStack, duration);

			_currStepDetails.time = duration;
			_stepDetails.Add(_currStepDetails);
		}

        /// <summary>
        /// Generate a printable report specifying the average and maximum times spent
        /// stepping over different internal ink instruction types.
        /// This report type is primarily used to profile the ink engine itself rather
        /// than your own specific ink.
        /// </summary>
		public string StepLengthReport()
		{
			var sb = new StringBuilder();

			sb.AppendLine("TOTAL: "+_rootNode.totalMillisecs+"ms");

			var averageStepTimes = _stepDetails
				.GroupBy(s => s.type)
				.Select(typeToDetails => new KeyValuePair<string, double>(typeToDetails.Key, typeToDetails.Average(d => d.time)))
				.OrderByDescending(stepTypeToAverage => stepTypeToAverage.Value)
				.Select(stepTypeToAverage => {
					var typeName = stepTypeToAverage.Key;
					var time = stepTypeToAverage.Value;
					return typeName + ": " + time + "ms";
				})
				.ToArray();

			sb.AppendLine("AVERAGE STEP TIMES: "+string.Join(", ", averageStepTimes));

			var accumStepTimes = _stepDetails
				.GroupBy(s => s.type)
				.Select(typeToDetails => new KeyValuePair<string, double>(typeToDetails.Key + " (x"+typeToDetails.Count()+")", typeToDetails.Sum(d => d.time)))
				.OrderByDescending(stepTypeToAccum => stepTypeToAccum.Value)
				.Select(stepTypeToAccum => {
					var typeName = stepTypeToAccum.Key;
					var time = stepTypeToAccum.Value;
					return typeName + ": " + time;
				})
				.ToArray();

			sb.AppendLine("ACCUMULATED STEP TIMES: "+string.Join(", ", accumStepTimes));

			return sb.ToString();
		}

        /// <summary>
        /// Create a large log of all the internal instructions that were evaluated while profiling was active.
        /// Log is in a tab-separated format, for easy loading into a spreadsheet application.
        /// </summary>
		public string Megalog()
		{
			var sb = new StringBuilder();

			sb.AppendLine("Step type\tDescription\tPath\tTime");

			foreach(var step in _stepDetails) {
				sb.Append(step.type);
				sb.Append("\t");
				sb.Append(step.obj.ToString());
				sb.Append("\t");
				sb.Append(step.obj.path);
				sb.Append("\t");
				sb.AppendLine(step.time.ToString("F8"));
			}

			return sb.ToString();
		}

		internal void PreSnapshot() {
			_snapWatch.Reset();
			_snapWatch.Start();
		}

		internal void PostSnapshot() {
			_snapWatch.Stop();
			_snapTotal += Millisecs(_snapWatch);
		}

		double Millisecs(Stopwatch watch)
		{
			var ticks = watch.ElapsedTicks;
			return ticks * _millisecsPerTick;
		}

		internal static string FormatMillisecs(double num) {
			if( num > 5000 ) {
				return string.Format("{0:N1} secs", num / 1000.0);
			} if( num > 1000 ) {
				return string.Format("{0:N2} secs", num / 1000.0);
			} else if( num > 100 ) {
				return string.Format("{0:N0} ms", num);
			} else if( num > 1 ) {
				return string.Format("{0:N1} ms", num);
			} else if( num > 0.01 ) {
				return string.Format("{0:N3} ms", num);
			} else {
				return string.Format("{0:N} ms", num);
			}
		}

		Stopwatch _continueWatch = new Stopwatch();
		Stopwatch _stepWatch = new Stopwatch();
		Stopwatch _snapWatch = new Stopwatch();

		double _continueTotal;
		double _snapTotal;
		double _stepTotal;

		string[] _currStepStack;
		StepDetails _currStepDetails;
		ProfileNode _rootNode;
		int _numContinues;

		struct StepDetails {
			public string type;
			public Runtime.Object obj;
			public double time;
		}
		List<StepDetails> _stepDetails = new List<StepDetails>();

		static double _millisecsPerTick = 1000.0 / Stopwatch.Frequency;
	}


    /// <summary>
    /// Node used in the hierarchical tree of timings used by the Profiler.
    /// Each node corresponds to a single line viewable in a UI-based representation.
    /// </summary>
	public class ProfileNode {

        /// <summary>
        /// The key for the node corresponds to the printable name of the callstack element.
        /// </summary>		
        public readonly string key;


        #pragma warning disable 0649
        /// <summary>
        /// Horribly hacky field only used by ink unity integration,
        /// but saves constructing an entire data structure that mirrors
        /// the one in here purely to store the state of whether each
        /// node in the UI has been opened or not  /// </summary>
        public bool openInUI;
        #pragma warning restore 0649

        /// <summary>
        /// Whether this node contains any sub-nodes - i.e. does it call anything else
        /// that has been recorded?
        /// </summary>
        /// <value><c>true</c> if has children; otherwise, <c>false</c>.</value>
		public bool hasChildren {
			get {
				return _nodes != null && _nodes.Count > 0;
			}
		}

        /// <summary>
        /// Total number of milliseconds this node has been active for.
        /// </summary>
		public int totalMillisecs {
			get {
				return (int)_totalMillisecs;
			}
		}

		internal ProfileNode() {

		}

		internal ProfileNode(string key) {
			this.key = key;
		}

		internal void AddSample(string[] stack, double duration) {
			AddSample(stack, -1, duration);
		}

		void AddSample(string[] stack, int stackIdx, double duration) {

			_totalSampleCount++;
			_totalMillisecs += duration;

			if( stackIdx == stack.Length-1 ) {
				_selfSampleCount++;
				_selfMillisecs += duration;
			}

			if( stackIdx+1 < stack.Length )
				AddSampleToNode(stack, stackIdx+1, duration);
		}

		void AddSampleToNode(string[] stack, int stackIdx, double duration)
		{
			var nodeKey = stack[stackIdx];
			if( _nodes == null ) _nodes = new Dictionary<string, ProfileNode>();

			ProfileNode node;
			if( !_nodes.TryGetValue(nodeKey, out node) ) {
				node = new ProfileNode(nodeKey);
				_nodes[nodeKey] = node;
			}

			node.AddSample(stack, stackIdx, duration);
		}

        /// <summary>
        /// Returns a sorted enumerable of the nodes in descending order of
        /// how long they took to run.
        /// </summary>
		public IEnumerable<KeyValuePair<string, ProfileNode>> descendingOrderedNodes {
			get {
				if( _nodes == null ) return null;
				return _nodes.OrderByDescending(keyNode => keyNode.Value._totalMillisecs);
			}
		}

		void PrintHierarchy(StringBuilder sb, int indent)
		{
			Pad(sb, indent);

			sb.Append(key);
			sb.Append(": ");
			sb.AppendLine(ownReport);

			if( _nodes == null ) return;

			foreach(var keyNode in descendingOrderedNodes) {
				keyNode.Value.PrintHierarchy(sb, indent+1);
			}
		}

        /// <summary>
        /// Generates a string giving timing information for this single node, including
        /// total milliseconds spent on the piece of ink, the time spent within itself
        /// (v.s. spent in children), as well as the number of samples (instruction steps)
        /// recorded for both too.
        /// </summary>
        /// <value>The own report.</value>
		public string ownReport {
			get {
				var sb = new StringBuilder();
				sb.Append("total ");
				sb.Append(Profiler.FormatMillisecs(_totalMillisecs));
				sb.Append(", self ");
				sb.Append(Profiler.FormatMillisecs(_selfMillisecs));
				sb.Append(" (");
				sb.Append(_selfSampleCount);
				sb.Append(" self samples, ");
				sb.Append(_totalSampleCount);
				sb.Append(" total)");
				return sb.ToString();
			}
			
		}

		void Pad(StringBuilder sb, int spaces)
		{
			for(int i=0; i<spaces; i++) sb.Append("   ");
		}

        /// <summary>
        /// String is a report of the sub-tree from this node, but without any of the header information
        /// that's prepended by the Profiler in its Report() method.
        /// </summary>
		public override string ToString ()
		{
			var sb = new StringBuilder();
			PrintHierarchy(sb, 0);
			return sb.ToString();
		}

		Dictionary<string, ProfileNode> _nodes;
		double _selfMillisecs;
		double _totalMillisecs;
		int _selfSampleCount;
		int _totalSampleCount;
	}
}

