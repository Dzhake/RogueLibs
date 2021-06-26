﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace RogueLibsCore
{
	public class FloorUnlock : DisplayedUnlock
	{
		public FloorUnlock() : this(null, false) { }
		public FloorUnlock(string name, bool unlockedFromStart = false) : base(name, "Floor", unlockedFromStart) { }
		internal FloorUnlock(Unlock unlock) : base(unlock) { }

		public override bool IsEnabled
		{
			get => !Unlock.notActive;
			set => Unlock.notActive = !value;
		}
		public override bool IsAvailable
		{
			get => !Unlock.unavailable;
			set
			{
				Unlock.unavailable = !value;
				bool? cur = gc?.sessionDataBig?.floorUnlocks?.Contains(Unlock);
				if (cur == true && !value) { gc.sessionDataBig.floorUnlocks.Remove(Unlock); Unlock.floorCount--; }
				else if (cur == false && value) { gc.sessionDataBig.floorUnlocks.Add(Unlock); Unlock.floorCount++; }
			}
		}

		public override void UpdateButton()
		{
			if (Menu.Type == UnlocksMenuType.FloorsMenu)
				UpdateButton(false);
		}
		public override void OnPushedButton()
		{
			if (IsUnlocked)
			{
				Menu.Agent.mainGUI.HideScrollingMenu();
				gc.mainGUI.ShowCharacterSelect();
				bool quick = gc.challenges.Contains("QuickGame");
				gc.sessionDataBig.elevatorLevel = Name == "Floor5" ? (quick ? 9 : 13)
					: Name == "Floor4" ? (quick ? 7 : 10)
					: Name == "Floor3" ? (quick ? 5 : 7)
					: Name == "Floor2" ? (quick ? 3 : 4)
					: Name == "Floor1" ? 1 : 1;
				if (gc.multiplayerMode)
				{
					if (gc.serverPlayer)
					{
						SendAnnouncementInChat("WantsToGo", Name);
						gc.playerAgent.objectMult.CallRpcForceShowCharacterSelect();
					}
					else gc.playerAgent.objectMult.CallCmdForceShowCharacterSelect(Name);
				}
			}
			else PlaySound("CantDo");
		}

		public override string GetName() => IsUnlocked || Unlock.nowAvailable ? gc.nameDB.GetName(Name + "Name", Unlock.unlockNameType) : "?????";
		public override string GetDescription()
		{
			if (IsUnlocked || Unlock.nowAvailable)
			{
				string text = "";
				AddCancellationsTo(ref text);
				AddRecommendationsTo(ref text);
				return text.Trim('\n');
			}
			else
			{
				string text = "?????";
				AddPrerequisitesTo(ref text);
				return text.Trim('\n');
			}
		}
	}
}