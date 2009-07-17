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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PageDelegation.cs
 * CHANGELOG:
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
using Server.Engines.IOBSystem.Gumps.ControlGump;
using Server.Engines.CommitGump;
namespace Server.Engines.IOBSystem.Gumps.ControlGump
{
	/// <summary>
	/// 
	/// </summary>
	public partial class KinCityControlGump : CommitGumpBase
	{
		private sealed class PageDelegation : ICommitGumpEntity
		{
			private sealed class GumpState
			{
				public Dictionary<PlayerMobile, int> SlotsAssigned = new Dictionary<PlayerMobile, int>(); //to prevent exploits
				public Dictionary<PlayerMobile, int> SlotsRemaining = new Dictionary<PlayerMobile,int>(); //to prevent exploits
				public Dictionary<PlayerMobile, int> SlotsRemainingChanged = new Dictionary<PlayerMobile,int>();
				public string Status = string.Empty;
				public int OriginalRemainingSlots =0;
				public int RemainingSlots = 0;

				/// <summary>
				/// Initializes a new instance of the <see cref="GumpState"/> class.
				/// </summary>
				/// <param name="data">The data.</param>
				public GumpState(KinCityData data)
				{
					SlotsRemaining = new Dictionary<PlayerMobile, int>();
					foreach (KinCityData.BeneficiaryData bd in data.BeneficiaryDataList)
					{
						SlotsRemaining.Add(bd.Pm, bd.UnassignedGuardSlots);
						SlotsRemainingChanged.Add(bd.Pm, bd.UnassignedGuardSlots);
						SlotsAssigned.Add(bd.Pm, CountAssignedSlots(bd)); 
					}
					RemainingSlots = data.UnassignedGuardPostSlots;
				}

				/// <summary>
				/// Gets a value indicating whether this instance is dirty.
				/// </summary>
				/// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
				public bool IsDirty
				{
					get
					{
						//Check changed slots against the initial changed slots
						foreach (KeyValuePair<PlayerMobile, int> kvp in SlotsRemaining)
							if (SlotsRemainingChanged[kvp.Key] != kvp.Value) return true;
						return false;
					}
				}

				/// <summary>
				/// Returns the total amount of assigned slots the benficary has
				/// </summary>
				/// <param name="data"></param>
				/// <returns></returns>
				public int CountAssignedSlots(KinCityData.BeneficiaryData data)
				{
					int slots = 0;
					//Add the present slot cost of each guard posts
					if (data.GuardPosts != null && data.GuardPosts.Count > 0)
						foreach (KinGuardPost kgp in data.GuardPosts)
							slots += (int)kgp.CostType;
					return slots;
				}

			}

			private GumpState m_State = null;
			private KinCityControlGump m_Gump = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="PageDelegation"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageDelegation(KinCityControlGump gump)
			{
				m_Gump = gump;
				((ICommitGumpEntity)this).LoadStateInfo();
			}

			#region IGumpEntity Members

			string ICommitGumpEntity.ID
			{
				get { return "PageDelegation"; }
			}

			/// <summary>
			/// Validate outstanding changes
			/// </summary>
			/// <returns></returns>
			bool ICommitGumpEntity.Validate()
			{
				if (!ReferenceEquals(m_Gump.From, m_Gump.Data.CityLeader) && m_Gump.From.AccessLevel <= AccessLevel.Player ) return false;
				if (!m_State.IsDirty) return true;
				//Check against the intial snapshot to make sure nothing has changed since
				foreach (KinCityData.BeneficiaryData bd in m_Gump.Data.BeneficiaryDataList)
				{
					if (!m_State.SlotsRemaining.ContainsKey(bd.Pm))
						return false;

					if (m_State.SlotsRemaining[bd.Pm] != bd.UnassignedGuardSlots)
						return false;
				}
				return true;
			}

			/// <summary>
			/// Commit any outstanding changes
			/// </summary>
			/// <param name="sender"></param>
			void ICommitGumpEntity.CommitChanges()
			{
				if (!m_State.IsDirty || m_State.SlotsRemainingChanged.Count == 0)
					return;

				//Exploit check : make sure the unassigned count for the city hasn't changed sinced the gump has been in progress
				if (((ICommitGumpEntity)this).Validate())
				{
					foreach (KeyValuePair<PlayerMobile, int> kvp in m_State.SlotsRemainingChanged)
					{
						if (m_Gump.Data.GetBeneficiary(kvp.Key).UnassignedGuardSlots != m_State.SlotsRemainingChanged[kvp.Key])
						{
							m_Gump.Data.ModifyGuardSlots(kvp.Key, kvp.Value - m_State.SlotsRemaining[kvp.Key]);
						}
					}
					m_Gump.From.SendMessage("Guard slots successfully adjusted");
				}
				else
				{
					m_Gump.From.SendMessage("Guard slots failed to adjust correctly and did not commit");
				}
			}

