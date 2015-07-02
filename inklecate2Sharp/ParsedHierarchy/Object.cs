using System.Collections.Generic;
using System.Text;

namespace Inklewriter.Parsed
{
	public abstract class Object
	{
        public Runtime.DebugMetadata debugMetadata { 
            get {
                if (_debugMetadata == null) {
                    if (parent != null) {
                        return parent.debugMetadata;
                    }
                }

                return _debugMetadata;
            }

            set {
                _debugMetadata = value;
            }
        }
        private Runtime.DebugMetadata _debugMetadata;

		public Parsed.Object parent { get; set; }
        public List<Parsed.Object> content { get; protected set; }

		private Runtime.Object _runtimeObject;
		public Runtime.Object runtimeObject
		{
			get {
				if (_runtimeObject == null) {
					_runtimeObject = GenerateRuntimeObject ();
                    _runtimeObject.debugMetadata = debugMetadata;
				}
				return _runtimeObject;
			}

			set {
				_runtimeObject = value;
			}
		}

        // virtual so that certian object types can return a different
        // path than just the path to the main runtimeObject.
        // e.g. a Choice returns a path to its content rather than
        // its outer container.
        public virtual Runtime.Path runtimePath
        {
            get {
                return runtimeObject.path;
            }
        }

        public Parsed.Path PathRelativeTo(Parsed.Object otherObj)
        {
            var ownAncestry = ancestry;
            var otherAncestry = otherObj.ancestry;

            Parsed.Object highestCommonAncestor = null;
            int minLength = System.Math.Min (ownAncestry.Count, otherAncestry.Count);
            for (int i = 0; i < minLength; ++i) {
                var a1 = ancestry [i];
                var a2 = otherAncestry [i];
                if (a1 == a2)
                    highestCommonAncestor = a1;
                else
                    break;
            }
                
            FlowBase commonFlowAncestor = highestCommonAncestor as FlowBase;
            if (commonFlowAncestor == null)
                commonFlowAncestor = highestCommonAncestor.ClosestFlowBase ();


            var pathComponents = new List<string> ();
            bool hasWeavePoint = false;
            FlowLevel baseFlow = FlowLevel.WeavePoint;

            var ancestor = this;
            while(ancestor != null && ancestor != commonFlowAncestor && !(ancestor is Story)) {

                if (ancestor == commonFlowAncestor)
                    break;

                if (!hasWeavePoint) {
                    var weavePointAncestor = ancestor as IWeavePoint;
                    if (weavePointAncestor != null && weavePointAncestor.name != null) {
                        pathComponents.Add (weavePointAncestor.name);
                        hasWeavePoint = true;
                        continue;
                    }
                }

                var flowAncestor = ancestor as FlowBase;
                if (flowAncestor != null) {
                    pathComponents.Add (flowAncestor.name);
                    baseFlow = flowAncestor.flowLevel;
                }

                ancestor = ancestor.parent;
            }

            pathComponents.Reverse ();

            if (pathComponents.Count > 0) {
                return new Path (baseFlow, pathComponents);
            }

            return null;
        }

        public List<Parsed.Object> ancestry
        {
            get {
                var result = new List<Parsed.Object> ();

                var ancestor = this.parent;
                while(ancestor != null) {
                    result.Add (ancestor);
                    ancestor = ancestor.parent;
                }

                result.Reverse ();

                return result;
            }
        }

        public string DescriptionOfScope()
        {
            var locationNames = new List<string> ();

            Parsed.Object ancestor = this;
            while (ancestor != null) {
                var ancestorFlow = ancestor as FlowBase;
                if (ancestorFlow != null && ancestorFlow.name != null) {
                    locationNames.Add ("'"+ancestorFlow.name+"'");
                }
                ancestor = ancestor.parent;
            }

            var scopeSB = new StringBuilder ();
            if (locationNames.Count > 0) {
                var locationsListStr = string.Join (", ", locationNames);
                scopeSB.Append (locationsListStr);
                scopeSB.Append (" and ");
            }

            scopeSB.Append( "at top scope");

            return scopeSB.ToString ();
        }

        // Return the object so that method can be chained easily
        public T AddContent<T>(T subContent) where T : Parsed.Object
        {
            if (content == null) {
                content = new List<Parsed.Object> ();
            }

            subContent.parent = this;
            content.Add (subContent);

            return subContent;
        }

        public void AddContent<T>(List<T> listContent) where T : Parsed.Object
        {
            foreach (var obj in listContent) {
                AddContent (obj);
            }
        }

        public T InsertContent<T>(int index, T subContent) where T : Parsed.Object
        {
            if (content == null) {
                content = new List<Parsed.Object> ();
            }

            subContent.parent = this;
            content.Insert (index, subContent);

            return subContent;
        }

        public delegate bool FindQueryFunc<T>(T obj);
        public T Find<T>(FindQueryFunc<T> queryFunc) where T : class
        {
            if (content == null)
                return null;
            
            foreach (var obj in content) {
                var tObj = obj as T;
                if (tObj != null && queryFunc (tObj) == true) {
                    return tObj;
                }

                var nestedResult = obj.Find (queryFunc);
                if (nestedResult != null)
                    return nestedResult;
            }

            return null;
        }


        public IList<T> FindAll<T>(FindQueryFunc<T> queryFunc) where T : class
        {
            var found = new List<T> ();

            FindAll (queryFunc, found);

            return found;
        }

        void FindAll<T>(FindQueryFunc<T> queryFunc, List<T> foundSoFar) where T : class
        {
            var tObj = this as T;
            if (tObj != null && queryFunc (tObj) == true) {
                foundSoFar.Add (tObj);
            }

            if (content == null)
                return;

            foreach (var obj in content) {
                obj.FindAll (queryFunc, foundSoFar);
            }
        }

		public abstract Runtime.Object GenerateRuntimeObject ();

        public virtual void ResolveReferences(Story context)
		{
            if (content != null) {
                foreach(var obj in content) {
                    obj.ResolveReferences (context);
                }
            }
		}

        public FlowBase ClosestFlowBase()
        {
            var ancestor = this.parent;
            while (ancestor != null) {
                if (ancestor is FlowBase) {
                    return (FlowBase)ancestor;
                }
                ancestor = ancestor.parent;
            }

            return null;
        }

        public virtual void Error(string message, Parsed.Object source = null, bool isWarning = false)
		{
			if (source == null) {
				source = this;
			}

			if (this.parent != null) {
				this.parent.Error (message, source, isWarning);
			}
		}

        public void Warning(string message, Parsed.Object source = null)
        {
            Error (message, source, isWarning: true);
        }
	}
}

