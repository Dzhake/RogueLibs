﻿using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RogueLibsCore
{
    internal sealed partial class RogueLibsPlugin
    {
        private static readonly MethodInfo Quests_SpawnBigQuestCompletedText = AccessTools.Method(typeof(Quests), "SpawnBigQuestCompletedText");
        private static CustomBigQuest? quest => GameController.gameController.playerAgent.GetHook<CustomBigQuest>();

        public void PatchQuests()
        {
            Patcher.Postfix(typeof(Quests), nameof(Quests.CheckIfBigQuestCompleteFloor));
            Patcher.Postfix(typeof(QuestSlotBig), nameof(QuestSlotBig.GetQuestInfo));
            Patcher.Postfix(typeof(Quests), nameof(Quests.CheckIfBigQuestFinishedElevator));
            Patcher.Postfix(typeof(Quests), nameof(Quests.BigQuestUpdate));
            Patcher.Postfix(typeof(Quests), nameof(Quests.setupQuests));
            Patcher.Postfix(typeof(Quests), nameof(Quests.CheckIfBigQuestObject));

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

        public static void QuestSlotBig_GetQuestInfo(QuestSlotBig __instance)
        {
            if (quest is null) return;
            __instance.questDescription.text = quest.GetProgress();
        }

        public static void Quests_CheckIfBigQuestFinishedElevator(Agent myPlayer, ref bool __result)
        {
            CustomBigQuest? quest = myPlayer.GetHook<CustomBigQuest>();
            if (quest?.CheckCompleted() ?? false)
            {
                __result = true;
            }
        }

        public static void Quests_BigQuestUpdate(Quests __instance)
        {
            Agent agent = GameController.gameController.playerAgent;
            if (quest is null || agent.completingBigQuestLevel || agent.failingBigQuestLevel) return;
            if (!agent.completedBigQuestLevel && quest.CheckCompleted())
            {
                __instance.StartCoroutine((IEnumerator)Quests_SpawnBigQuestCompletedText.Invoke(__instance, [agent]));
            }
        }

        public static void Quests_setupQuests()
        {
            if (quest is null) return;
            quest.SetupQuestMarkers();
        }

        public static void Quests_CheckIfBigQuestObject(PlayfieldObject myObject, ref bool __result)
        {
            if (quest is null) return;
            __result = quest.IsBigQuestObject(myObject);
        }
    }
}
