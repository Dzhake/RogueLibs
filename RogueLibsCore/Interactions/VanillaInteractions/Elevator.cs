﻿using System.Reflection;
using HarmonyLib;

namespace RogueLibsCore
{
    internal static partial class VanillaInteractions
    {
        [Include]
        private static void Patch_Elevator()
        {
            Patch<Elevator>(Params1);
            PatchInteract<Elevator>();

            RogueLibs.CreateCustomName("OpenFloorsMenu", NameTypes.Interface, new CustomNameInfo
            {
                English = "Choose a Floor",
                Russian = @"Выбрать этаж",
            });
            RogueLibs.CreateCustomName("GoIntoTheCity", NameTypes.Interface, new CustomNameInfo
            {
                English = "Go Into The City",
                Russian = @"Отправиться в город",
            });

            RogueInteractions.CreateProvider<Elevator>(static h =>
            {
                if (h.Helper.interactingFar) return;

                if (h.gc.levelType == "HomeBase")
                {
                    if (h.Object.extraVar == 1)
                    {
                        h.AddImplicitButton("OpenFloorsMenu", static m =>
                        {
                            m.Object.ShowScrollingMenu("Floors");
                            m.StopInteraction();
                        });
                    }
                    else if (h.Object.extraVar == 2)
                    {
                        h.SetStopCallback(static m => m.Agent.SayDialogue("ElevatorWontGoDown"));
                    }
                    else
                    {
                        h.AddImplicitButton("GoIntoTheCity", static m =>
                        {
                            m.gc.mainGUI.ShowCharacterSelect();
                            m.StopInteraction();
                            if (m.gc.multiplayerMode)
                            {
                                if (m.gc.serverPlayer)
                                {
                                    m.gc.playerAgent.objectMult.SendChatAnnouncement("WantsToGo", "", "");
                                    m.gc.playerAgent.objectMult.RpcForceShowCharacterSelect();
                                    return;
                                }
                                m.gc.playerAgent.objectMult.CmdForceShowCharacterSelect("");
                            }
                        });

                    }
                }
                else if (h.gc.levelType == "Tutorial")
                {
                    if (h.Object.extraVar == 1)
                    {
                        h.AddImplicitButton("ElevatorGoUp", static m =>
                        {
                            m.gc.exitPoint.TryToExit(m.Agent);
                            m.StopInteraction();
                        });
                    }
                    else
                    {
                        h.SetStopCallback(static m => m.Agent.SayDialogue("ElevatorWontGoDown"));
                    }
                }
                else
                {
                    if (h.Object.elevatorLocation == 1)
                    {
                        if (!h.gc.exitPoint.DetermineIfCanBodyguardExit(h.Agent))
                        {
                            h.SetStopCallback(static m => m.Agent.SayDialogue("ElevatorWontGoUpBodyguard"));
                            return;
                        }
                        if (!h.gc.exitPoint.DetermineIfCanExit())
                        {
                            h.SetStopCallback(static m => m.Agent.SayDialogue("ElevatorWontGoUp"));
                        }
                        else
                        {
                            bool showingConfirm = (bool)elevatorShowingSecondButtonSet.GetValue(h.Object);
                            h.SetSideEffect(static m => elevatorShowingSecondButtonSet.SetValue(m.Object, false));
                            h.AddButton("ElevatorGoUp", m =>
                            {
                                if (m.Object.BigQuestRunning(m.Agent) && !showingConfirm)
                                {
                                    elevatorShowingSecondButtonSet.SetValue(m.Object, true);
                                    m.Object.SetObjectNameDisplay(m.Agent);
                                    return;
                                }
                                m.gc.exitPoint.TryToExit(m.Agent);
                                m.Agent.mainGUI.invInterface.justPressedInteract = false;
                                m.StopInteraction();
                            });
                        }
                    }
                    else
                    {
                        h.SetStopCallback(static m => m.Agent.SayDialogue("ElevatorWontGoDown"));
                    }
                }
            });
        }
        private static readonly FieldInfo elevatorShowingSecondButtonSet = AccessTools.Field(typeof(Elevator), "showingSecondButtonSet");
    }
}
