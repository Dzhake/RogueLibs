﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents the <see cref="CustomItem"/> type metadata.</para>
    /// </summary>
    public sealed class CustomItemMetadata
    {
        public Type Type { get; }
        /// <summary>
        ///   <para>Gets the custom item's name.</para>
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///   <para>Gets the custom item's categories.</para>
        /// </summary>
        public ReadOnlyCollection<string> Categories { get; }

        private static readonly ReadOnlyCollection<string> emptyCollection = new(Array.Empty<string>());
        /// <summary>
        ///   <para>Gets the collection of inventory checks, ignored by the custom item.</para>
        /// </summary>
        public ReadOnlyCollection<string> IgnoredChecks { get; } = emptyCollection;

        /// <summary>
        ///   <para>Returns a <see cref="CustomName"/> associated with this item's name.</para>
        /// </summary>
        /// <returns>The <see cref="CustomName"/> associated with this item's name, if found; otherwise, <see langword="null"/>.</returns>
        public CustomName? GetName() => RogueLibs.NameProvider.FindEntry(Name, NameTypes.Item);
        /// <summary>
        ///   <para>Returns a <see cref="CustomName"/> associated with this item's description.</para>
        /// </summary>
        /// <returns>The <see cref="CustomName"/> associated with this item's description, if found; otherwise, <see langword="null"/>.</returns>
        public CustomName? GetDescription() => RogueLibs.NameProvider.FindEntry(Name, NameTypes.Description);
        /// <summary>
        ///   <para>Returns an <see cref="ItemUnlock"/> associated with this item.</para>
        /// </summary>
        /// <returns>The <see cref="ItemUnlock"/> associated with this item, if found; otherwise, <see langword="null"/>.</returns>
        public ItemUnlock? GetUnlock() => RogueLibs.GetUnlock<ItemUnlock>(Name);
        internal RogueSprite? sprite;
        /// <summary>
        ///   <para>Returns a <see cref="RogueSprite"/> that represents this item. Note that it works only on sprites initialized with <see cref="ItemBuilder.WithSprite(byte[], Rect, float)"/> or one of its overloads.</para>
        /// </summary>
        /// <returns>The <see cref="RogueSprite"/> that represents this item, if found; otherwise, <see langword="null"/>.</returns>
        public RogueSprite? GetSprite() => sprite;

        private static readonly Dictionary<Type, CustomItemMetadata> infos = new Dictionary<Type, CustomItemMetadata>();
        /// <summary>
        ///   <para>Gets the specified <see cref="CustomItem"/> <paramref name="type"/>'s metadata.</para>
        /// </summary>
        /// <param name="type">The <see cref="CustomItem"/> type to get the metadata for.</param>
        /// <returns>The specified <paramref name="type"/>'s metadata.</returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not a <see cref="CustomItem"/>.</exception>
        public static CustomItemMetadata Get(Type type) => infos.TryGetValue(type, out CustomItemMetadata info) ? info : infos[type] = new CustomItemMetadata(type);
        /// <summary>
        ///   <para>Gets the specified <typeparamref name="TItem"/>'s metadata.</para>
        /// </summary>
        /// <typeparam name="TItem">The <see cref="CustomItem"/> type get the metadata for.</typeparam>
        /// <returns>The specified <typeparamref name="TItem"/>'s metadata.</returns>
        public static CustomItemMetadata Get<TItem>() where TItem : CustomItem => Get(typeof(TItem));

        private CustomItemMetadata(Type type)
        {
            if (!typeof(CustomItem).IsAssignableFrom(type))
                throw new ArgumentException($"{nameof(type)} does not inherit from {nameof(CustomItem)}!", nameof(type));

            Type = type;
            ItemNameAttribute? nameAttr = type.GetCustomAttributes<ItemNameAttribute>().FirstOrDefault();
            Name = nameAttr?.Name ?? type.Name;

            string[] categories = type.GetCustomAttributes<ItemCategoriesAttribute>()
                                      .SelectMany(static c => c.Categories).Distinct().ToArray();
            if (categories.Length == 0 && !typeof(CustomAbility).IsAssignableFrom(type))
                RogueFramework.LogWarning($"Type {type} does not have any {nameof(ItemCategoriesAttribute)}!");
            Categories = new ReadOnlyCollection<string>(categories);

            IEnumerable<string> typeIgnores = type.GetCustomAttributes<IgnoreChecksAttribute>().SelectMany(static a => a.IgnoredChecks);

            List<MethodInfo> methods = new List<MethodInfo>();
            if (typeof(IItemUsable).IsAssignableFrom(type))
            {
                methods.Add(type.GetInterfaceMethod(typeof(IItemUsable),
                    nameof(IItemUsable.UseItem))!);
            }
            if (typeof(IItemCombinable).IsAssignableFrom(type))
            {
                methods.AddRange(type.GetInterfaceMethods(typeof(IItemCombinable),
                    nameof(IItemCombinable.CombineFilter),
                    nameof(IItemCombinable.CombineItems),
                    nameof(IItemCombinable.CombineTooltip),
                    nameof(IItemCombinable.CombineCursorText))!);
            }
            if (typeof(IItemTargetable).IsAssignableFrom(type))
            {
                methods.AddRange(type.GetInterfaceMethods(typeof(IItemTargetable),
                    nameof(IItemTargetable.TargetFilter),
                    nameof(IItemTargetable.TargetObject),
                    nameof(IItemTargetable.TargetCursorText))!);
            }
            if (typeof(IItemTargetableAnywhere).IsAssignableFrom(type))
            {
                methods.AddRange(type.GetInterfaceMethods(typeof(IItemTargetableAnywhere),
                    nameof(IItemTargetableAnywhere.TargetFilter),
                    nameof(IItemTargetableAnywhere.TargetPosition),
                    nameof(IItemTargetableAnywhere.TargetCursorText))!);
            }
            IEnumerable<string> methodIgnores = methods
                .SelectMany(static m => m.GetCustomAttributes<IgnoreChecksAttribute>()
                .SelectMany(static a => a.IgnoredChecks));

            string[] ignoredChecks = typeIgnores.Concat(methodIgnores).Distinct().ToArray();
            if (ignoredChecks.Length > 0)
                IgnoredChecks = new ReadOnlyCollection<string>(ignoredChecks);
        }
    }
    /// <summary>
    ///   <para>Specifies a different name for the custom item to use.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ItemNameAttribute : Attribute
    {
        /// <summary>
        ///   <para>Gets the custom item's name.</para>
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="ItemNameAttribute"/> class with the specified <paramref name="name"/>.</para>
        /// </summary>
        /// <param name="name">The custom item's name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public ItemNameAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
    }
    /// <summary>
    ///   <para>Specifies custom item's categories.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ItemCategoriesAttribute : Attribute
    {
        /// <summary>
        ///   <para>Gets the collection of custom item's categories.</para>
        /// </summary>
        public ReadOnlyCollection<string> Categories { get; }
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="ItemCategoriesAttribute"/> class with the specified <paramref name="categories"/>.</para>
        /// </summary>
        /// <param name="categories">The custom item's categories.</param>
        public ItemCategoriesAttribute(params string[] categories)
        {
            if (categories is null) throw new ArgumentNullException(nameof(categories));
            Categories = new ReadOnlyCollection<string>(Array.FindAll(categories, static c => c is not null));
        }
    }
    /// <summary>
    ///   <para>Indicates that the custom item should ignore certain inventory checks.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class IgnoreChecksAttribute : Attribute
    {
        /// <summary>
        ///   <para>Gets the collection of inventory checks ignored by the item.</para>
        /// </summary>
        public ReadOnlyCollection<string> IgnoredChecks { get; }
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="IgnoreChecksAttribute"/> class with the specified <paramref name="ignoreChecks"/>.</para>
        /// </summary>
        /// <param name="ignoreChecks">The inventory checks to ignore.</param>
        public IgnoreChecksAttribute(params string[] ignoreChecks)
        {
            if (ignoreChecks is null) throw new ArgumentNullException(nameof(ignoreChecks));
            IgnoredChecks = new ReadOnlyCollection<string>(Array.FindAll(ignoreChecks, static c => c is not null));
        }
    }
}
