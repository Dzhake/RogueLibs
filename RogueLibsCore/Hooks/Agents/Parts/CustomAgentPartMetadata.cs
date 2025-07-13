using System;
using System.Collections.Generic;

namespace RogueLibsCore
{
    public sealed class CustomAgentPartMetadata
    {
        public string Name { get; }
        public int Id { get; }

        private static readonly List<string> _ids = ["Hair", "FacialHair", "Eyes", "Head", "Body", "Arm1", "Arm2", "Leg1", "Leg2"];

        private static readonly Dictionary<Type, CustomAgentPartMetadata> _infos = [];

        public static int GetId(string name) => _ids.IndexOf(name);

        public static CustomAgentPartMetadata Get(Type type) => _infos.TryGetValue(type, out var info) ? info : _infos[type] = new CustomAgentPartMetadata(type);

        public static CustomAgentPartMetadata Get<TAgentPart>() where TAgentPart : CustomAgentPart =>
            Get(typeof(TAgentPart));

        private CustomAgentPartMetadata(Type type)
        {
            if (!typeof(CustomAgent).IsAssignableFrom(type)) throw new ArgumentException($"{nameof(type)} does not inherit from {nameof(CustomAgentPart)}!", nameof(type));

            Name = type.Name;
            _ids.Add(Name);
            Id = _ids.Count;
        }
    }
}
