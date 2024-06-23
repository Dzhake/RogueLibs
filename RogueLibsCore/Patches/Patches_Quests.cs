using System;
using System.Collections;
using UnityEngine;

namespace RogueLibsCore
{
    internal sealed partial class RogueLibsPlugin
    {
        public void PatchQuests()
        {
            Patcher.Postfix(typeof(Quests), nameof(Quests.CheckIfBigQuestCompleteFloor));

            Patcher.AnyErrors();
        }

        public static void Quests_CheckIfBigQuestCompleteFloor(Agent myPlayer, ref bool __result)
        {
            CustomBigQuest? hook = myPlayer.GetHook<CustomBigQuest>();
            if (hook != null)
            {
                if (hook.CheckCompleted())
                {
                    __result = true;
                }
            }
        }
    }
}
