using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a factory of <see cref="CustomAgent"/> hooks.</para>
    /// </summary>
    public sealed class CustomAgentPartFactory : HookFactoryBase<CustomAgent>
    {
        private readonly Dictionary<string, AgentPartEntry> _agentPartsDict = [];

        /// <inheritdoc/>
        public override bool TryCreate(CustomAgent? instance, [NotNullWhen(true)] out IHook<CustomAgent>? hook)
        {
            hook = null;
            if (instance != null && _agentPartsDict.TryGetValue(instance.Agent.name, out var entry))
            {
                hook = entry.Initializer();
                if (hook is CustomAgentPart custom) custom.Metadata = entry.Metadata;
            }
            return hook != null;
        }

        /// <summary>
        ///   <para>Adds the specified <typeparamref name="TAgentPart"/> type to the factory.</para>
        /// </summary>
        /// <typeparam name="TAgentPart">The <see cref="CustomAgentPart"/> type to add.</typeparam>
        /// <returns>The added agent's metadata.</returns>
        public CustomAgentPartMetadata AddAgentPart<TAgentPart>() where TAgentPart : CustomAgentPart, new()
        {
            var metadata = CustomAgentPartMetadata.Get<TAgentPart>();
            if (RogueFramework.IsDebugEnabled(DebugFlags.Agents))
                RogueFramework.LogDebug($"Created custom agent part {typeof(TAgentPart)} ({metadata.Name}).");
            _agentPartsDict.Add(metadata.Name, new AgentPartEntry { Initializer = static () => new TAgentPart(), Metadata = metadata });
            return metadata;
        }

        private struct AgentPartEntry
        {
            public Func<IHook<CustomAgent>> Initializer;
            public CustomAgentPartMetadata Metadata;
        }
    }
}
