/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property 
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 27, 2007
 */

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PagePopulace.cs
 * CHANGELOG:
 *	04/27/09, plasma
 *		Changed message if changing guards to LB
 *	02/10/08 - Plasma,
 *		Initial creation
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Engines.IOBSystem;
using Server.Engines.CommitGump;
namespace Server.Engines.IOBSystem.Gumps.ControlGump
{

	public partial class KinCityControlGump : CommitGumpBase
	{

		/// <summary>
		/// Epic
		/// </summary>
		private sealed class PagePopulace : ICommitGumpEntity
		{

			private sealed class GumpState
			{
				public long NPCData;
				public long OriginalNPCData;
				public KinCityData.GuardOptions GuardOption;
				public KinCityData.GuardOptions OriginalGuardOption;

				public bool GuardsChanged
				{
					get { return GuardOption != OriginalGuardOption; }
				}

				public bool NPCsChanged
				{
					get { return NPCData != OriginalNPCData; }
				}

				public GumpState(long npcData, KinCityData.GuardOptions guardOption)
				{
					NPCData = OriginalNPCData = npcData;
					GuardOption = OriginalGuardOption = guardOption;
				}
			}

			private enum Buttons
			{
				//Guards
				btnGuardsNone = 1,
				btnGuardsLordBritish,
				btnGuardsKinsReds,
				btnGuardsKinNonInnocents,
				btnGuardsEveryone,
				btnGuardsNonInnocents,
				btnGuardsKin,
				btnGuardsCriminals,
				//Professions
				btnProfessionBanker,
				btnProfessionMages,
				btnProfessionWeaponSmiths,
				btnProfessionBarStaff,
				btnProfessionAnimalTenders,
				btnProfessionPatrolGuards,
				btnProfessionTravellers,
				btnProfessionGypsies,
				btnProfessionCarpenters,
				btnProfessionHealers,
				btnProfessionInnStaff,
				btnProfessionBlackSmiths,
				btnProfessionTailors,
				btnProfessionTownCriers,
				btnProfessionGeneralShops,
				btnProfessionFightBrokers,
				btnProfessionProvisioners
			}

			private KinCityControlGump m_Gump = null;
			private GumpState m_State = null;

			public PagePopulace(KinCityControlGump gump)
			{
				m_Gump = gump;
				((ICommitGumpEntity)this).LoadStateInfo();
			}


			#region IGumpEntity Members

			bool ICommitGumpEntity.Validate()
			{
				if (!ReferenceEquals(m_Gump.From, m_Gump.Data.CityLeader) && m_Gump.From.AccessLevel <= AccessLevel.Player) return false;
				//check guards can be changed
				if (m_State.GuardsChanged)
				{
					if (m_Gump.From.AccessLevel > AccessLevel.Counselor) return true;
					if (!m_Gump.Data.CanChangeGuards)
					{
						return false;
					}
				}
				return true;
			}

			string ICommitGumpEntity.ID
			{
				get { return "PagePopulace"; }
			}

			void ICommitGumpEntity.CommitChanges()
			{
				//Page 3
				//NPC toggle 
				if (m_State.NPCsChanged)
				{
					//test each flag to see what's different and flip the neccesary flags
					for (long l = 1; l <= 0x80000000; l <<= 1)
					{
						long flagA = (l & m_State.NPCData);
						long flagB = (l & m_Gump.Data.NPCCurrentFlags);
						if (flagA != flagB)
							m_Gump.Data.ToggleNPCFlag((KinCityData.NPCFlags)l);
					}
					//Update the city NPC spawners 
					KinCityManager.UpdateCityNPCSpawners(m_Gump.City);
					m_Gump.From.SendMessage("Allowable professions successfully updated");
				}

				if (m_State.GuardsChanged)
				{
					if (!((ICommitGumpEntity)this).Validate())
					{
						m_Gump.From.SendMessage("You are not allowed to change the guards yet");
					}
					else
					{
						//Commit  (gm+ overrides the guard chage timeout)
						KinCityManager.ChangeGuards(m_Gump.City, m_State.GuardOption, m_Gump.From.AccessLevel >= AccessLevel.Counselor);
						if (m_State.GuardOption == KinCityData.GuardOptions.LordBritish)
						{
							m_Gump.From.SendMessage("Lord British's guards will return within the next few minutes.");
						}
						else
						{
							m_Gump.From.SendMessage("Guards changed successfully");
						}
						
					}
				}
			}

