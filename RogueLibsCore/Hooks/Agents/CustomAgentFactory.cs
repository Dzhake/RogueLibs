using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a factory of <see cref="CustomAgent"/> hooks.</para>
    /// </summary>
    public sealed class CustomAgentFactory : HookFactoryBase<Agent>
    {
        private readonly Dictionary<string, AgentEntry> _agentsDict = [];

        /// <inheritdoc/>
        public override bool TryCreate(Agent? instance, [NotNullWhen(true)] out IHook<Agent>? hook)
        {
            hook = null;
            if (instance?.agentName != null && _agentsDict.TryGetValue(instance.agentName, out var entry))
            {
                hook = entry.Initializer();
                if (hook is CustomAgent custom) custom.Metadata = entry.Metadata;
            }
            return hook != null;
        }

        /// <summary>
        ///   <para>Adds the specified <typeparamref name="TAgent"/> type to the factory.</para>
        /// </summary>
        /// <typeparam name="TAgent">The <see cref="CustomAgent"/> type to add.</typeparam>
        /// <returns>The added agent's metadata.</returns>
        public CustomAgentMetadata AddAgent<TAgent>() where TAgent : CustomAgent, new()
        {
            var metadata = CustomAgentMetadata.Get<TAgent>();
            if (RogueFramework.IsDebugEnabled(DebugFlags.Agents))
                RogueFramework.LogDebug($"Created custom agent {typeof(TAgent)} ({metadata.Name}).");
            _agentsDict.Add(metadata.Name, new AgentEntry { Initializer = static () => new TAgent(), Metadata = metadata });
            return metadata;
        }

        private struct AgentEntry
        {
            public Func<IHook<Agent>> Initializer;
            public CustomAgentMetadata Metadata;
        }
    }
}
