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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PageVote.cs
 * CHANGELOG:
 *	04/06/09 - Plasma,
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
using Server.Engines.CommitGump.Controls;

namespace Server.Engines.IOBSystem.Gumps.ControlGump
{

	public partial class KinCityControlGump : CommitGumpBase
	{

		private sealed class PageVote: ICommitGumpEntity
		{
			private KinCityControlGump m_Gump = null;
			private ButtonSet m_BeneficiaryButtons = null;
			private DirtyState m_State = null;
			private bool m_CanVote = true;

			/// <summary>
			/// 
			/// </summary>
			private enum Buttons
			{
				/// <summary>
				/// 
				/// </summary>
				btnStopVoteStageEarly = 1
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PageOverview"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageVote(KinCityControlGump gump)
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
				return true;
			}

			/// <summary>
			/// Unqiue ID use as a key in the sesssion
			/// </summary>
			/// <value></value>
			string ICommitGumpEntity.ID
			{
				get { return "PageVote"; }
			}

			/// <summary>
			/// Commit any outstanding changes
			/// </summary>
			/// <param name="sender"></param>
			void ICommitGumpEntity.CommitChanges()
			{
				if (m_State.IsValueDirty<int>("VotedFor"))
				{
					string playerName = GetBeneficiaryNames()[m_State.GetValue<int>("VotedFor")];
					KinCityData.BeneficiaryData bd = m_Gump.Data.GetBeneficiary(playerName);
					if (bd != null)
					{
						m_Gump.Data.CastVote(bd.Pm, m_Gump.From as PlayerMobile);
					}
				}
			}

			/// <summary>
			/// Creation of the entity's graphics
			/// </summary>
			void ICommitGumpEntity.Create()
			{
				#region page 1

				//	Page 1 - overview	 //////////////////////////////////////////////////////////////////////

				StringBuilder htmlSb = new StringBuilder("<basefont color=#FFCC00><center>Candidates for City Leadership</center></basefont>");
				m_Gump.AddHtml(170, 40, 409, 19, htmlSb.ToString(), false, false);
				htmlSb = new StringBuilder("<basefont color=gray><center>Cast your vote by nominating a person below.</center></basefont>");
				m_Gump.AddHtml(170, 80, 409, 19, htmlSb.ToString(), false, false); //left
				
				//If this is a gm+, show a button that allows the vote process to be stopped (for testing shit only)
				if( m_Gump.From.AccessLevel > AccessLevel.Counselor )
				{
					m_Gump.AddButton(47, 132, 5572, 5572, (int)Buttons.btnStopVoteStageEarly, GumpButtonType.Reply, 1);
				}
				
				CreateButtons();

				KinCityData.BeneficiaryData bd = m_Gump.Data.GetBeneficiary(m_Gump.From as PlayerMobile);
				if (bd == null)
				{
					//This mobile is not a beneficiary of the city
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You are not a beneficiary of this city and cannot vote</center></basefont>", false, false);
					m_CanVote = false;
				}
				else if (bd.HasVoted)
				{
					//This mobile has already voted..
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You have already voted for a leader and may not vote again</center></basefont>", false, false);
					m_CanVote = false;
				}
				else if (m_State.IsValueDirty<int>("VotedFor") )
				{
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You must press OK before your vote is cast</center></basefont>", false, false);
				}
				

				//////////////////////////////////////////////////////////////////////////////////////////////

				#endregion
			}

			private void CreateButtons()
			{
				//Show votable beneficiaries and a button..
				m_BeneficiaryButtons = new ButtonSet
					(
						m_Gump, GetBeneficiaryNames(), 2,
						135, 25, 1153, 1150,
						270, 115, 10,
						//Anon methods GO!
						delegate(int id) { return id == m_State.GetValue<int>("VotedFor"); }, //Get Status
						delegate(int id) { if( m_CanVote ) m_State.SetValue("VotedFor", id); },					//Click
						delegate(int id) { if( id.Equals(m_State.GetValue<int>("VotedFor") )) return 271; else return 1359; }//Get label colour
					);

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
					m_State.SetValue<int>("VotedFor", -1);
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
				((ICommitGumpEntity)m_BeneficiaryButtons).OnResponse(sender, info);
				
				switch (info.ButtonID)
				{
					case (int)Buttons.btnStopVoteStageEarly:
						{
							m_Gump.Data.ProcessVotes();
							sender.Mobile.SendMessage("City vote stage stopped successfully");
							return GumpReturnType.Cancel; //stop gump
						}
				}
				return CommitGumpBase.GumpReturnType.None;
			}

			/// <summary>
			/// Update the state / session with any changes in memory
			/// </summary>
			void ICommitGumpEntity.SaveStateInfo()
			{

			}

			#endregion

			/// <summary>
			/// Gets the beneficiary names.
			/// </summary>
			/// <returns></returns>
			private List<string> GetBeneficiaryNames()
			{
				List<string> results = new List<string>();
				foreach (KinCityData.BeneficiaryData bd in m_Gump.Data.BeneficiaryDataList)
					if( bd.Pm != null )
						results.Add(bd.Pm.Name);
				return results;
			}

		}
	}
}
