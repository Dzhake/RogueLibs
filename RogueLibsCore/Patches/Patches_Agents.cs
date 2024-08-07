﻿using System;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine;

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

            // Fix WB and WBH agent sprites
            Patcher.Postfix(typeof(AgentHitbox), nameof(AgentHitbox.SetWBSprites));

            Patcher.Postfix(typeof(ObjectSprite), nameof(ObjectSprite.SetAgentOff));

            Patcher.AnyErrors();
        }

        public static void Gun_spawnBullet(Gun __instance, Bullet __result)
        {
            LastFiredBulletHook hook = __instance.agent.GetHook<LastFiredBulletHook>() ?? __instance.agent.AddHook<LastFiredBulletHook>();
            hook.LastFiredBullet = __result;
        }

        public static void Agent_SetupAgentStats(Agent __instance)
        {
            HookSystem.DestroyHookController(__instance);

            IHookController controller = __instance.GetHookController();
            foreach (IHookFactory factory in RogueFramework.AgentFactories)
            {
                IHook? hook = factory.TryCreateHook(__instance);
                if (hook is not null) controller.AddHook(hook);
            }

            foreach (IHookFactory factory in RogueFramework.BigQuestFactories)
            {
                IHook? hook = factory.TryCreateHook(__instance);
                if (hook is not null) controller.AddHook(hook);
            }
        }
        public static void AgentHitbox_SetupBodyStrings(AgentHitbox __instance)
        {
            CustomAgent custom = __instance.agent.GetHook<CustomAgent>();
            if (custom != null)
            {
                if (custom.Metadata.GetBodySprites() != null)
                {
                    __instance.agentBodyStrings.Clear();
                    __instance.agentBodyStrings = new();

                    for (int i = 0; i < custom.Metadata.bodySprite.Length; i++)
                    {
                        __instance.agentBodyStrings.Add(custom.Metadata.bodySprite[i].Name);
                    }
                }
                if (custom.Metadata.GetHeadSprites() != null)
                {
                    __instance.headStrings.Clear();
                    __instance.headStrings = new();
                    for (int i = 0; i < custom.Metadata.headSprite.Length; i++)
                    {
                        __instance.headStrings.Add(custom.Metadata.headSprite[i].Name);
                    }
                }
            }
        }
        public static void AgentHitbox_SetWBSprites(AgentHitbox __instance)
        {
            CustomAgent custom = __instance.agent.GetHook<CustomAgent>();
            if (custom != null)
            {
                if (custom.Metadata.GetHeadSprites() != null)
                {
                    if (!__instance.headWB.GetCurrentSpriteDef().name.Contains(custom.Metadata.Name))
                        __instance.headWB.SetSprite(custom.Metadata.Name + __instance.headWB.GetCurrentSpriteDef().name);

                    if (!__instance.headWBH.GetCurrentSpriteDef().name.Contains(custom.Metadata.Name))
                        __instance.headWBH.SetSprite(custom.Metadata.Name + __instance.headWBH.GetCurrentSpriteDef().name);
                }
            }
        }
    }
}
