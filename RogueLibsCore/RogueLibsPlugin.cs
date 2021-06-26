﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace RogueLibsCore
{
	[BepInPlugin(RogueLibs.GUID, RogueLibs.Name, RogueLibs.CompiledVersion)]
	[BepInIncompatibility("abbysssal.streetsofrogue.ectd")]
	public sealed partial class RogueLibsPlugin : BaseUnityPlugin
	{
		public RoguePatcher Patcher;
		public void Awake()
		{
			RogueFramework.Plugin = this;
			RogueFramework.Logger = Logger;

			Patcher = new RoguePatcher(this);

			PatchItems();
			PatchTraitsAndStatusEffects();
			PatchMisc();
			PatchUnlocks();
			PatchScrollingMenu();
			PatchCharacterCreation();
			PatchSprites();
		}
	}
}