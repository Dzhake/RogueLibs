﻿using System.Collections;
using System.Collections.Generic;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>The implementation of the <see cref="IName"/> interface, used by the <see cref="CustomNameProvider"/>. Provides properties for getting and setting the 8 default languages' localization texts.</para>
    /// </summary>
    public class CustomName : IName, IUsesNameContainer
    {
        internal CustomName(string name, string type, string english)
        {
            Name = name;
            Type = type;
            container = english;
        }
        internal CustomName(string name, string type, IEnumerable<KeyValuePair<LanguageCode, string>> dictionary)
        {
            Name = name;
            Type = type;

            if (dictionary is IUsesNameContainer nameCont)
                container = NameContainer.Clone(nameCont.GetNameContainer());
            else
            {
                foreach (KeyValuePair<LanguageCode, string> entry in dictionary)
                    container = NameContainer.Set(container, entry.Key, entry.Value);
            }
        }

        /// <summary>
        ///   <para>Gets the localizable string's name.</para>
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///   <para>Gets the localizable string's type.</para>
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///   <para>Gets or sets the English localization text.</para>
        /// </summary>
        public string? English { get => this[LanguageCode.English]; set => this[LanguageCode.English] = value; }
        /// <summary>
        ///   <para>Gets or sets the Spanish localization text.</para>
        /// </summary>
        public string? Spanish { get => this[LanguageCode.Spanish]; set => this[LanguageCode.Spanish] = value; }
        /// <summary>
        ///   <para>Gets or sets the Chinese localization text.</para>
        /// </summary>
        public string? Chinese { get => this[LanguageCode.Chinese]; set => this[LanguageCode.Chinese] = value; }
        /// <summary>
        ///   <para>Gets or sets the German localization text.</para>
        /// </summary>
        public string? German { get => this[LanguageCode.German]; set => this[LanguageCode.German] = value; }
        /// <summary>
        ///   <para>Gets or sets the Brazilian localization text.</para>
        /// </summary>
        public string? Brazilian { get => this[LanguageCode.Brazilian]; set => this[LanguageCode.Brazilian] = value; }
        /// <summary>
        ///   <para>Gets or sets the French localization text.</para>
        /// </summary>
        public string? French { get => this[LanguageCode.French]; set => this[LanguageCode.French] = value; }
        /// <summary>
        ///   <para>Gets or sets the Russian localization text.</para>
        /// </summary>
        public string? Russian { get => this[LanguageCode.Russian]; set => this[LanguageCode.Russian] = value; }
        /// <summary>
        ///   <para>Gets or sets the Korean localization text.</para>
        /// </summary>
        public string? Korean { get => this[LanguageCode.Korean]; set => this[LanguageCode.Korean] = value; }

        private object? container;
        object? IUsesNameContainer.GetNameContainer() => container;

        /// <inheritdoc/>
        public string? this[LanguageCode language]
        {
            get => NameContainer.Get(container, language);
            set => container = NameContainer.Set(container, language, value);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<LanguageCode, string>> GetEnumerator()
            => NameContainer.Enumerate(container);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
