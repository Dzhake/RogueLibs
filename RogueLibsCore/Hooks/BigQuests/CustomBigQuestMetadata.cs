using System;
using System.Collections.Generic;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents the <see cref="CustomBigQuest"/> type metadata.</para>
    /// </summary>
    public sealed class CustomBigQuestMetadata
    {
        /// <summary>
        ///   <para>Gets the custom big quest's name.</para>
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///   <para>Returns a <see cref="CustomName"/> associated with this big quest's name.</para>
        /// </summary>
        /// <returns>The <see cref="CustomName"/> associated with this big quest's name, if found; otherwise, <see langword="null"/>.</returns>
        public CustomName? GetName() => RogueLibs.NameProvider.FindEntry(Name, NameTypes.Unlock);
        /// <summary>
        ///   <para>Returns an <see cref="BigQuestUnlock"/> associated with this big quest.</para>
        /// </summary>
        /// <returns>The <see cref="BigQuestUnlock"/> associated with this big quest, if found; otherwise, <see langword="null"/>.</returns>
        public BigQuestUnlock? GetUnlock() => RogueLibs.GetUnlock<BigQuestUnlock>(Name);


        private static readonly Dictionary<Type, CustomBigQuestMetadata> infos = new();
        /// <summary>
        ///   <para>Gets the specified <see cref="CustomBigQuest"/> <paramref name="type"/>'s metadata.</para>
        /// </summary>
        /// <param name="type">The <see cref="CustomBigQuest"/> type to get the metadata for.</param>
        /// <returns>The specified <paramref name="type"/>'s metadata.</returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not a <see cref="CustomBigQuest"/>.</exception>
        public static CustomBigQuestMetadata Get(Type type) => infos.TryGetValue(type, out CustomBigQuestMetadata info) ? info : infos[type] = new CustomBigQuestMetadata(type);
        /// <summary>
        ///   <para>Gets the specified <typeparamref name="TBigQuest"/>'s metadata.</para>
        /// </summary>
        /// <typeparam name="TBigQuest">The <see cref="CustomBigQuest"/> type get the metadata for.</typeparam>
        /// <returns>The specified <typeparamref name="TBigQuest"/>'s metadata.</returns>
        public static CustomBigQuestMetadata Get<TBigQuest>() where TBigQuest : CustomBigQuest => Get(typeof(TBigQuest));
        private CustomBigQuestMetadata(Type type)
        {
            if (!typeof(CustomBigQuest).IsAssignableFrom(type))
                throw new ArgumentException($@"{nameof(type)} does not inherit from {nameof(CustomBigQuest)}!", nameof(type));

            Name = type.Name;
        }
    }
}