			void ICommitGumpEntity.Create()
			{
				#region page 3

				//	Page 3 - Populace	 //////////////////////////////////////////////////////////////////////

				string html = "<basefont color=gray><center>Here you may restrict the legal professions of the civillians living in your city, and select the type of guards that will defend it</basefont></center>";
				m_Gump.AddHtml(170, 40, 412, 19, "<basefont color=#FFCC00><center>City Populace</center></basefont>", false, false);
				m_Gump.AddHtml(170, 118, 205, 314, "<basefont color=white><center>Civillians</center></basefont>", false, false);
				m_Gump.AddHtml(386, 118, 199, 314, "<basefont color=white><center>Guards</center></basefont>", false, false);
				m_Gump.AddHtml(169, 72, 412, 136, html, false, false);

				m_Gump.AddButton(174, 157, GetNPCButtonState(KinCityData.NPCFlags.Bank, true), GetNPCButtonState(KinCityData.NPCFlags.Bank, false), (int)Buttons.btnProfessionBanker, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 182, GetNPCButtonState(KinCityData.NPCFlags.Mages, true), GetNPCButtonState(KinCityData.NPCFlags.Mages, false), (int)Buttons.btnProfessionMages, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 207, GetNPCButtonState(KinCityData.NPCFlags.WeaponArmour, true), GetNPCButtonState(KinCityData.NPCFlags.WeaponArmour, false), (int)Buttons.btnProfessionWeaponSmiths, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 232, GetNPCButtonState(KinCityData.NPCFlags.EatDrink, true), GetNPCButtonState(KinCityData.NPCFlags.EatDrink, false), (int)Buttons.btnProfessionBarStaff, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 257, GetNPCButtonState(KinCityData.NPCFlags.Animal, true), GetNPCButtonState(KinCityData.NPCFlags.Animal, false), (int)Buttons.btnProfessionAnimalTenders, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 282, GetNPCButtonState(KinCityData.NPCFlags.Patrol, true), GetNPCButtonState(KinCityData.NPCFlags.Patrol, false), (int)Buttons.btnProfessionPatrolGuards, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 307, GetNPCButtonState(KinCityData.NPCFlags.Quest, true), GetNPCButtonState(KinCityData.NPCFlags.Quest, false), (int)Buttons.btnProfessionTravellers, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 332, GetNPCButtonState(KinCityData.NPCFlags.Gypsy, true), GetNPCButtonState(KinCityData.NPCFlags.Gypsy, false), (int)Buttons.btnProfessionGypsies, GumpButtonType.Reply, 0);
				m_Gump.AddButton(174, 357, GetNPCButtonState(KinCityData.NPCFlags.Carpenter, true), GetNPCButtonState(KinCityData.NPCFlags.Carpenter, false), (int)Buttons.btnProfessionCarpenters, GumpButtonType.Reply, 0);

				m_Gump.AddButton(305, 157, GetNPCButtonState(KinCityData.NPCFlags.Healer, true), GetNPCButtonState(KinCityData.NPCFlags.Healer, false), (int)Buttons.btnProfessionHealers, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 182, GetNPCButtonState(KinCityData.NPCFlags.Inn, true), GetNPCButtonState(KinCityData.NPCFlags.Inn, false), (int)Buttons.btnProfessionInnStaff, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 207, GetNPCButtonState(KinCityData.NPCFlags.Smith, true), GetNPCButtonState(KinCityData.NPCFlags.Smith, false), (int)Buttons.btnProfessionBlackSmiths, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 232, GetNPCButtonState(KinCityData.NPCFlags.Tailor, true), GetNPCButtonState(KinCityData.NPCFlags.Tailor, false), (int)Buttons.btnProfessionTailors, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 257, GetNPCButtonState(KinCityData.NPCFlags.TownCrier, true), GetNPCButtonState(KinCityData.NPCFlags.TownCrier, false), (int)Buttons.btnProfessionTownCriers, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 282, GetNPCButtonState(KinCityData.NPCFlags.Misc, true), GetNPCButtonState(KinCityData.NPCFlags.Misc, false), (int)Buttons.btnProfessionGeneralShops, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 307, GetNPCButtonState(KinCityData.NPCFlags.FightBroker, true), GetNPCButtonState(KinCityData.NPCFlags.FightBroker, false), (int)Buttons.btnProfessionFightBrokers, GumpButtonType.Reply, 0);
				m_Gump.AddButton(305, 332, GetNPCButtonState(KinCityData.NPCFlags.Provisioner, true), GetNPCButtonState(KinCityData.NPCFlags.Provisioner, false), (int)Buttons.btnProfessionProvisioners, GumpButtonType.Reply, 0);


				m_Gump.AddLabel(208, 157, 1359, @"Bankers");
				m_Gump.AddLabel(208, 182, 1359, @"Mages");
				m_Gump.AddLabel(208, 207, 1359, @"Weaponsmiths");
				m_Gump.AddLabel(208, 232, 1359, @"Bar Staff");
				m_Gump.AddLabel(208, 257, 1359, @"Animal Tenders");
				m_Gump.AddLabel(208, 282, 1359, @"Patrol Guards");
				m_Gump.AddLabel(208, 307, 1359, @"Travellers");
				m_Gump.AddLabel(208, 332, 1359, @"Gypsies");
				m_Gump.AddLabel(208, 357, 1359, @"Carpenters");

				m_Gump.AddLabel(343, 157, 1359, @"Healers");
				m_Gump.AddLabel(343, 182, 1359, @"Inn Staff");
				m_Gump.AddLabel(343, 207, 1359, @"Blacksmiths");
				m_Gump.AddLabel(343, 232, 1359, @"Tailors");
				m_Gump.AddLabel(343, 257, 1359, @"Town Criers");
				m_Gump.AddLabel(341, 282, 1359, @"General Shops");
				m_Gump.AddLabel(342, 307, 1359, @"Fight Brokers");
				m_Gump.AddLabel(343, 332, 1359, @"Provisioners");

				if (!ReferenceEquals(m_Gump.Data.CityLeader, m_Gump.From))
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>Only the city leader may make changes</center></basefont>", false, false);
				else if (m_State.GuardsChanged && !m_Gump.Data.CanChangeGuards)
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You cannot change the guards yet</center></basefont>", (bool)false, (bool)false);
				else if (m_State.NPCsChanged || m_State.GuardsChanged )
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You must press OK before these changes will take effect</center></basefont>", (bool)false, (bool)false);

				m_Gump.AddButton(440, 157, GetGuardButtonState(KinCityData.GuardOptions.None, true), GetGuardButtonState(KinCityData.GuardOptions.None, false), (int)Buttons.btnGuardsNone, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 182, GetGuardButtonState(KinCityData.GuardOptions.LordBritish, true), GetGuardButtonState(KinCityData.GuardOptions.LordBritish, false), (int)Buttons.btnGuardsLordBritish, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 207, GetGuardButtonState(KinCityData.GuardOptions.FactionAndReds, true), GetGuardButtonState(KinCityData.GuardOptions.FactionAndReds, false), (int)Buttons.btnGuardsKinsReds, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 232, GetGuardButtonState(KinCityData.GuardOptions.FactionAndRedsAndCrim, true), GetGuardButtonState(KinCityData.GuardOptions.FactionAndRedsAndCrim, false), (int)Buttons.btnGuardsKinNonInnocents, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 257, GetGuardButtonState(KinCityData.GuardOptions.Everyone, true), GetGuardButtonState(KinCityData.GuardOptions.Everyone, false), (int)Buttons.btnGuardsEveryone, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 282, GetGuardButtonState(KinCityData.GuardOptions.RedsAndCrim, true), GetGuardButtonState(KinCityData.GuardOptions.RedsAndCrim, false), (int)Buttons.btnGuardsNonInnocents, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 307, GetGuardButtonState(KinCityData.GuardOptions.FactionOnly, true), GetGuardButtonState(KinCityData.GuardOptions.FactionOnly, false), (int)Buttons.btnGuardsKin, GumpButtonType.Reply, 0);
				m_Gump.AddButton(440, 332, GetGuardButtonState(KinCityData.GuardOptions.Crim, true), GetGuardButtonState(KinCityData.GuardOptions.Crim, false), (int)Buttons.btnGuardsCriminals, GumpButtonType.Reply, 0);

				m_Gump.AddLabel(478, 157, 1359, @"None");
				m_Gump.AddLabel(478, 182, 1359, @"Lord British");
				m_Gump.AddLabel(478, 207, 1359, @"Kin/Murderers");
				m_Gump.AddLabel(478, 232, 1359, @"Kin/Non Innocents");
				m_Gump.AddLabel(478, 257, 1359, @"Everyone");
				m_Gump.AddLabel(476, 282, 1359, @"Non Innocents");
				m_Gump.AddLabel(478, 307, 1359, @"Kin");
				m_Gump.AddLabel(477, 332, 1359, @"Criminals");

				//////////////////////////////////////////////////////////////////////////////////////////////

				#endregion

			}

