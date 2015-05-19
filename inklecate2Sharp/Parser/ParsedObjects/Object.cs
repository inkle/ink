using System;
using System.Collections.Generic;

namespace inklecate2Sharp.Parsed
{
	public class Object
	{
		public DebugMetadata debugMetadata { get; set; }

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
	}
}