			/// <summary>
			/// Creation of the entity's graphics
			/// </summary>
			void ICommitGumpEntity.Create()
			{
				//Note: The first time the state is created it also takes a snapshot of the current slot distribution
				//This is to prevent exploits when commiting
				if (m_State == null) m_State = new GumpState(m_Gump.Data);
				#region page 4

				//	Page 4 - delegation	 //////////////////////////////////////////////////////////////////////

				StringBuilder htmlSb = new StringBuilder("<basefont color=#FFCC00><center>Guard Post Delegation</center></basefont>");
				m_Gump.AddHtml(170, 40, 409, 19, htmlSb.ToString(), (bool)false, (bool)false); //left
				htmlSb = new StringBuilder("<basefont color=gray><center>Here you may delegate the creation of guard posts to beneficiaries</center></basefont>");
				m_Gump.AddHtml(170, 70, 409, 19, htmlSb.ToString(), (bool)false, (bool)false); //left
				htmlSb = new StringBuilder(string.Format("<basefont color=white><center>Guard Slots Remaining : {0}/{1}</center></basefont>", m_State.RemainingSlots, KinSystem.GetCityGuardPostSlots(m_Gump.City)));
				m_Gump.AddHtml(170, 100, 409, 19, htmlSb.ToString(), (bool)false, (bool)false); //left

				//Add a button and label for each beneficiary
				int buttonID = 1;
				int yLocation = 137;
				int ySpacing = 25;

				foreach (KinCityData.BeneficiaryData data in m_Gump.Data.BeneficiaryDataList)
				{
					int pageDivider = (buttonID / 22);
					int xLocation = 174 + (pageDivider * 230);
					m_Gump.AddButton(xLocation, yLocation, 2223, 2223, buttonID, GumpButtonType.Reply, 0);
					m_Gump.AddButton(xLocation + 20, yLocation, 2224, 2224, buttonID + 1, GumpButtonType.Reply, 0);
					string caption = string.Format("{0}/{1} slots : {2}", m_State.SlotsRemainingChanged[data.Pm] + m_State.SlotsAssigned[data.Pm], m_State.SlotsRemainingChanged[data.Pm],(data.Pm.Name.Length > 12 ? data.Pm.Name.Substring(0, 12) : data.Pm.Name));
					m_Gump.AddLabel(xLocation + 45, yLocation - 5, GetStatusColour(data.Pm), caption);
					yLocation += ySpacing;
					if (buttonID == 21)
						yLocation = 137;
					buttonID += 2;
				}

				if (!((ICommitGumpEntity)this).Validate())
					m_Gump.AddHtml(169, 415, 412, 136, "<basefont color=red><center>Your City's guard data has been modified from elsewhere.  You must close and start again.</center></basefont>", (bool)false, (bool)false);
				else if (!ReferenceEquals(m_Gump.Data.CityLeader, m_Gump.From))
					m_Gump.AddHtml(169, 425, 412, 136, string.Format("<basefont color=gray><center>{0}</center></basefont>", "Only the city leader may make changes"), false, false);
				else if (m_State.IsDirty)
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You must press OK before these changes will take effect</center></basefont>", false, false);

				//////////////////////////////////////////////////////////////////////////////////////////////

				m_State.Status = string.Empty;

				#endregion

			}


			/// <summary>
			/// Restore data from the session
			/// </summary>
			void ICommitGumpEntity.LoadStateInfo()
			{
				m_State = m_Gump.Session[((ICommitGumpEntity)this).ID] as GumpState;
			}

			/// <summary>
			/// Handle user response
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="info"></param>
			/// <returns></returns>
			CommitGumpBase.GumpReturnType ICommitGumpEntity.OnResponse(Server.Network.NetState sender, RelayInfo info)
			{

				if (info.ButtonID >= 10000) return CommitGumpBase.GumpReturnType.None;  //these are master page buttons			

				//Only the leader can change stuff here
				if (!ReferenceEquals(m_Gump.Data.CityLeader, sender.Mobile) && sender.Mobile.AccessLevel <= AccessLevel.Player)
				{
					m_State.Status = "Only the city leader may make changes";
					return GumpReturnType.None;
				}
				
				//Work out which beneficary was selected, and if it was the + or - button.
				int playerIndex = (int)Math.Ceiling(((double)info.ButtonID / 2)) - 1;
				int buttonIndex = (info.ButtonID % 2); if (buttonIndex > 0) buttonIndex = 1;
				//sender.Mobile.SendMessage(string.Format("Player {0} selected. Button {1}",playerIndex,buttonIndex));
				PlayerMobile pm = m_Gump.Data.BeneficiaryDataList[playerIndex].Pm;

				if (buttonIndex == 1)
				{
					
					//Minus was pressed
					if (m_State.SlotsRemainingChanged[pm] == 0)
					{
						m_State.Status = string.Format("{0} does not have any unassiged slots to pool", pm.Name);
						return CommitGumpBase.GumpReturnType.None;
					}
					m_State.SlotsRemainingChanged[pm]--;
				
					m_State.RemainingSlots++;
				}
				else
				{
					//Plus was pressed
					if (m_State.RemainingSlots  == 0)
					{
						m_State.Status = "There are no guard slots left in the pool to assign";
						return CommitGumpBase.GumpReturnType.None;
					}
					m_State.SlotsRemainingChanged[pm]++;
					m_State.RemainingSlots--;
				}
				return CommitGumpBase.GumpReturnType.None;
			}

			/// <summary>
			/// Update the state / session with any changes in memory
			/// </summary>
			void ICommitGumpEntity.SaveStateInfo()
			{
				m_Gump.Session[((ICommitGumpEntity)this).ID] = m_State;
			}

			#endregion

			/// <summary>
			/// Gets the status colour.
			/// </summary>
			/// <param name="pm">The pm.</param>
			/// <returns></returns>
			private int GetStatusColour(PlayerMobile pm)
			{
				if (pm == null) return 1359;

				if (m_State.SlotsRemainingChanged[pm] > m_State.SlotsRemaining[pm])
				{
					//green
					return 271;
				}
				else if (m_State.SlotsRemainingChanged[pm] == m_State.SlotsRemaining[pm])
				{
					return 1359;
				}
				else
				{
					//red
					return 136;
				}

			}

		}
	}
}
