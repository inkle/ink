using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public abstract class Object
	{
		public DebugMetadata debugMetadata { get; set; }

		public Parsed.Object parent { get; set; }

		private Runtime.Object _runtimeObject;
		public Runtime.Object runtimeObject
		{
			get {
				if (_runtimeObject == null) {
					_runtimeObject = GenerateRuntimeObject ();
				}
				return _runtimeObject;
			}

			set {
				_runtimeObject = value;
			}
		}

		public virtual Runtime.Object GenerateRuntimeObject()
		{
			return null;
		}

		public virtual void ResolvePaths()
		{

		}

		public virtual Parsed.Object ResolvePath(Path path)
		{
			if (parent == null) {
				return null;
			} else {
				return parent.ResolvePath (path);
			}
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
}

