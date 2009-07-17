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

/* Scripts/Engines/IOBSystem/Gumps/Pages/PageMain.cs
 * CHANGELOG:
 *	05/25/09, plasma
 *		Now recalcualtes next spawn time when you cange the hire speed
 *	02/10/08 - Plasma,
 *		Initial creation
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Engines.IOBSystem;
using Server.Engines.CommitGump;
using Server.Engines.IOBSystem.Attributes;
using Server.Engines.CommitGump.Controls;

namespace Server.Engines.IOBSystem.Gumps.GuardPostGump
{

	public partial class KinGuardPostGump
	{
		/// <summary>
		/// 
		/// </summary>
		private sealed class PageMain : ICommitGumpEntity
		{
					 
			private enum Buttons
			{
				//misc
				btnMenuMainOK = 1,
				//type  
				//speed
				//btnSpeedFast,
				//btnSpeedMedium,
				//btnSpeedSlow,
				////target priority
				//btnTargStrongest,
				//btnTargWeakest,
				//btnTargClosest,
				//fight style
				//btnStyleMelee,
				//btnStyleMagic
			}

			private enum FightModeButtons			//Recreate a bit of the fightmode enum for the auto UI - we dont care about the rest
			{
				//Map the correct values back so we can have sex0r auto UI
				Strongest = FightMode.Strongest,
				Weakest = FightMode.Weakest,
				Closest = FightMode.Closest
			}


			//strings			
			private readonly string m_OK = @"<basefont color=gray><center>You must press Okay before these changes will take effect.</center></basefont>";
			private readonly string m_MAINT = @"<basefont color=white><center>Next Maintenance Time : {0} minutes. </center></basefont>";
			private readonly string m_HIRE = @"<basefont color=white><center>Next Hire Time : {0} minutes. </center></basefont>";
			private readonly string m_SLOTS = @"<basefont color={0}><center>You have {1} guard slots left.</center></basefont>";
			private readonly string m_SILVER = "<basefont color={0}><center>Funding : {1} silver.</center></basefont>";
			
			List<KinFactionGuardTypes> m_Types = new List<KinFactionGuardTypes>();
			private KinGuardPostGump m_Gump = null;
			private DirtyState m_State = null;
			private ButtonSet m_GuardTypeButtons = null;
			private ButtonSet m_TargetButtons = null;
			private ButtonSet m_SpeedButtons = null;

			/// <summary>
			/// Gets or sets the guard post.
			/// </summary>
			/// <value>The guard post.</value>
			public KinGuardPost GuardPost
			{
				get { return m_Gump.Session["GuardPost"] as KinGuardPost; }
				set { m_Gump.Session["GuardPost"] = value; }
			}


			/// <summary>
			/// Initializes a new instance of the <see cref="PageMain"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageMain(KinGuardPostGump gump)
			{
				m_Gump = gump;
				((ICommitGumpEntity)this).LoadStateInfo();
			}

			#region IGumpEntity Members

			/// <summary>
			/// Validate outstanding changes
			/// </summary>
			/// <returns></returns>
			bool ICommitGumpEntity.Validate()
			{
				//Assert that the values havent changed 
				if (((int)m_State.GetOriginalValue<KinGuardPost.HireSpeeds>("Speed")) != (int)GuardPost.HireSpeed)	return false;
				if (((int)m_State.GetOriginalValue<int>("Slots")) != GetSlotsAvailable())														return false;
				if (((int)m_State.GetOriginalValue<FightMode>("Target")) != (int)GuardPost.FightMode)								return false;
				if (((int)m_State.GetOriginalValue<KinFactionGuardTypes>("Type")) != (int)GuardPost.GuardType)			return false;
				return true;
			}

			/// <summary>
			/// Unqiue ID use as a key in the sesssion
			/// </summary>
			/// <value></value>
			string ICommitGumpEntity.ID
			{
				get { return "PageMain"; }
			}

			/// <summary>
			/// Commit any outstanding changes
			/// </summary>
			/// <param name="sender"></param>
			void ICommitGumpEntity.CommitChanges()
			{
				if (!((ICommitGumpEntity)this).Validate())
				{
					m_Gump.From.SendMessage("The guard post has been changed since you were modifying it. Please make the changes again.");
					return;
				}
				if (m_State.IsValueDirty<KinFactionGuardTypes>("Type")	)
				{
					//set guard type
					
					//TODO: - check some delay?
					GuardPost.GuardType =(KinFactionGuardTypes) m_State.GetValue<KinFactionGuardTypes>("Type");
					m_Gump.From.SendMessage("Guard type changed to {0}.", KinSystem.GetEnumTypeDescription<KinFactionGuardTypes>((KinFactionGuardTypes)m_State.GetValue<KinFactionGuardTypes>("Type")));
				}
				if (m_State.IsValueDirty<KinGuardPost.HireSpeeds>("Speed"))
				{
					GuardPost.HireSpeed = (KinGuardPost.HireSpeeds)m_State.GetValue<KinGuardPost.HireSpeeds>("Speed");
					m_Gump.From.SendMessage("Hire rate successfully changed to {0}", KinSystem.GetEnumTypeDescription<KinGuardPost.HireSpeeds>((KinGuardPost.HireSpeeds)m_State.GetValue<KinGuardPost.HireSpeeds>("Speed")));
					GuardPost.RefreshNextSpawnTime();
				}
				if (m_State.IsValueDirty<int>("Slots"))
				{
					KinFactionGuardTypes currentType = (KinFactionGuardTypes)m_State.GetOriginalValue<KinFactionGuardTypes>("Type");
					KinFactionGuardTypes type = (KinFactionGuardTypes)m_State.GetValue<KinFactionGuardTypes>("Type");
					int cost = KinSystem.GetGuardCostType(type);
					int currentCost = KinSystem.GetGuardCostType(currentType);
					int slots = 0;
					slots += (currentCost - cost);
					KinCityData data = KinCityManager.GetCityData(GuardPost.City);
					if (data == null) return;
					KinCityData.BeneficiaryData bd = data.GetBeneficiary(GuardPost.Owner);
					if (bd == null) return;
					bd.ModifyGuardSlots(slots);
					m_Gump.From.SendMessage("Unassigned guard slots modified by {0}.", slots);
				}
				/*
				if (m_State.StyleChanged)
				{
					//Assign new style
					GuardPost.FightStyle = m_State.Style;
				}
				*/
				if (m_State.IsValueDirty<FightMode>("Target"))
				{	
					//Dont overwrite this one
					// 0 the strongest, weakest, closest
					//NAND out the options so these bits are all 0
					GuardPost.FightMode &= ~FightMode.Strongest;
					GuardPost.FightMode &= ~FightMode.Weakest;
					GuardPost.FightMode &= ~FightMode.Closest;
					//write new version
					GuardPost.FightMode |= (FightMode)m_State.GetValue<FightMode>("Target");
					m_Gump.From.SendMessage("Guard target priority successfully changed.");
					GuardPost.UpdateExisitngGuards();
				}
			}

			void ICommitGumpEntity.Create()
			{
				if (GuardPost.Owner == null) return;

				m_Gump.AddPage(0);
				m_Gump.AddImage(486, 28, 2623);
				m_Gump.AddImage(36, 1, 10861);
				m_Gump.AddImage(6, 33, 2623);
				m_Gump.AddImage(6, 242, 2623);
				m_Gump.AddImage(6, 15, 2620);
				m_Gump.AddImage(219, 14, 2621);
				m_Gump.AddImage(140, 33, 2623);
				m_Gump.AddImage(139, 242, 2623);
				m_Gump.AddImage(6, 452, 2626);
				m_Gump.AddImage(213, 462, 2621);
				m_Gump.AddImage(469, 451, 2628);
				m_Gump.AddImage(470, 14, 2622);
				m_Gump.AddImage(486, 41, 2623);
				m_Gump.AddImage(485, 243, 2623);
				m_Gump.AddImage(26, 14, 2621);
				m_Gump.AddImage(23, 462, 2621);
				m_Gump.AddAlphaRegion(493, 14, 22, 463);
				m_Gump.AddBackground(16, 25, 470, 440, 9270);
				m_Gump.AddAlphaRegion(8, 473, 499, 8);
				
				m_Gump.AddButton(401, 417, 247, 248, (int)Buttons.btnMenuMainOK, GumpButtonType.Reply, 0);
				m_Gump.AddHtml(45, 46, 412, 19, @"<basefont color=#FFCC00><center>Guard Post Customisation</basefont></center>", (bool)false, (bool)false);
				m_Gump.AddHtml(60, 76, 205, 314, "<basefont color=white><center>Guard Type</center></basefont>", (bool)false, (bool)false);
				m_Gump.AddHtml(286, 76, 199, 314, "<basefont color=white><center>Hire Rate</center></basefont>", (bool)false, (bool)false);
			
				//For the guards we could just use the enum directly but in this case i want to sort the guards by their cost type first.
				//And also possibly exclude guard types from this particular Kin.
				m_Types = KinSystem.GetEligibleGuardTypes(GuardPost.Owner.IOBRealAlignment);
				
				m_Types.Sort(delegate(KinFactionGuardTypes x, KinFactionGuardTypes y) //Sort by cost type so they are grouped
					{
						int costA = (int)KinSystem.GetGuardCostType(x);
						int costB = (int)KinSystem.GetGuardCostType(y);
						if (costA < costB) return 1;
						else if (costA == costB) return 0;
						else return -1;
					}
				);

				List<string> guardTypeStrings = new List<string>();
				foreach( KinFactionGuardTypes t in m_Types )
					guardTypeStrings.Add(KinSystem.GetEnumTypeDescription<KinFactionGuardTypes>(t));

				//Add complete button set of possible guards, in two columns. Hook response methods up.
				m_GuardTypeButtons = new ButtonSet
				(
					m_Gump, guardTypeStrings, 2,
					135, 25, 1153, 1150,
					41, 115, 1000,
					//Anon methods GO!
					delegate(int id) { return m_State.GetValue<KinFactionGuardTypes>("Type").Equals(m_Types[id]); },	//Get Status
					delegate(int id) { SetGuardType(m_Types[id]); },														//Click
					delegate(int id) { return GetGuardTextColour(m_Types[id]); }								//Get label colour
				);

				m_Gump.AddHtml(75, 310, 230, 314, string.Format(m_MAINT, GetNextMaintTimeMinutes()) , (bool)false, (bool)false);
				m_Gump.AddHtml(75, 335, 230, 314, string.Format(m_SLOTS, GetSlotTextColour(), m_State.GetValue<int>("Slots")), (bool)false, (bool)false);
				m_Gump.AddHtml(75, 360, 230, 314, string.Format(m_SILVER, GetSilverTextColour(), GuardPost.Silver), (bool)false, (bool)false);
				
				//Defrag creatures and show next hire time if there is no guard currently spawned.
				GuardPost.Defrag();
				if( GuardPost.Creatures.Count == 0 ) 
					m_Gump.AddHtml(75, 385, 230, 314, string.Format(m_HIRE, GetNextHireTimeMinutes()), (bool)false, (bool)false);


				//Add button set for the FightMode subset we are interested in.
				m_SpeedButtons = new ButtonSet
				(
					m_Gump, typeof(KinGuardPost.HireSpeeds), 1,
					0, 25, 1153, 1150,
					340, 115, 2000,
					//Anon methods GO!
					delegate(int id) { return ((int)m_State.GetValue<KinGuardPost.HireSpeeds>("Speed")).Equals(id); },
					delegate(int id) { m_State.SetValue("Speed", (KinGuardPost.HireSpeeds)id);  },
					delegate(int id) { return 1359; }
				);
	
				m_Gump.AddHtml(286, 200, 199, 314, "<basefont color=white><center>Target Priority</center></basefont>", (bool)false, (bool)false);

				//Add button set for the FightMode subset we are interested in.
				m_TargetButtons = new ButtonSet
				(
					m_Gump, typeof(FightModeButtons) , 1,
					0, 25, 1153, 1150,
					340, 230, 3000,  //note: these are flags so we give them the highest offset
						//Anon methods GO!
					delegate(int id) { return (((int)m_State.GetValue<FightMode>("Target")) & id) != 0; },
					delegate(int id) { SetTarget((FightMode)id); },
					delegate(int id) { return 1359; }
				);

				/*
				m_Gump.AddHtml(286, 310, 199, 314, "<basefont color=white><center>Fight Style</center></basefont>", (bool)false, (bool)false);
					
				spacer = 315;

				m_Gump.AddLabel(380, spacer += 25, 1359, @"Melee");
				m_Gump.AddButton(340, spacer, 1150, 1153, (int)Buttons.btnStyleMelee, GumpButtonType.Reply, 0);
				m_Gump.AddLabel(380, spacer += 25, 1359, @"Magic");
				m_Gump.AddButton(340, spacer, 1150, 1153, (int)Buttons.btnStyleMagic, GumpButtonType.Reply, 0);
				 */

				if(	m_State.IsValueDirty<KinFactionGuardTypes>("Type")
						||	m_State.IsValueDirty<int>("Slots")
						||	m_State.IsValueDirty<KinGuardPost.HireSpeeds>("Speed")
						||	m_State.IsValueDirty<FightMode>("Target") )
					m_Gump.AddHtml(10, 415, 412, 136, m_OK, (bool)false, (bool)false);
			}

			/// <summary>
			/// Restore data from the session
			/// </summary>
			void ICommitGumpEntity.LoadStateInfo()
			{
				m_State = (m_Gump.Session[((ICommitGumpEntity)this).ID] as DirtyState);
				if (m_State == null)
				{
					m_State = new DirtyState();
					m_Gump.Session[((ICommitGumpEntity)this).ID] = m_State;
					m_State.SetValue("Type",GuardPost.GuardType);
					KinCityData data = KinCityManager.GetCityData(GuardPost.City);
					if (data == null) return;
					KinCityData.BeneficiaryData bdata = data.BeneficiaryDataList.Find(delegate(KinCityData.BeneficiaryData b) { return b.Pm == GuardPost.Owner; });
					if (bdata == null && m_Gump.From.AccessLevel <= AccessLevel.Player ) return;
					m_State.SetValue("Slots",GetSlotsAvailable());
					m_State.SetValue("Target", GuardPost.FightMode);
					m_State.SetValue("Speed", GuardPost.HireSpeed);
				}
			}

			/// <summary>
			/// Handle user response
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="info"></param>
			/// <returns></returns>
			CommitGumpBase.GumpReturnType ICommitGumpEntity.OnResponse(Server.Network.NetState sender, RelayInfo info)
			{
				((ICommitGumpEntity)m_GuardTypeButtons).OnResponse(sender, info);
				((ICommitGumpEntity)m_TargetButtons).OnResponse(sender, info);
				((ICommitGumpEntity)m_SpeedButtons).OnResponse(sender, info);

				switch ((Buttons)info.ButtonID)
				{
					case Buttons.btnMenuMainOK:
						{
							return CommitGumpBase.GumpReturnType.OK;
						}		
				}
				return CommitGumpBase.GumpReturnType.None;
			}

			/// <summary>
			/// Update the state / session with any changes in memory
			/// </summary>
			void ICommitGumpEntity.SaveStateInfo()
			{
				//nothing to do here as using state directly throughout
			}

			#endregion

			#region private methods

			/// <summary>
			/// Sets the target.
			/// </summary>
			/// <param name="fightMode">The fight mode.</param>
			private void SetTarget(FightMode fightMode)
			{
				//NAND out the options so these bits are all 0
				FightMode newFightMode = (FightMode)m_State.GetValue<FightMode>("Target");
				newFightMode &= ~FightMode.Strongest;
				newFightMode &= ~FightMode.Weakest;
				newFightMode &= ~FightMode.Closest;
				newFightMode |= fightMode;
				//Set the required option
				m_State.SetValue("Target", newFightMode);
			}

			/// <summary>
			/// Gets the slots available for the Owner.
			/// </summary>
			/// <returns></returns>
			private int GetSlotsAvailable()
			{
				int slots = 0;
				KinCityData.BeneficiaryData benData = GetBenData();
				if (benData == null) return slots;
				return benData.UnassignedGuardSlots;
			}

			/// <summary>
			/// Gets the benificary data of the Owner
			/// </summary>
			/// <returns></returns>
			private KinCityData.BeneficiaryData GetBenData()
			{
				KinCityData cityData = KinCityManager.GetCityData(GuardPost.City);
				if (cityData == null) return null; 
				KinCityData.BeneficiaryData benData = cityData.BeneficiaryDataList.Find(delegate(KinCityData.BeneficiaryData bd) { return bd.Pm == GuardPost.Owner; });
				return benData;
			}

			/// <summary>
			/// Gets the next maint time minutes.
			/// </summary>
			/// <returns></returns>
			private int GetNextMaintTimeMinutes()
			{
				int minutes = (GuardPost.NextMaintTime - DateTime.Now).Minutes;
				if (minutes < 0) minutes = 0;
				return minutes;
			}

			/// <summary>
			/// Gets the next hire time minutes.
			/// </summary>
			/// <returns></returns>
			private int GetNextHireTimeMinutes()
			{
				int minutes = (GuardPost.NextSpawnTime - DateTime.Now).Minutes;
				if (minutes < 0) minutes = 0;
				return minutes;
			}

			/// <summary>
			/// Gets the guard text colour.
			/// </summary>
			/// <param name="guardType">Type of the guard.</param>
			/// <returns></returns>
			private int GetGuardTextColour(KinFactionGuardTypes guardType)
			{
				//less than five maint costs is low health
				int cost = KinSystem.GetGuardCostType(guardType);				

				if( cost == (int)KinFactionGuardCostTypes.HighCost )
					return 136;  //red
			
				//less than 10 maint costs is medium health
				if (cost == (int)KinFactionGuardCostTypes.MediumCost )
					return 1359;  //orange

				if (cost == (int)KinFactionGuardCostTypes.LowCost)
					return 271; //green!

				return 130; //red TODO: Special colour here.
			}

			/// <summary>
			/// Returns red, orange or green depending on the amount of silver in the guardpost compared to the maintenance cost of the current guard type
			/// </summary>
			/// <returns></returns>
			private string GetSilverTextColour()
			{
				//less than five maint costs is low health
				if (GuardPost.Silver < (GuardPost.MaintCost * 5))
					return "red";
					//return 136;  //red

				//less than 10 maint costs is medium health
				if (GuardPost.Silver < (GuardPost.MaintCost * 10))
					return "yellow";
					//return 1359;  //orange

				return "green";
				//return 271; //green!
			}

			/// <summary>
			/// Returns red, orange or green depending on the amount of silver in the guardpost compared to the maintenance cost of the current guard type
			/// </summary>
			/// <returns></returns>
			private string GetSlotTextColour()
			{
				//less than five maint costs is low health
				if (!m_State.IsValueDirty<int>("Slots"))
					return "white";

				int orig = (int)m_State.GetOriginalValue<int>("Slots");
				int current = (int)m_State.GetValue<int>("Slots");

				//less than 10 maint costs is medium health
				if (orig < current)
					return "green";

				return "red";
			}

			/// <summary>
			/// Sets the type of the guard.
			/// </summary>
			/// <param name="type">The type.</param>
			private void SetGuardType(KinFactionGuardTypes type)
			{
				//Check if the new type is the same cost as the old type
				KinFactionGuardTypes currentType = (KinFactionGuardTypes)m_State.GetValue<KinFactionGuardTypes>("Type");
				int cost = KinSystem.GetGuardCostType(type);
				int currentCost = KinSystem.GetGuardCostType(currentType);
				if( !cost.Equals(currentCost))
				{
					//Modify the slots accordingly
					int slots = (int)m_State.GetValue<int>("Slots");
					slots += (currentCost - cost);
					if (slots < 0) return;
					m_State.SetValue("Slots", slots);
				}
				m_State.SetValue("Type", type);
			}

			#endregion
		}

	}
}
