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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PageFinance.cs
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

	public partial class KinCityControlGump : CommitGumpBase
	{

		private sealed class PageFinance : ICommitGumpEntity
		{

			private enum Buttons
			{
				btnLowerTax = 1,
				btnRasieTax,
				btnDistributeTreasury
			}

			private KinCityControlGump m_Gump = null;
			private DirtyState m_State = null;

			/// <summary>
			/// Initializes a new instance of the <see cref="PageFinance"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageFinance(KinCityControlGump gump)
			{
				m_Gump = gump;
				((ICommitGumpEntity)this).LoadStateInfo();
			}
			#region IGumpEntity Members

			/// <summary>
			/// Unqiue ID use as a key in the sesssion
			/// </summary>
			/// <value></value>
			string ICommitGumpEntity.ID
			{
				get { return "PageFinance"; }
			}

			/// <summary>
			/// Validate outstanding changes
			/// </summary>
			/// <returns></returns>
			bool ICommitGumpEntity.Validate()
			{
				if (!ReferenceEquals(m_Gump.From, m_Gump.Data.CityLeader) && m_Gump.From.AccessLevel <= AccessLevel.Player) return false;
				return true;
			}

			/// <summary>
			/// Commit any outstanding changes
			/// </summary>
			/// <param name="sender"></param>
			void ICommitGumpEntity.CommitChanges()
			{
				if (m_State.IsValueDirty<double>("Tax"))
				{
					m_Gump.Data.TaxRate = m_State.GetValue<double>("Tax");
					m_Gump.From.SendMessage("You have successfully adjusted the tax rate");
				}

				if (m_State.IsValueDirty<bool>("Distribute"))
				{
					if (m_Gump.Data.Treasury > 0)
					{
						m_Gump.Data.DistributeTreasury();
						m_Gump.From.SendMessage("You have successfully distributed the treasury amongst the beneficiaries");
					}
					else
					{
						m_Gump.From.SendMessage("There is no gold in the treasury to distribute.");
					}
				
				}
			}

			/// <summary>
			/// Creation of the entity's graphics
			/// </summary>
			void ICommitGumpEntity.Create()
			{
				#region page 2

				//	Page 2 - Finances	 //////////////////////////////////////////////////////////////////////

				StringBuilder htmlSb = new StringBuilder("<basefont color=#FFCC00><center>City Finanaces</center></basefont>");
				m_Gump.AddHtml(170, 40, 409, 19, htmlSb.ToString(), (bool)false, (bool)false);
				htmlSb = new StringBuilder("<basefont color=white><center>Tax Rate</center></basefont>");
				m_Gump.AddHtml(170, 80, 409, 19, htmlSb.ToString(), (bool)false, (bool)false);
				htmlSb = new StringBuilder().AppendFormat("<basefont color={0}><center>{1}</center></basefont>", GetTaxColour(), string.Format("{0:0%}", m_State.GetValue<double>("Tax")));
				m_Gump.AddHtml(170, 110, 409, 19, htmlSb.ToString(), (bool)false, (bool)false);

				m_Gump.AddButton(355, 140, 2223, 2223, (int)Buttons.btnLowerTax, GumpButtonType.Reply, 0);
				m_Gump.AddButton(355 + 20, 140, 2224, 2224, (int)Buttons.btnRasieTax, GumpButtonType.Reply, 0);

				htmlSb = new StringBuilder("<basefont color=white><center>Treasury</center></basefont>");
				m_Gump.AddHtml(170, 170, 409, 19, htmlSb.ToString(), (bool)false, (bool)false);
				htmlSb = new StringBuilder();
				htmlSb.AppendFormat("<basefont color=gray><center>{0}</center></basefont>", string.Format("{0:n}", m_Gump.Data.Treasury));
				m_Gump.AddHtml(170, 200, 409, 19, htmlSb.ToString(), (bool)false, (bool)false);


				htmlSb = new StringBuilder();
				htmlSb.AppendFormat("<basefont color=white><center>Pressing the button below will distribute the treasury amongst your <br> beneficiaires.</center></basefont>", string.Format("{0:n}", m_Gump.Data.Treasury));
				m_Gump.AddHtml(170, 230, 409,50, htmlSb.ToString(), (bool)false, (bool)false);
				//m_Gump.AddButton(355, 300, 2642, 2643, 3, GumpButtonType.Reply, 0);
				m_Gump.AddButton(355, 300, m_State.IsValueDirty<bool>("Distribute") ? 4036 : 4037, m_State.IsValueDirty<bool>("Distribute") ? 4037 : 4036, (int)Buttons.btnDistributeTreasury, GumpButtonType.Reply, 0);

				if (!ReferenceEquals(m_Gump.Data.CityLeader, m_Gump.From))
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>Only the city leader may make changes</center></basefont>", false, false);
				else if( m_State.IsValueDirty<double>("Tax") || m_State.IsValueDirty<bool>("Distribute") )
					m_Gump.AddHtml(169, 425, 412, 136, "<basefont color=gray><center>You must press OK before these changes will take effect</center></basefont>", false, false);

				//////////////////////////////////////////////////////////////////////////////////////////////

				#endregion
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
					m_State.SetValue("Tax", m_Gump.Data.TaxRate);
					m_State.SetValue("Distribute", false);
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
				//Only the leader can change stuff here
				if (!ReferenceEquals(m_Gump.Data.CityLeader, sender.Mobile) && sender.Mobile.AccessLevel <= AccessLevel.Player)
					return GumpReturnType.None;

				switch ((Buttons)info.ButtonID)
				{
					case Buttons.btnLowerTax:
						{
							if (m_State.GetValue<double>("Tax") > 0.00)
							{
								m_State.SetValue("Tax", m_State.GetValue<double>("Tax") - 0.01);
							}
							break;
						}
					case Buttons.btnRasieTax:
						{
							if (m_State.GetValue<double>("Tax") < 0.5)
							{
								m_State.SetValue("Tax", m_State.GetValue<double>("Tax") + 0.01);
							}
							break;
						}
					case Buttons.btnDistributeTreasury:
						{
							//Toggle distribution flag
							m_State.SetValue("Distribute", !m_State.GetValue<bool>("Distribute"));
							return GumpReturnType.None;
						}
					default:
						break;
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
			/// Gets the tax colour.
			/// </summary>
			/// <returns></returns>
			private string GetTaxColour()
			{
				if (!m_State.IsValueDirty<double>("Tax"))
				{
					return "gray";  //Normal
				}
				else if (m_State.GetValue<double>("Tax") > m_State.GetOriginalValue<double>("Tax"))
				{
					return "green"; //Higher - green
				}
				else
				{
					return "red"; //Lower - red
				}
			}
		}
	}
}
