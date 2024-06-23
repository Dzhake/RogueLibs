namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Indicates that this hook's LevelStart method should be called when level starts.</para>
    /// </summary>
    public interface ITriggerOnLevelStart
    {
        /// <summary>
        ///   <para>Method, which is called when level starts.</para>
        /// </summary>
        void LevelStart();
    }
}
