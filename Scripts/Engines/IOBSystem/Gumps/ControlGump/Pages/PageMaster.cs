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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PageMaster.cs
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
using Server.Engines.CommitGump;

namespace Server.Engines.IOBSystem.Gumps.ControlGump
{

	public partial class KinCityControlGump : CommitGumpBase
	{

		/// <summary>
		/// 
		/// </summary>
		private sealed class PageMaster : ICommitGumpEntity
		{
			//use high numbers for the master to ensure it doesn't clash with other pages
			private enum Buttons
			{
				//Main Menu
				btnMenuMainOK = 100000,
				btnMenuMainCancel,
				btnMenuOverview,
				btnMenuFinance,
				btnMenuPopulace,
				btnMenuDelegation,
				btnMenuVote
			}

			private KinCityControlGump m_Gump = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="PageMaster"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageMaster(KinCityControlGump gump)
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
				get { return "PageMaster"; }
			}

			/// <summary>
			/// Commit any outstanding changes
			/// </summary>
			/// <param name="sender"></param>
			void ICommitGumpEntity.CommitChanges()
			{
				//no state for the overview
			}

			/// <summary>
			/// Creation of the entity's graphics
			/// </summary>
			void ICommitGumpEntity.Create()
			{
				#region master page

				m_Gump.AddPage(0);
				m_Gump.AddImage(169, 2, 10861);
				m_Gump.AddImage(6, 33, 2623);
				m_Gump.AddImage(6, 242, 2623);
				m_Gump.AddImage(6, 15, 2620);
				m_Gump.AddImage(25, 15, 2621);
				m_Gump.AddImage(295, 14, 2621);
				m_Gump.AddImage(140, 33, 2623);
				m_Gump.AddImage(139, 242, 2623);
				m_Gump.AddImage(140, 15, 2620);
				m_Gump.AddImage(6, 452, 2626);
				m_Gump.AddImage(139, 451, 2626);
				m_Gump.AddImage(26, 462, 2621);
				m_Gump.AddImage(296, 461, 2621);
				m_Gump.AddImage(600, 450, 2628);
				m_Gump.AddImage(600, 14, 2622);
				m_Gump.AddImage(616, 32, 2623);
				m_Gump.AddImage(615, 241, 2623);
				m_Gump.AddImage(332, 14, 2621);
				m_Gump.AddImage(341, 462, 2621);
				m_Gump.AddBackground(144, 22, 470, 441, 9270);
				m_Gump.AddBackground(12, 22, 128, 441, 9270);
				m_Gump.AddAlphaRegion(622, 16, 22, 463);
				m_Gump.AddAlphaRegion(5, 471, 635, 8);

				m_Gump.AddButton(44, 412, 247, 248, (int)Buttons.btnMenuMainOK, GumpButtonType.Reply, 0);

				//Right, special case here. If the city is still in the vote stage, we don't want to show the normal buttons.
				//Instead, the only button will be called Vote (which is normally Overview)

				if (m_Gump.Page == 5)
				{
						m_Gump.AddButton(47, 40, 5558, 5558, (int)Buttons.btnMenuVote, GumpButtonType.Reply, 1);
						m_Gump.AddLabel(47, 101, 1359, @"Voting");
				}
				else
				{
					if (m_Gump.Page == 1)
						m_Gump.AddButton(47, 40, 5558, 5558, (int)Buttons.btnMenuOverview, GumpButtonType.Reply, 1);
					else
						m_Gump.AddButton(47, 40, 5557, 5558, (int)Buttons.btnMenuOverview, GumpButtonType.Reply, 1);
					m_Gump.AddLabel(53, 101, 1359, @"Overview");

					if (m_Gump.Page == 2)
						m_Gump.AddButton(47, 132, 5572, 5572, (int)Buttons.btnMenuFinance, GumpButtonType.Reply, 2);
					else
						m_Gump.AddButton(47, 132, 5571, 5572, (int)Buttons.btnMenuFinance, GumpButtonType.Reply, 2);
					m_Gump.AddLabel(55, 191, 1359, @"Finance");

					if (m_Gump.Page == 3)
						m_Gump.AddButton(47, 222, 5576, 5576, (int)Buttons.btnMenuPopulace, GumpButtonType.Reply, 3);
					else
						m_Gump.AddButton(47, 222, 5575, 5576, (int)Buttons.btnMenuPopulace, GumpButtonType.Reply, 3);
					m_Gump.AddLabel(52, 284, 1359, @"Populace");

					if (m_Gump.Page == 4)
						m_Gump.AddButton(47, 312, 5592, 5592, (int)Buttons.btnMenuDelegation, GumpButtonType.Reply, 4);
					else
						m_Gump.AddButton(47, 312, 5591, 5592, (int)Buttons.btnMenuDelegation, GumpButtonType.Reply, 4);
					m_Gump.AddLabel(50, 377, 1359, @"Delegation");
				}
				#endregion

			}

			/// <summary>
			/// Restore data from the session
			/// </summary>
			void ICommitGumpEntity.LoadStateInfo()
			{

			}

			/// <summary>
			/// Handle user response
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="info"></param>
			/// <returns></returns>
			CommitGumpBase.GumpReturnType ICommitGumpEntity.OnResponse(Server.Network.NetState sender, RelayInfo info)
			{

				switch ((Buttons)info.ButtonID)
				{
					case Buttons.btnMenuMainOK:
						{
							return CommitGumpBase.GumpReturnType.OK;
						}
					case Buttons.btnMenuMainCancel:
						{
							return CommitGumpBase.GumpReturnType.Cancel;
						}
					case Buttons.btnMenuOverview: m_Gump.Page = 1; break;
					case Buttons.btnMenuFinance: m_Gump.Page = 2; break;
					case Buttons.btnMenuPopulace: m_Gump.Page = 3; break;
					case Buttons.btnMenuDelegation: m_Gump.Page = 4; break;
					case Buttons.btnMenuVote: m_Gump.Page = 5; break;
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
		}
	}
}
