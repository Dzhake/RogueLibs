using System;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a <see cref="CustomBigQuest"/> builder, that attaches additional information to the big qest.</para>
    /// </summary>
    public class BigQuestBuilder
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="BigQuestBuilder"/> class with the specified <paramref name="metadata"/>.</para>
        /// </summary>
        /// <param name="metadata">The big quest metadata to use.</param>
        public BigQuestBuilder(CustomBigQuestMetadata metadata) => Metadata = metadata;
        /// <summary>
        ///   <para>The used big quest metadata.</para>
        /// </summary>
        public CustomBigQuestMetadata Metadata { get; }

        /// <summary>
        ///   <para>Gets the big quest's localizable name.</para>
        /// </summary>
        public CustomName? Name { get; private set; }
        /// <summary>
        ///   <para>Gets the big quest's localizable description.</para>
        /// </summary>
        public CustomName? Description { get; private set; }
        /// <summary>
        ///   <para>Gets the big quest's unlock.</para>
        /// </summary>
        public BigQuestUnlock? Unlock { get; private set; }

        /// <summary>
        ///   <para>Creates a localizable string with the specified localization <paramref name="info"/> to act as the big quest's name.</para>
        /// </summary>
        /// <param name="info">The localization data to initialize the localizable string with.</param>
        /// <returns>The current instance of <see cref="BigQuestBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentException">A localizable string that acts as the big quest's name already exists.</exception>
        public BigQuestBuilder WithName(CustomNameInfo info)
        {
            Name = RogueLibs.CreateCustomName(Metadata.Name + "_BQ", NameTypes.Unlock, info);
            return this;
        }

        /// <summary>
        ///   <para>Creates a localizable string with the specified localization <paramref name="info"/> to act as the big quest's description.</para>
        /// </summary>
        /// <param name="info">The localization data to initialize the localizable string with.</param>
        /// <returns>The current instance of <see cref="BigQuestBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentException">A localizable string that acts as the big quest's description already exists.</exception>
        public BigQuestBuilder WithDescription(CustomNameInfo info)
        {
            Description = RogueLibs.CreateCustomName($"D_{Metadata.Name}_BQ", NameTypes.Unlock, info);
            return this;
        }
        /// <summary>
        ///   <para>Creates a default <see cref="BigQuestUnlock"/> for the big quest, that is unlocked by default.</para>
        /// </summary>
        /// <returns>The current instance of <see cref="BigQuestBuilder"/>, for chaining purposes.</returns>
        public BigQuestBuilder WithUnlock() => WithUnlock(new BigQuestUnlock(Metadata.Name + "_BQ", true));

        /// <summary>
        ///   <para>Creates the specified <paramref name="unlock"/> for the big quest.</para>
        /// </summary>
        /// <param name="unlock">The unlock information to initialize.</param>
        /// <returns>The current instance of <see cref="BigQuestBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unlock"/> is <see langword="null"/>.</exception>
        public BigQuestBuilder WithUnlock(BigQuestUnlock unlock)
        {
            if (unlock is null) throw new ArgumentNullException(nameof(unlock));
            unlock.Unlock.unlockName = Metadata.Name + "_BQ";
            RogueLibs.CreateCustomUnlock(unlock);
            Unlock = unlock;
            return this;
        }
    }
}
