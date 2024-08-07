﻿using System;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a <see cref="CustomDisaster"/> builder, that attaches additional information to the disaster.</para>
    /// </summary>
    public class DisasterBuilder
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="DisasterBuilder"/> class with the specified <paramref name="metadata"/>.</para>
        /// </summary>
        /// <param name="metadata">The disaster metadata to use.</param>
        public DisasterBuilder(CustomDisasterMetadata metadata) => Metadata = metadata;
        /// <summary>
        ///   <para>The used disaster metadata.</para>
        /// </summary>
        public CustomDisasterMetadata Metadata { get; }

        /// <summary>
        ///   <para>Gets the disaster's localizable name.</para>
        /// </summary>
        public CustomName? Name { get; private set; }
        /// <summary>
        ///   <para>Gets the disaster's localizable description.</para>
        /// </summary>
        public CustomName? Description { get; private set; }
        /// <summary>
        ///   <para>Gets the disaster's localizable first message.</para>
        /// </summary>
        public CustomName? Message1 { get; private set; }
        /// <summary>
        ///   <para>Gets the disaster's localizable second message.</para>
        /// </summary>
        public CustomName? Message2 { get; private set; }
        /// <summary>
        ///   <para>Gets the disaster's removal mutator unlock.</para>
        /// </summary>
        public RemovalMutator? removalMutator { get; private set; }

        /// <summary>
        ///   <para>Creates a localizable string with the specified localization <paramref name="info"/> to act as the disaster's name.</para>
        /// </summary>
        /// <param name="info">The localization data to initialize the localizable string with.</param>
        /// <returns>The current instance of <see cref="DisasterBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentException">A localizable string that acts as the disaster's name already exists.</exception>
        public DisasterBuilder WithName(CustomNameInfo info)
        {
            Name = RogueLibs.CreateCustomName($"LevelFeeling{Metadata.Name}_Name", NameTypes.Interface, info);
            return this;
        }
        /// <summary>
        ///   <para>Creates a localizable string with the specified localization <paramref name="info"/> to act as the disaster's description.</para>
        /// </summary>
        /// <param name="info">The localization data to initialize the localizable string with.</param>
        /// <returns>The current instance of <see cref="DisasterBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentException">A localizable string that acts as the disaster's description already exists.</exception>
        public DisasterBuilder WithDescription(CustomNameInfo info)
        {
            Description = RogueLibs.CreateCustomName($"LevelFeeling{Metadata.Name}_Desc", NameTypes.Description, info);
            return this;
        }
        /// <summary>
        ///   <para>Creates a localizable string with the specified localization <paramref name="info"/> to act as the disaster's message. Can be repeated to create up to two messages.</para>
        /// </summary>
        /// <param name="info">The localization data to initialize the localizable string with.</param>
        /// <returns>The current instance of <see cref="DisasterBuilder"/>, for chaining purposes.</returns>
        /// <exception cref="ArgumentException">A localizable string that acts as the disaster's message already exists.</exception>
        /// <exception cref="InvalidOperationException">The maximum of two localizable messages have already been created.</exception>
        public DisasterBuilder WithMessage(CustomNameInfo info)
        {
            if (Message1 is null) Message1 = RogueLibs.CreateCustomName($"LevelFeeling{Metadata.Name}1", NameTypes.Description, info);
            else if (Message2 is null) Message2 = RogueLibs.CreateCustomName($"LevelFeeling{Metadata.Name}2", NameTypes.Description, info);
            else throw new InvalidOperationException("You can't specify more than two messages for a custom disaster!");
            return this;
        }
        /// <summary>
        ///   <para>Creates a removal mutator for this disaster.</para>
        /// </summary>
        /// <returns>The current instance of <see cref="DisasterBuilder"/>, for chaining purposes.</returns>
        public DisasterBuilder WithRemovalMutator(CustomNameInfo displayName = default)
        {
            RogueLibs.CreateCustomUnlock(removalMutator = new RemovalMutator("NoD_" + Metadata.Name, true, displayName)
            {
                IsAvailableInDailyRun = false,
            });

            return this;
        }

        public class RemovalMutator(string name, bool unlockedFromStart, CustomNameInfo displayName = default)
            : MutatorUnlock(name, unlockedFromStart)
        {
            public override string GetFancyName() => displayName.GetCurrent() ?? $"E_{Name}";
        }
    }
}
