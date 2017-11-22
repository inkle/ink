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
		internal ProfileNode rootNode {
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
			sb.AppendFormat("RESTORING: {0}\n", FormatMillisecs(_restoreTotal));
			sb.AppendFormat("OTHER: {0}\n", FormatMillisecs(_continueTotal - (_stepTotal + _snapTotal + _restoreTotal)));
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
				var objPath = callstack.elements[i].currentObject.path;
				string stackElementName = "";

				for(int c=0; c<objPath.componentCount; c++) {
					var comp = objPath.GetComponent(c);
					if( !comp.isIndex ) {
						stackElementName = comp.name;
						break;
					}
				}

				stack[i] = stackElementName;
			}
				
			_currStepStack = stack;

			var currObj = callstack.currentElement.currentObject ?? callstack.currentElement.currentContainer;

			_currStepDetails = new StepDetails {
				type = currObj.GetType().Name,
				detail = currObj.ToString()
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
		internal string StepLengthReport()
		{
			var sb = new StringBuilder();

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

			var maxStepTimes = _stepDetails
				.OrderByDescending(d => d.time)
				.Select(d => d.detail + ":" + d.time + "ms")
				.Take(100)
				.ToArray();

			sb.AppendLine("MAX STEP TIMES: "+string.Join("\n", maxStepTimes));

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

		internal void PreRestore() {
			_restoreWatch.Reset();
			_restoreWatch.Start();
		}

		internal void PostRestore() {
			_restoreWatch.Stop();
			_restoreTotal += Millisecs(_restoreWatch);
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
		Stopwatch _restoreWatch = new Stopwatch();

		double _continueTotal;
		double _snapTotal;
		double _stepTotal;
		double _restoreTotal;

		string[] _currStepStack;
		StepDetails _currStepDetails;
		ProfileNode _rootNode;
		int _numContinues;

		struct StepDetails {
			public string type;
			public string detail;
			public double time;
		}
		List<StepDetails> _stepDetails = new List<StepDetails>();

		static double _millisecsPerTick = 1000.0 / Stopwatch.Frequency;
	}


	internal class ProfileNode {
		public readonly string key;

        // Horribly hacky field only used by ink unity integration,
        // but saves constructing an entire data structure that mirrors
        // the one in here purely to store the state of whether each
        // node in the UI has been opened or not.
        #pragma warning disable 0649
        public bool openInUI;
        #pragma warning restore 0649

		public bool hasChildren {
			get {
				return _nodes != null && _nodes.Count > 0;
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

