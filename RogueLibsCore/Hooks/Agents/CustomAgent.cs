using System;
using System.Linq;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a custom agent.</para>
    /// </summary>
    public abstract class CustomAgent : HookBase<Agent>
    {
        /// <summary>
        ///   <para>Gets the current <see cref="Agent"/> instance.</para>
        /// </summary>
        public Agent Agent => Instance;

        private AgentColors? colors;
        /// <summary>
        ///   <para>Gets the custom agent's colors settings.</para>
        /// </summary>
        public AgentColors Colors => colors ??= new AgentColors(this);
        /// <summary>
        ///   <para>Gets the custom agent's metadata.</para>
        /// </summary>
        public CustomAgentMetadata Metadata { get; internal set; } = null!; // initialized immediately in CustomAgentFactory

        /// <inheritdoc/>
        protected sealed override void Initialize()
        {
            Agent.strengthStatMod = RogueFramework.SpecialInt;
            Agent.enduranceStatMod = RogueFramework.SpecialInt;
            Agent.accuracyStatMod = RogueFramework.SpecialInt;
            Agent.speedStatMod = RogueFramework.SpecialInt;

            Agent.inhuman = false;
            Agent.wontFlee = false;
            Agent.cantChallengeToFight = true;
            Agent.preventsMindControl = false;

            Agent.copBot = false;
            Agent.hackable = false;

            Agent.modMeleeSkill = RogueFramework.SpecialInt;
            Agent.modGunSkill = RogueFramework.SpecialInt;
            Agent.modToughness = RogueFramework.SpecialInt;
            Agent.modVigilant = RogueFramework.SpecialInt;

            Agent.agentCategories.Clear();
            Agent.agentCategories.AddRange(Metadata.Categories);

            Agent.SetupSpecialInvDatabase();

            if (Agent.defaultGoal == string.Empty || Agent.defaultGoal == "None")
                Agent.SetDefaultGoal("Wander");

            Agent.agentHitboxScript.hasSetup = true;

            Agent.customCharacterData.facialHair = "None";

            if (Metadata.bodySprite != null)
            {
                Agent.shirtless = false;
                Agent.customCharacterData.bodyType = $"{Metadata.Name}";
            }
            else
            {
                RogueFramework.LogWarning($"Custom agent {GetType()} doesn't have body sprite!");
            }

            if (Metadata.headSprite != null)
                Agent.agentHitboxScript.headPieceType = Metadata.Name;
            else
                RogueFramework.LogWarning($"Custom agent {GetType()} doesn't have head sprite!");

            try
            {
                SetupAgent();
            }
            catch (Exception e)
            {
                RogueFramework.LogError(e, "SetupAgent", this, Agent); // maybe need rename to SetupAgent
            }

            if (Agent.healthMax <= 0)
            {
                RogueFramework.LogWarning($"Custom agent {GetType()} healthMax <= 0!");
                Agent.healthMax = 80;
            }

            Agent.health = Agent.healthMax;

            if (Agent.strengthStatMod is RogueFramework.SpecialInt)
                Agent.strengthStatMod = 0;

            if (Agent.enduranceStatMod is RogueFramework.SpecialInt)
                Agent.enduranceStatMod = 1;

            if (Agent.accuracyStatMod is RogueFramework.SpecialInt)
                Agent.accuracyStatMod = 1;

            if (Agent.speedStatMod is RogueFramework.SpecialInt)
                Agent.speedStatMod = 1;


            if (Agent.modMeleeSkill is RogueFramework.SpecialInt)
                Agent.modMeleeSkill = 0;

            if (Agent.modGunSkill is RogueFramework.SpecialInt)
                Agent.modGunSkill = 0;

            if (Agent.modToughness is RogueFramework.SpecialInt)
                Agent.modToughness = 0;

            if (Agent.modVigilant is RogueFramework.SpecialInt)
                Agent.modVigilant = 0;

            if (!Agent.agentCategories.Any())
            {
                RogueFramework.LogWarning($"Custom agent {GetType()} doesn't have any agent categories!");
                Agent.setInitialCategories = true;
            }

            Agent.agentActive = true;
            Agent.SetBrainActive(true);
        }
        /// <summary>
        ///   <para>The method that is called when the agent's stats are set up.</para>
        /// </summary>
        public abstract void SetupAgent();
    }
}
