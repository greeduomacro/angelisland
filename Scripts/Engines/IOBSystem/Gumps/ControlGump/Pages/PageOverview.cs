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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/Pages/PageOverview.cs
 * CHANGELOG:
 *	05/25/09, plasma
 *		- Changed to used get kin description method
 *	04/27/09, plasma
 *		- Added special case city text for nujel'm and skara
 *		- Shifted overview text across and down a bit so it doesn't look so ghey
 *	04/06/09, plasma
 *		Made the overview actually show the correct data not just some defaults!
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

		private sealed class PageOverview : ICommitGumpEntity
		{
			private KinCityControlGump m_Gump = null;
			/// <summary>
			/// Initializes a new instance of the <see cref="PageOverview"/> class.
			/// </summary>
			/// <param name="gump">The gump.</param>
			public PageOverview(KinCityControlGump gump)
			{
				m_Gump = gump;
				//no state for the overview
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
				get { return "PageOverview"; }
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
				#region page 1

				//	Page 1 - overview	 //////////////////////////////////////////////////////////////////////

				StringBuilder htmlSb = new StringBuilder("<basefont color=#FFCC00><center>City Overview - ").Append(GetCityNameString()).Append("</center></basefont>");
				m_Gump.AddHtml(170, 40, 409, 19, htmlSb.ToString(), (bool)false, (bool)false); //left

				htmlSb = new StringBuilder("<basefont color=white><p align=\"right\">City Leader :</p>");
				htmlSb.Append(("<p align=\"right\">Controlling Kin :</p>"));
				htmlSb.Append(("<p align=\"right\">Tax Rate :</p>"));
				htmlSb.Append(("<p align=\"right\">Beneficiaries :</p>"));
				htmlSb.Append(("<p align=\"right\">Treasury :</p>"));
				htmlSb.Append(("<p align=\"right\">City Guards :</p>"));
				
				htmlSb.Append("</basefont>");

				m_Gump.AddHtml(175, 100, 205, 361, htmlSb.ToString(), (bool)false, (bool)false);  //top

				htmlSb = new StringBuilder("<basefont color=gray><p>").Append(m_Gump.Data.CityLeader != null ? m_Gump.Data.CityLeader.Name : "Unknown").Append("</p>");
				htmlSb.Append("<p>").Append(IOBSystem.GetIOBName(m_Gump.Data.ControlingKin)).Append("</p>");
				htmlSb.AppendFormat("<p>{0}</p>", string.Format("{0:0%}", m_Gump.Data.TaxRate));
				htmlSb.AppendFormat("<p>{0}</p>", m_Gump.Data.BeneficiaryDataList.Count);
				htmlSb.AppendFormat("<p>{0}</P>", string.Format("{0:n}", m_Gump.Data.Treasury));
				htmlSb.AppendFormat("<p>{0}</p>", GetGuardTypeString());
				
				htmlSb.Append("</basefont>");

				m_Gump.AddHtml(395, 100, 199, 361, htmlSb.ToString(), (bool)false, (bool)false); //right

				//////////////////////////////////////////////////////////////////////////////////////////////

				#endregion

			}

			/// <summary>
			/// Restore data from the session
			/// </summary>
			void ICommitGumpEntity.LoadStateInfo()
			{
				//no state for the overview
			}

			/// <summary>
			/// Handle user response
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="info"></param>
			/// <returns></returns>
			CommitGumpBase.GumpReturnType ICommitGumpEntity.OnResponse(Server.Network.NetState sender, RelayInfo info)
			{
				return CommitGumpBase.GumpReturnType.None;
				//no state for the overview
			}

			/// <summary>
			/// Update the state / session with any changes in memory
			/// </summary>
			void ICommitGumpEntity.SaveStateInfo()
			{

			}

			#endregion

			private string GetCityNameString()
			{
				if( m_Gump.Data.City == KinFactionCities.SkaraBrae )
					return "Skara Brae";
				if( m_Gump.Data.City == KinFactionCities.Nujelm )
					return "Nujel'm";
				return m_Gump.Data.City.ToString();
			}

			/// <summary>
			/// Gets the guard type string.
			/// </summary>
			/// <returns></returns>
			private string GetGuardTypeString()
			{
				switch (m_Gump.Data.GuardOption )
				{
					case KinCityData.GuardOptions.None:
						return "None";
					case KinCityData.GuardOptions.LordBritish:
						return "Lord British";
					case KinCityData.GuardOptions.FactionOnly:
						return "Kin";
					case KinCityData.GuardOptions.FactionAndReds:
						return "Kin, Murderers";
					case KinCityData.GuardOptions.FactionAndRedsAndCrim:
						return "Kin, Murderers, Criminals";
					case KinCityData.GuardOptions.Everyone:
						return "Everyone";
					case KinCityData.GuardOptions.RedsAndCrim:
						return "Criminals & Murderers";
					case KinCityData.GuardOptions.Crim:
						return "Criminals";
					default:
						return "None";
				}
			}

		}
	}
}
