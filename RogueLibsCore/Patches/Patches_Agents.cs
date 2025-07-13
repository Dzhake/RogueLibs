using System;

namespace RogueLibsCore
{
    internal sealed partial class RogueLibsPlugin
    {
        public void PatchAgents()
        {
            // GetLastFiredBullet() extension
            Patcher.Postfix(typeof(Gun), nameof(Gun.spawnBullet),
                new Type[5] { typeof(bulletStatus), typeof(InvItem), typeof(int), typeof(bool), typeof(string) });

            // Create and initialize agent hooks
            Patcher.Postfix(typeof(Agent), nameof(Agent.SetupAgentStats));

            // Initialize agent sprites
            Patcher.Postfix(typeof(AgentHitbox), nameof(AgentHitbox.SetupBodyStrings));

            // Patches for agent state related features
            Patcher.Postfix(typeof(AgentHitbox), nameof(AgentHitbox.EnterWater));
            Patcher.Postfix(typeof(AgentHitbox), nameof(AgentHitbox.ExitWater));

            // Fix WB and WBH agent sprites
            //Patcher.Postfix(typeof(AgentHitbox), nameof(AgentHitbox.SetWBSprites));

            Patcher.Postfix(typeof(ObjectSprite), nameof(ObjectSprite.SetAgentOff));

            Patcher.AnyErrors();
        }

        public static void Gun_spawnBullet(Gun __instance, Bullet __result)
        {
            LastFiredBulletHook hook = __instance.agent.GetOrAddHook<LastFiredBulletHook>();
            hook.LastFiredBullet = __result;
        }

        public static void Agent_SetupAgentStats(Agent __instance)
        {
            HookSystem.DestroyHookController(__instance);
            var controller = __instance.GetHookController();

            foreach (var factory in RogueFramework.AgentFactories)
            {
                var hook = factory.TryCreateHook(__instance);
                if (hook != null) controller.AddHook(hook);
            }

            foreach (var factory in RogueFramework.BigQuestFactories)
            {
                var hook = factory.TryCreateHook(__instance);
                if (hook != null) controller.AddHook(hook);
            }
        }

        public static void AgentHitbox_SetupBodyStrings(AgentHitbox __instance)
        {
            var custom = __instance.agent.GetHook<CustomAgent>();
            if (custom == null) return;

            if (custom.Metadata._bodySprite != null)
            {
                __instance.agentBodyStrings.Clear();

                for (var i = 0; i < custom.Metadata._bodySprite.Length; i++)
                    __instance.agentBodyStrings.Add(custom.Metadata._bodySprite[i].Name);
            }

            if (custom.Metadata._headSprite != null)
            {
                __instance.headStrings.Clear();

                for (var i = 0; i < custom.Metadata._headSprite.Length; i++)
                    __instance.headStrings.Add(custom.Metadata._headSprite[i].Name);
            }
        }

        public static void AgentHitbox_EnterWater(AgentHitbox __instance)
        {
            var parts = __instance.agent.GetHooks<CustomAgentPart>();
            foreach (var part in parts)
            {
                try { ((IAgentPartWatertable)part).OnWaterEnter(); }
                catch (Exception e) { RogueFramework.LogError(e, nameof(IAgentPartWatertable.OnWaterEnter), part, __instance.agent); }
            }
        }

        public static void AgentHitbox_ExitWater(AgentHitbox __instance)
        {
            var parts = __instance.agent.GetHooks<CustomAgentPart>();
            foreach (var part in parts)
            {
                try { ((IAgentPartWatertable)part).OnWaterExit(); }
                catch (Exception e) { RogueFramework.LogError(e, nameof(IAgentPartWatertable.OnWaterExit), part, __instance.agent); }
            }
        }

        /*public static void AgentHitbox_SetWBSprites(AgentHitbox __instance)
        {
            var custom = __instance.agent.GetHook<CustomAgent>();
            if (custom == null) return;

            if (custom.Metadata.GetHeadSprites() != null)
            {
                if (!__instance.headWB.GetCurrentSpriteDef().name.Contains(custom.Metadata.Name))
                    __instance.headWB.SetSprite(custom.Metadata.Name + __instance.headWB.GetCurrentSpriteDef().name);

                if (!__instance.headWBH.GetCurrentSpriteDef().name.Contains(custom.Metadata.Name))
                    __instance.headWBH.SetSprite(custom.Metadata.Name + __instance.headWBH.GetCurrentSpriteDef().name);
            }
        }*/
    }
}
