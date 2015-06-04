using System;
using System.Collections.Generic;

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

		public abstract Runtime.Object GenerateRuntimeObject ();

        public virtual void ResolveReferences(Story context)
		{

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

		public virtual void Error(string message, Parsed.Object source = null)
		{
			if (source == null) {
				source = this;
			}

			if (this.parent != null) {
				this.parent.Error (message, source);
			}
		}
	}

    public class Object<T> : Object where T : Runtime.Object, new()
    {
        public override Runtime.Object GenerateRuntimeObject ()
        {
            return new T ();
        }
    }
}