			void ICommitGumpEntity.LoadStateInfo()
			{
				m_State = m_Gump.Session[((ICommitGumpEntity)this).ID] as GumpState;
				if (m_State == null)
				{
					//First time in, create and store the state in the gump session
					m_State = new GumpState(m_Gump.Data.NPCCurrentFlags, m_Gump.Data.GuardOption);
					m_Gump.Session[((ICommitGumpEntity)this).ID] = m_State;
				}
			}

			CommitGumpBase.GumpReturnType ICommitGumpEntity.OnResponse(Server.Network.NetState sender, RelayInfo info)
			{
				//Only the leader can change stuff here
				if (!ReferenceEquals(m_Gump.Data.CityLeader, sender.Mobile) && sender.Mobile.AccessLevel <= AccessLevel.Player)
					return GumpReturnType.None;

				switch ((Buttons)info.ButtonID)
				{
					case Buttons.btnProfessionBanker: ToggleNPCUndoData(KinCityData.NPCFlags.Bank); break;
					case Buttons.btnProfessionMages: ToggleNPCUndoData(KinCityData.NPCFlags.Mages); break;
					case Buttons.btnProfessionWeaponSmiths: ToggleNPCUndoData(KinCityData.NPCFlags.WeaponArmour); break;
					case Buttons.btnProfessionBarStaff: ToggleNPCUndoData(KinCityData.NPCFlags.EatDrink); break;
					case Buttons.btnProfessionAnimalTenders: ToggleNPCUndoData(KinCityData.NPCFlags.Animal); break;
					case Buttons.btnProfessionPatrolGuards:
						{
							if (m_Gump.Data.GuardOption != KinCityData.GuardOptions.LordBritish)
							{
								sender.Mobile.SendMessage("You cannot control this profession with your current guard type");
							}
							else
							{
								ToggleNPCUndoData(KinCityData.NPCFlags.Patrol);
							}
							break;
						}
					case Buttons.btnProfessionTravellers: ToggleNPCUndoData(KinCityData.NPCFlags.Quest); break;
					case Buttons.btnProfessionGypsies: ToggleNPCUndoData(KinCityData.NPCFlags.Gypsy); break;
					case Buttons.btnProfessionCarpenters: ToggleNPCUndoData(KinCityData.NPCFlags.Carpenter); break;
					case Buttons.btnProfessionHealers: ToggleNPCUndoData(KinCityData.NPCFlags.Healer); break;
					case Buttons.btnProfessionInnStaff: ToggleNPCUndoData(KinCityData.NPCFlags.Inn); break;
					case Buttons.btnProfessionBlackSmiths: ToggleNPCUndoData(KinCityData.NPCFlags.Smith); break;
					case Buttons.btnProfessionTailors: ToggleNPCUndoData(KinCityData.NPCFlags.Tailor); break;
					case Buttons.btnProfessionTownCriers: ToggleNPCUndoData(KinCityData.NPCFlags.TownCrier); break;
					case Buttons.btnProfessionGeneralShops: ToggleNPCUndoData(KinCityData.NPCFlags.Misc); break;
					case Buttons.btnProfessionFightBrokers: ToggleNPCUndoData(KinCityData.NPCFlags.FightBroker); break;
					case Buttons.btnProfessionProvisioners: ToggleNPCUndoData(KinCityData.NPCFlags.Provisioner); break;
					//
					case Buttons.btnGuardsCriminals: ChangeGuardUndoData(KinCityData.GuardOptions.Crim); break;
					case Buttons.btnGuardsEveryone: ChangeGuardUndoData(KinCityData.GuardOptions.Everyone); break;
					case Buttons.btnGuardsKin: ChangeGuardUndoData(KinCityData.GuardOptions.FactionOnly); break;
					case Buttons.btnGuardsKinNonInnocents: ChangeGuardUndoData(KinCityData.GuardOptions.FactionAndRedsAndCrim); break;
					case Buttons.btnGuardsKinsReds: ChangeGuardUndoData(KinCityData.GuardOptions.FactionAndReds); break;
					case Buttons.btnGuardsLordBritish: ChangeGuardUndoData(KinCityData.GuardOptions.LordBritish); break;
					case Buttons.btnGuardsNone: ChangeGuardUndoData(KinCityData.GuardOptions.None); break;
					case Buttons.btnGuardsNonInnocents: ChangeGuardUndoData(KinCityData.GuardOptions.RedsAndCrim); break;
				}
				return CommitGumpBase.GumpReturnType.None;
			}

			void ICommitGumpEntity.SaveStateInfo()
			{
				//Nothing to do here, using state directly.
			}

			#endregion

			private bool GetNPCButtonState(KinCityData.NPCFlags npc)
			{
				return (((m_State.NPCData & (long)npc) != 0));
			}

			private int GetNPCButtonState(KinCityData.NPCFlags npc, bool on)
			{
				if (!((m_State.NPCData & (long)npc) != 0) == on)
					return 1150;
				else
					return 1153;
			}

			private int GetGuardButtonState(KinCityData.GuardOptions guard, bool on)
			{
				if (m_State.GuardOption == guard)
					return 1153;
				else
					return 1150;
			}

			private void ToggleNPCUndoData(KinCityData.NPCFlags flag)
			{
				//XOR flip!
				m_State.NPCData ^= (int)flag;
			}

			private void ChangeGuardUndoData(KinCityData.GuardOptions option)
			{
				m_State.GuardOption = option;
			}

		}
	}
}
