using System;
using System.Collections.Generic;
using System.Linq;

namespace Inklewriter.Parsed
{
	public class Path
	{
        public FlowLevel baseTargetLevel { 
            get { 
                if (baseLevelIsAmbiguous)
                    return FlowLevel.Story;
                else
                    return (FlowLevel) _baseTargetLevel;
            }
        }

        public bool baseLevelIsAmbiguous {
            get {
                return _baseTargetLevel == null;
            }
        }

        public string firstComponent {
            get {
                if (_components == null || _components.Count == 0)
                    return null;

                return _components [0];
            }
        }

        // TODO: Re-implement this, and intelligently!
        public Path debugSuggestedAlternative { 
            get {
                return null;
            }
        }

        public Path(FlowLevel baseFlowLevel, List<string> components)
        {
            _baseTargetLevel = baseFlowLevel;
            _components = components;
        }

        public Path(string ambiguousName)
        {
            _baseTargetLevel = null;
            _components = new List<string> ();
            _components.Add (ambiguousName);
        }
             
		public override string ToString ()
		{
            if (_components == null || _components.Count == 0) {
                if (baseTargetLevel == FlowLevel.WeavePoint)
                    return "-> <next gather point>";
                else
                    return "<invalid Path>";
            }

            var dotStr = string.Join (".", _components);

            string baseArrowStr;
            switch (baseTargetLevel) {
            case FlowLevel.Knot:
                baseArrowStr = "==>";
                break;
            case FlowLevel.Stitch:
                baseArrowStr = "=>";
                break;
            case FlowLevel.WeavePoint:
                baseArrowStr = "->";
                break;
            default:
                return dotStr;
            }

            return baseArrowStr + " " + dotStr;
		}

        public Parsed.Object ResolveFromContext(Parsed.Object context)
        {
            if (_components == null || _components.Count == 0) {
                return null;
            }
            
            var baseTargetObject = ResolveBaseTarget (context);
            if (baseTargetObject == null) {
                return null;
            }

            if (_components.Count > 1) {
                return ResolveTailComponents (baseTargetObject);
            }

            return baseTargetObject;
        }

        // Find the root object from the base, i.e. root from:
        //    root.sub1.sub2
        Parsed.Object ResolveBaseTarget(Parsed.Object context)
        {
            var firstComp = firstComponent;

            while (context != null) {

                var foundBase = TryGetChildFromContext (context, firstComp, _baseTargetLevel);
                if (foundBase != null)
                    return foundBase;

                context = context.parent;
            }

            return null;
        }

        // Find the final child from path given root, i.e.:
        //   root.sub.finalChild
        Parsed.Object ResolveTailComponents(Parsed.Object rootTarget)
        {
            Parsed.Object foundComponent = rootTarget;
            FlowLevel minimumExpectedLevel = (FlowLevel)(baseTargetLevel + 1);
            for (int i = 1; i < _components.Count; ++i) {
                var compName = _components [i];

                foundComponent = TryGetChildFromContext (foundComponent, compName, minimumExpectedLevel);
                if (foundComponent == null)
                    break;

                if( i < _components.Count-1 )
                    minimumExpectedLevel = (FlowLevel)(minimumExpectedLevel + 1);
            }

            return foundComponent;
        }

        Parsed.Object TryGetChildFromContext(Parsed.Object context, string childName, FlowLevel? childLevel)
        {
            bool ambiguousChildLevel = childLevel == null;

            var weaveContext = context as Weave;
            if ( (ambiguousChildLevel || childLevel == FlowLevel.WeavePoint) && weaveContext != null) {
                var foundWeavePoint = weaveContext.WeavePointNamed (childName);

                if (foundWeavePoint != null)
                    return (Parsed.Object) foundWeavePoint;

                if (!ambiguousChildLevel)
                    return null;
            }

            var flowContext = context as FlowBase;
            if (flowContext != null) {
                var shouldDeepSearch = flowContext.flowLevel == FlowLevel.Knot;
                return flowContext.ContentWithNameAtLevel (childName, childLevel, shouldDeepSearch);
            }

            return null;
        }
            
        FlowLevel? _baseTargetLevel;
        List<string> _components;
	}
}

