

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
        ///   <para>Increases points by given amount if quest name matches quest name. Returns "true" if points were increased.</para>
        /// </summary>
        public bool TryIncreasePoints(BigQuestUnlock quest, int diff = 1, Agent? agent = null)
        {
            string questName = quest.Name;
            if (agent == null) agent = gc.playerAgent;

            if (agent.bigQuest == questName)
            {
                agent.oma.bigQuestObjectCount += diff;
                return true;
            }

            return false;
        }

        /// <summary>
        ///   <para>Method which should return true is quest is completed. Checks "gc.playerAgent.bigQuestObjectCountTemp >= gc.playerAgent.bigQuestObjectCountTotalTemp" by default</para>
        /// </summary>
        public virtual bool CheckCompleted()
        {
            return gc.playerAgent.bigQuestObjectCountTemp >= gc.playerAgent.bigQuestObjectCountTotalTemp;
        }

        public virtual void SetupQuestMarkers()
        {
            //TODO Quests.IEnumerator SetupQuestMarkers2(Quest newQuest, bool cantCreateOnClient1)
        }

        //public void CreateQuestMarker(PlayfieldObject myObject, Quest newQuest, string markerType, string homeBaseMarkerType)


    }
}
