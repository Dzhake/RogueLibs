﻿using System.Linq;

namespace RogueLibsCore
{
    internal static partial class VanillaInteractions
    {
        [Include]
        private static void Patch_Television()
        {
            Patch<Television>(Params1);
            PatchInteractFar<Television>();

            RogueInteractions.CreateProvider<Television>(static h =>
            {
                if (!h.Helper.interactingFar) return;

                if (!h.Object.isHighVolume && h.gc.levelType != "Tutorial")
                {
                    h.AddButton("IncreaseVolume", static m => m.Object.IncreaseVolume());
                }
                h.AddButton("BlowUpTelevision", static m =>
                {
                    m.Object.BlowUpTelevision(m.Agent);
                    if (m.gc.levelType == "Tutorial")
                    {
                        m.gc.tutorial.TargetSomething();
                        foreach (Computer computer in m.gc.objectRealList.OfType<Computer>())
                            computer.openedTutorialDoor = true;
                    }
                    m.StopInteraction();
                });
            });
        }
    }
}
