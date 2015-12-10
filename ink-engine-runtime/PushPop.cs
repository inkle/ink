using Newtonsoft.Json;
using System;

namespace Ink.Runtime
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class PushPop : Runtime.Object
    {
        public enum Type
        {
            Tunnel,
            Function
        }

        public enum Direction
        {
            Push,
            Pop
        }

        // For serialisation
        [JsonProperty("isPop")]
        public bool isPop {
            get {
                return direction == Direction.Pop;
            }
            set {
                direction = value ? Direction.Pop : Direction.Push;
            }
        }

        // For serialisation
        [JsonProperty("pushPop")]
        [UniqueJsonIdentifier]
        public string typeString {
            get {
                return this.type.ToString ();
            }
            set {
                string[] enumNames = Enum.GetNames (typeof(Type));
                int enumIndex = Array.IndexOf (enumNames, value);
                this.type = (Type) Enum.GetValues(typeof(Type)).GetValue(enumIndex);
            }
        }


        public Type type;
        public Direction direction;

        public PushPop (Type type, Direction direction)
        {
            this.type = type;
            this.direction = direction;
        }

        // For serialisation only
        public PushPop()
        {
            this.type = Type.Tunnel;
            this.direction = Direction.Push;
        }

        public override string ToString ()
        {
            return string.Format ("Stack {0} ({1})", this.direction, this.type);
        }
    }
}

