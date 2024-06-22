using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Profiling.Memory.Experimental;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a factory of <see cref="CustomBigQuest"/> hooks.</para>
    /// </summary>
    public sealed class CustomBigQuestFactory : HookFactoryBase<Agent>
    {
        private readonly Dictionary<string, BigQuestEntry> bigQuestsDict = new();
        /// <inheritdoc/>
        public override bool TryCreate(Agent? instance, out IHook<Agent> hook)
        {
            if (instance?.bigQuest != null && bigQuestsDict.TryGetValue(instance.bigQuest, out BigQuestEntry entry))
            {
                hook = entry.Initializer();
                if (hook is CustomBigQuest custom)
                    custom.Metadata = entry.Metadata;
                return true;
            }
            hook = null;
            return false;
        }
        /// <summary>
        ///   <para>Adds the specified <typeparamref name="TBigQuest"/> type to the factory.</para>
        /// </summary>
        /// <typeparam name="TBigQuest">The <see cref="CustomBigQuest"/> type to add.</typeparam>
        /// <returns>The added agent's metadata.</returns>
        public CustomBigQuestMetadata AddQuest<TBigQuest>() where TBigQuest : CustomBigQuest, new()
        {
            CustomBigQuestMetadata metadata = CustomBigQuestMetadata.Get<TBigQuest>();
            bigQuestsDict.Add(metadata.Name, new BigQuestEntry { Initializer = static () => new TBigQuest(), Metadata = metadata });
            return metadata;
        }

        private struct BigQuestEntry
        {
            public Func<IHook<Agent>> Initializer;
            public CustomBigQuestMetadata Metadata;
        }
    }
}
