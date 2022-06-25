﻿using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RogueLibsCore
{
    public sealed partial class RogueLibsPlugin
    {
        public void PatchInteractions()
        {
            Patcher.Postfix(typeof(PlayfieldObject), "Awake");

            Patcher.Prefix(typeof(ObjectReal), nameof(ObjectReal.DetermineButtons), nameof(DetermineButtonsHook));
            Patcher.Prefix(typeof(ObjectReal), nameof(ObjectReal.PressedButton), nameof(PressedButtonHook),
                           new Type[2] { typeof(string), typeof(int) });
            Patcher.Prefix(typeof(ObjectReal), nameof(ObjectReal.Interact), nameof(InteractHook));
            Patcher.Prefix(typeof(ObjectReal), nameof(ObjectReal.InteractFar), nameof(InteractFarHook));

            Patcher.Prefix(typeof(PlayfieldObject), nameof(PlayfieldObject.StopInteraction), new Type[1] { typeof(bool) });

            Patcher.Prefix(typeof(InteractionHelper), "EnterDetails", nameof(InteractionHelper_EnterDetails_Prefix));
            Patcher.Finalizer(typeof(InteractionHelper), "EnterDetails", nameof(InteractionHelper_EnterDetails_Finalizer));
            Patcher.Prefix(typeof(InteractionHelper), nameof(InteractionHelper.UpdateInteractionHelper),
                           nameof(InteractionHelper_UpdateInteractionHelper_Prefix));
            Patcher.Finalizer(typeof(InteractionHelper), nameof(InteractionHelper.UpdateInteractionHelper),
                              nameof(InteractionHelper_UpdateInteractionHelper_Finalizer));

            MethodInfo interactableGetter = AccessTools.PropertyGetter(typeof(PlayfieldObject), nameof(PlayfieldObject.interactable));
            Harmony harmony = Patcher.GetHarmony();
            harmony.Patch(interactableGetter, new HarmonyMethod(AccessTools.Method(typeof(RogueLibsPlugin), nameof(PlayfieldObject_get_interactable))));

            Patcher.Prefix(typeof(InteractionHelper), nameof(InteractionHelper.canInteractCounter));

            Patcher.Prefix(typeof(Movement), nameof(Movement.HasLOSObject360));

            Patcher.Postfix(typeof(LockdownWall), nameof(LockdownWall.SetWallDown));
            Patcher.Postfix(typeof(LockdownWall), nameof(LockdownWall.SetWallDownAnim));

            VanillaInteractions.PatchAll();

            Patcher.AnyErrors();
        }

        private static InteractionModel GetOrCreateModel(PlayfieldObject __instance)
        {
            InteractionModel? model = __instance.GetHook<InteractionModel>();
            if (model is null)
            {
                Type myType = __instance.GetType();
                if (!interactionModelConstructors.TryGetValue(myType, out ConstructorInfo? ctor))
                {
                    Type modelType = typeof(InteractionModel<>).MakeGenericType(__instance.GetType());
                    interactionModelConstructors.Add(myType, ctor = modelType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, Type.EmptyTypes, null));
                }
                if (ctor is null)
                {
                    RogueFramework.LogError($"Error patching {__instance.GetType()}. Constructor is null for some reason.");
                    return null!;
                }
                __instance.AddHook(model = (InteractionModel)ctor.Invoke(Array.Empty<object>()));
            }
            return model;
        }

        private static readonly Dictionary<Type, ConstructorInfo> interactionModelConstructors = new Dictionary<Type, ConstructorInfo>();
        public static void PlayfieldObject_Awake(PlayfieldObject __instance)
        {
            GetOrCreateModel(__instance);
        }
        public static bool DetermineButtonsHook(PlayfieldObject __instance)
        {
            #region Re-implementing base.DetermineButtons()
            __instance.buttons.Clear();
            __instance.buttonPrices.Clear();
            __instance.buttonsExtra.Clear();
            #endregion

            GetOrCreateModel(__instance).OnDetermineButtons();
            return false;
        }
        public static bool PressedButtonHook(PlayfieldObject __instance, string buttonText)
        {
            #region Re-implementing base.PressedButton()
            if (__instance.interactingAgent != null && (__instance.interactingAgent.controllerType != "Keyboard" || __instance.interactingAgent.controllerType == "Keyboard" && __instance.gc.playerControl.keyCheck(buttonType.Interact, __instance.interactingAgent)) && __instance.interactingAgent.localPlayer)
            {
                __instance.interactingAgent.mainGUI.invInterface.justPressedInteract = true;
            }
            #endregion

            GetOrCreateModel(__instance).OnPressedButton(buttonText);
            return false;
        }

        public static bool InteractHook(PlayfieldObject __instance, Agent agent)
        {
            #region Re-implementing base.Interact(agent)
            // PlayfieldObject.Interact
            __instance.interactingAgent = agent;
            __instance.hasInteracted = true;
            __instance.someoneInteracting = true;
            __instance.DetermineButtons();

            if (__instance.playfieldObjectReal != null)
                __instance.gc.playerAgent.SetCheckUseWithItemsAgain(__instance.playfieldObjectReal);

            float size = !__instance.gc.serverPlayer && __instance.playfieldObjectType == "Agent" ? 1.7f : 1.3f;
            agent.interactionHelper.boxCollider2D.size = new Vector2(size, size);
            agent.interactionHelper.interactingLimbo = 0f;

            if (!__instance.gc.serverPlayer && __instance.gc.clientControlling)
                agent.interactionHelper.clientInteracting = true;
            if (agent.localPlayer && !__instance.isItem && (agent.statusEffects.hasTrait("RollerSkates") || agent.statusEffects.hasTrait("RollerSkates2")))
            {
                agent.rb.velocity = Vector2.zero;
            }
            if (__instance.playfieldObjectAgent != null && __instance.gc.serverPlayer && (__instance.playfieldObjectAgent.movement.curPhysicsType == "Ice" || __instance.playfieldObjectAgent.statusEffects.hasTrait("RollerSkates") || __instance.playfieldObjectAgent.statusEffects.hasTrait("RollerSkates2")))
            {
                __instance.playfieldObjectAgent.rb.velocity = Vector2.zero;
            }
            // ObjectReal.Interact
            __instance.playerInvDatabase = agent.GetComponent<InvDatabase>();
            #endregion

            if (__instance.buttons.Count > 0)
                __instance.ShowObjectButtons();
            return false;
        }
        public static bool InteractFarHook(PlayfieldObject __instance, Agent agent)
        {
            #region Re-implementing base.InteractFar(agent)
            // PlayfieldObject.InteractFar
            __instance.interactingAgent = agent;
            if (__instance.gc.multiplayerMode && !__instance.gc.serverPlayer)
            {
                agent.interactionHelper.clientInteracting = true;
                Debug.Log("CmdInteractFar");
                agent.objectMult.CallCmdInteractFar(__instance.objectNetID);
            }
            __instance.someoneInteracting = true;
            agent.interactionHelper.interactingFar = true;
            __instance.DetermineButtons();
            if (__instance.playfieldObjectReal != null)
                __instance.gc.playerAgent.SetCheckUseWithItemsAgain(__instance.playfieldObjectReal);
            agent.interactionHelper.interactionObject = __instance.objectSprite.go;
            // agent.worldSpaceGUI.ShowObjectNameDisplay();
            if (__instance.playfieldObjectType == "ObjectReal")
                agent.worldSpaceGUI.objectNameDisplayText.text = __instance.GetComponent<ObjectReal>().objectRealRealName;
            else if (__instance.playfieldObjectType == "Agent")
                agent.worldSpaceGUI.objectNameDisplayText.text = __instance.GetComponent<Agent>().agentRealName;
            agent.interactionHelper.interactingLimbo = 0f;
            // ObjectReal.InteractFar
            __instance.playerInvDatabase = agent.GetComponent<InvDatabase>();
            #endregion

            RecentTargetItemHook? hook = agent.GetHook<RecentTargetItemHook>();
            if (hook?.Item?.Categories.Contains("Internal:HackInteract") is true)
            {
                if ((__instance.functional || __instance is SecurityCam or Turret)
                    && !__instance.tempNoOperating && __instance is ObjectReal real)
                    real.HackObject(agent);
            }
            else if (__instance.buttons.Count > 0)
                __instance.ShowObjectButtons();
            return false;
        }

        internal static bool useModelStopInteraction;
        public static bool PlayfieldObject_StopInteraction(PlayfieldObject __instance)
        {
            if (useModelStopInteraction)
            {
                GetOrCreateModel(__instance).shouldStop = true;
                return false;
            }
            return true;
        }

        private static readonly FieldInfo interactionHelperAgentField = AccessTools.Field(typeof(InteractionHelper), "agent");
        private static Agent? updatingInteractionHelper;
        public static void InteractionHelper_EnterDetails_Prefix(InteractionHelper __instance)
            => updatingInteractionHelper = (Agent)interactionHelperAgentField.GetValue(__instance);
        public static void InteractionHelper_EnterDetails_Finalizer() => updatingInteractionHelper = null;
        public static void InteractionHelper_UpdateInteractionHelper_Prefix(InteractionHelper __instance)
            => updatingInteractionHelper = (Agent)interactionHelperAgentField.GetValue(__instance);
        public static void InteractionHelper_UpdateInteractionHelper_Finalizer() => updatingInteractionHelper = null;

        public static bool PlayfieldObject_get_interactable(PlayfieldObject __instance, ref bool __result)
        {
            if (updatingInteractionHelper is null) return true;
            InteractionModel model = GetOrCreateModel(__instance);
            __result = model.IsInteractable(updatingInteractionHelper);
            return false;
        }

        public static bool InteractionHelper_canInteractCounter(out bool __result)
        {
            __result = true;
            return false;
        }

        public static bool Movement_HasLOSObject360(Movement __instance, PlayfieldObject myPlayfieldObject, Transform ___tr, ref bool __result)
        {
            if (updatingInteractionHelper && myPlayfieldObject.gameObject.layer is 8 or 9) // Walls or WallsHard
            {
                int prevLayer = myPlayfieldObject.gameObject.layer;
                GameObject? childWall = null;
                int prevWallLayer = 0;
                if (myPlayfieldObject is Boulder or Tree)
                {
                    string childWallName = myPlayfieldObject is Boulder ? "BoulderWall" : "TreeWall";
                    childWall = myPlayfieldObject.gameObject.transform.Find(childWallName)?.gameObject;
                    if (childWall is not null) prevWallLayer = childWall.layer;
                }
                try
                {
                    myPlayfieldObject.gameObject.layer = 21; // ObjectReals
                    if (childWall is not null) childWall.layer = 21;
                    __result = !Physics2D.Linecast(___tr.position, myPlayfieldObject.tr.position, __instance.myLayerMaskEfficient);
                }
                finally
                {
                    myPlayfieldObject.gameObject.layer = prevLayer;
                    if (childWall is not null) childWall.layer = prevWallLayer;
                }
                return false;
            }
            return true;
        }
        public static void LockdownWall_SetWallDown(LockdownWall __instance)
            => __instance.objectSprite.objectHitbox.enabled = true;
        public static void LockdownWall_SetWallDownAnim(LockdownWall __instance)
            => __instance.objectSprite.objectHitbox.enabled = true;
    }
}
