

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a custom big quest.</para>
    /// </summary>
    public abstract class CustomBigQuest : HookBase<Agent>
    {
        public static GameController gc => GameController.gameController;
        /// <summary>
        ///   <para>Gets the current <see cref="Agent"/> instance.</para>
        /// </summary>
        public Agent agent => Instance;
        /// <summary>
        ///   <para>Gets the custom big quest's metadata.</para>
        /// </summary>
        public CustomBigQuestMetadata Metadata { get; internal set; } = null!; // initialized immediately in CustomBigQuestFactory

        /// <summary>
        ///   <para>Get an <see cref="BigQuestUnlock"/> associated with this item.</para>
        /// </summary>
        public BigQuestUnlock? Unlock => Metadata.GetUnlock();

        /// <summary>
        ///   <para>Current quest progress.</para>
        /// </summary>
        public int Current;
        /// <summary>
        ///   <para>Total quest progress required.</para>
        /// </summary>
        public int Total;

        /// <summary>
        ///   <para>Method which should return true is quest is completed. Checks "gc.playerAgent.bigQuestObjectCountTemp >= gc.playerAgent.bigQuestObjectCountTotalTemp" by default.</para>
        /// </summary>
        public virtual bool CheckCompleted()
        {
            return Current >= Total;
        }

        /// <summary>
        ///   <para>Setup quest markers here</para>
        /// </summary>
        public abstract void SetupQuestMarkers();

        /// <summary>
        ///   <para>This should return text which will be displayed in map menu</para>
        /// </summary>
        public abstract string GetProgress();
    }
}
