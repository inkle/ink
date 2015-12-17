using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
    internal enum PushPopType 
    {
        Tunnel,
        Function
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class Pop : Runtime.Object
    {
        // For serialisation
        [JsonProperty("pop")]
        [UniqueJsonIdentifier]
        public string typeString {
            get {
                return SerialisationName (this.type);
            }
            set {
                this.type = SerialisedTypeFromName (value);
            }
        }

        public static string SerialisationName(PushPopType pushPopType)
        {
            switch (pushPopType) {
            case PushPopType.Tunnel:
                return "tun";
            case PushPopType.Function:
                return "func";
            }

            System.Diagnostics.Debug.Fail ("PushPopType wasn't recognised");
            return null;
        }

        public static PushPopType SerialisedTypeFromName(string name)
        {
            switch (name) {
            case "tun":
                return PushPopType.Tunnel;
            case "func":
                return PushPopType.Function;
            }

            System.Diagnostics.Debug.Fail ("PushPopType wasn't recognised");
            return PushPopType.Tunnel;
        }

        public PushPopType type;

        public Pop (PushPopType type)
        {
            this.type = type;
        }

        // For serialisation only
        public Pop()
        {
            this.type = PushPopType.Tunnel;
        }

        public override string ToString ()
        {
            return string.Format ("Pop {0}", this.type);
        }
    }
}

