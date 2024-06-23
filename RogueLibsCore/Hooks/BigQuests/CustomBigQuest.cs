

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
        ///   This should return true is quest is failed.
        /// </summary>
        public virtual bool CheckFailed()
        {
            return false;
        }

        public virtual void SetupQuestMarkers()
        {
            //TODO Quests.IEnumerator SetupQuestMarkers2(Quest newQuest, bool cantCreateOnClient1)
        }

        public abstract string GetProgress();

        //public void CreateQuestMarker(PlayfieldObject myObject, Quest newQuest, string markerType, string homeBaseMarkerType)


    }
}
