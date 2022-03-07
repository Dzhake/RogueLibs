﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a generic unlock in the game.</para>
    /// </summary>
    public abstract class UnlockWrapper : HookBase<Unlock>
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="UnlockWrapper"/> class with the specified <paramref name="name"/> and <paramref name="type"/>.</para>
        /// </summary>
        /// <param name="name">The name of the unlock.</param>
        /// <param name="type">The type of the unlock.</param>
        /// <param name="unlockedFromStart">Determines whether the unlock is unlocked by default.</param>
        protected UnlockWrapper(string name, string type, bool unlockedFromStart)
        {
            Instance = new Unlock(name, type, unlockedFromStart) { __RogueLibsCustom = this };
            Name = name;
            Type = type;
        }
        internal UnlockWrapper(Unlock unlock)
        {
            Instance = unlock;
            unlock.__RogueLibsCustom = this;
            Name = unlock.unlockName;
            Type = unlock.unlockType;
        }

        /// <summary>
        ///   <para>Gets the name of the unlock.</para>
        /// </summary>
        public string Name
        {
            get => Unlock.unlockName ?? throw new InvalidOperationException("The unlock was not fully set up - its name is set to null.");
            internal set => Unlock.unlockName = value;
        }
        /// <summary>
        ///   <para>Gets the type of the unlock.</para>
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///   <para>Gets the unlock that the wrapper is attached to.</para>
        /// </summary>
        public Unlock Unlock => Instance;

        /// <summary>
        ///   <para>Gets or sets whether the unlock is unlocked.</para>
        /// </summary>
        public virtual bool IsUnlocked { get => Unlock.unlocked; set => Unlock.unlocked = value; }
        /// <summary>
        ///   <para>Gets or sets the unlock cost of the unlock, in nuggets.</para>
        /// </summary>
        public int UnlockCost { get => Unlock.cost; set => Unlock.cost = value; }
        /// <summary>
        ///   <para>Gets or sets the loadout cost of the unlock, in nuggets.</para>
        /// </summary>
        public int LoadoutCost { get => Unlock.cost2; set => Unlock.cost2 = value; }
        /// <summary>
        ///   <para>Gets or sets the character creation cost of the unlock, in points.</para>
        /// </summary>
        public int CharacterCreationCost { get => Unlock.cost3; set => Unlock.cost3 = value; }

        /// <summary>
        ///   <para>Gets or sets whether the unlock is enabled and active in the game.</para>
        /// </summary>
        public abstract bool IsEnabled { get; set; }
        /// <summary>
        ///   <para>Gets or sets whether the unlock is available in the primary menus.</para>
        /// </summary>
        public abstract bool IsAvailable { get; set; }

        /// <summary>
        ///   <para>Gets or sets the list containing the cancellations of the unlock - conflicting unlocks.</para>
        /// </summary>
        public List<string> Cancellations { get => Unlock.cancellations; set => Unlock.cancellations = value; }
        /// <summary>
        ///   <para>Gets or sets the list containing the recommendations of the unlock - purely aesthetic.</para>
        /// </summary>
        public List<string> Recommendations { get => Unlock.recommendations; set => Unlock.recommendations = value; }
        /// <summary>
        ///   <para>Gets or sets the list containing the prerequisites of the unlock - unlocks that must be unlocked in order to unlock this one.</para>
        /// </summary>
        public List<string> Prerequisites { get => Unlock.prerequisites; set => Unlock.prerequisites = value; }

        /// <inheritdoc/>
        protected sealed override void Initialize() => SetupUnlock();
        /// <summary>
        ///   <para>Sets up the unlock.</para>
        /// </summary>
        public virtual void SetupUnlock() { }
        /// <summary>
        ///   <para>Determines whether the unlock can be unlocked right now, in terms of prerequisites and other requirements.</para>
        /// </summary>
        /// <returns><see langword="true"/>, if the unlock can be unlocked right now; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanBeUnlocked() => UnlockCost > -1
            && Unlock.prerequisites.TrueForAll(static c => gc.sessionDataBig.unlocks.Exists(u => u.unlockName == c && u.unlocked));
        /// <summary>
        ///   <para>Updates the unlock information of the unlock. When overriden, you must set the <see cref="Unlock.nowAvailable"/> field.</para>
        /// </summary>
        public virtual void UpdateUnlock()
        {
            if ((Unlock.nowAvailable = !Unlock.unlocked && CanBeUnlocked()) && UnlockCost is 0)
                gc.unlocks.DoUnlockForced(Name, Type);
        }

        /// <summary>
        ///   <para>Gets the displayed name of the unlock.</para>
        /// </summary>
        /// <returns>The localized name of the unlock.</returns>
        public virtual string GetName() => gc.nameDB.GetName(Name, Unlock.unlockNameType);
        /// <summary>
        ///   <para>Gets the displayed description of the unlock.</para>
        /// </summary>
        /// <returns>The localized description of the unlock.</returns>
        public virtual string GetDescription() => gc.nameDB.GetName(Name, Unlock.unlockDescriptionType);
        /// <summary>
        ///   <para>Gets the displayed image of the unlock.</para>
        /// </summary>
        /// <returns>The image of the unlock, if it exists; otherwise, <see langword="null"/>.</returns>
        public virtual Sprite? GetImage() => RogueFramework.ExtraSprites.TryGetValue(Name, out Sprite image) ? image : null;

        /// <summary>
        ///   <para>Gets the currently used instance of <see cref="GameController"/>.</para>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Usage of gc fields in SoR")]
        // ReSharper disable once InconsistentNaming
        public static GameController gc => GameController.gameController;

        public virtual bool ShowInPrerequisites => false;

    }
}
