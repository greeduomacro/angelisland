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

/* Scripts/Engines/IOBSystem/Gumps/ControlGump/KinCityControlGump.cs
 * CHANGELOG:
 *	07/19/09, plasma
 *		More NULL checks in case of missing or corrupt city data
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
	public sealed partial class KinCityControlGump : CommitGumpBase
	{
		private KinCityData m_Data;

		/// <summary>
		/// Gets the city.
		/// </summary>
		/// <value>The city.</value>
		public KinFactionCities City
		{
			get
			{
				return ((KinFactionCities)Session["City"]);
			}
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>The data.</value>
		public KinCityData Data
		{
			get
			{
				if (m_Data == null)
				{
					m_Data = KinCityManager.GetCityData(City);
				}
				return m_Data;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityControlGump"/> class.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="session">The session.</param>
		public KinCityControlGump(int page, GumpSession session, Mobile from)
			: base(page, session, from)
		{
			//This ctor is called from a continuation
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityControlGump"/> class.
		/// </summary>
		/// <param name="city">The city.</param>
		public KinCityControlGump(KinFactionCities city, Mobile from)
			: this(city, 1, from)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityControlGump"/> class.
		/// </summary>
		/// <param name="city">The city.</param>
		/// <param name="page">The page.</param>
		public KinCityControlGump(KinFactionCities city, int page, Mobile from)
			: this(city, page, null, from)
		{ }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="city"></param>
		/// <param name="page"></param>
		/// <param name="session"></param>
		private KinCityControlGump(KinFactionCities city, int page, GumpSession session, Mobile from)
			: base(page, session, from)
		{
			//This ctor gets called the first time the gump is opened			
			m_Data = KinCityManager.GetCityData(city);
			if (m_Data == null) return;

			Session["City"] = city;

			//If in the vote stage, sett page to 5 now
			if (m_Data.IsVotingStage)
			{
				Page = 5;
			}

			SetCurrentPage();
			if (MasterPage != null)
			{
				MasterPage.Create();
				if (CurrentPage != null) this.AddPage(1);
			}
			if (CurrentPage != null) CurrentPage.Create();


		}

		/// <summary>
		/// Registers the entities.
		/// </summary>
		protected override void RegisterEntities()
		{
			m_EntityRegister.Add(0, typeof(PageMaster));
			m_EntityRegister.Add(1, typeof(PageOverview));
			m_EntityRegister.Add(2, typeof(PageFinance));
			m_EntityRegister.Add(3, typeof(PagePopulace));
			m_EntityRegister.Add(4, typeof(PageDelegation));
			m_EntityRegister.Add(5, typeof(PageVote));
		}

		/// <summary>
		/// Sets the current page.
		/// </summary>
		protected override void SetCurrentPage()
		{
			//Create master page
			MasterPage = new PageMaster(this);

			//Create whatever other page currently selected
			switch (Page)
			{
				case 1:
					{
						CurrentPage = new PageOverview(this);
						break;
					}
				case 2:
					{
						CurrentPage = new PageFinance(this);
						break;
					}
				case 3:
					{
						CurrentPage = new PagePopulace(this);
						break;
					}
				case 4:
					{
						CurrentPage = new PageDelegation(this);
						break;
					}
				case 5:
					{
						CurrentPage = new PageVote(this);
						break;
					}


			}
		}
	}

}



namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class CityControlBoard : Item
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CityControlBoard"/> class.
		/// </summary>
		[Constructable]
		public CityControlBoard()
			: base(0x1E5E)
		{
			Movable = false;
			Hue = 0x2FF;
		}

		public KinFactionCities KinFactionCity
		{
			get
			{
				KinCityRegion region = KinCityRegion.GetKinCityAt(this);
				if (region == null) return KinFactionCities.Cove;
				return region.KinFactionCity;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CityControlBoard"/> class.
		/// </summary>
		/// <param name="serial">The serial.</param>
		public CityControlBoard(Serial serial)
			: base(serial)
		{
		}

		/// <summary>
		/// Called when [single click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnSingleClick(Mobile from)
		{
			this.LabelTo(from, "City Control Board");
		}

		/// <summary>
		/// Called when [double click].
		/// </summary>
		/// <param name="from">From.</param>
		public override void OnDoubleClick(Mobile from)
		{
			if (!(from is PlayerMobile)) return;
			if (CheckRange(from))
			{
				KinCityRegion region = KinCityRegion.GetKinCityAt(this);
				if (region == null)
				{
					from.SendMessage("This board is not placed within a Kin City");
					return;
				}

				if (from.AccessLevel <= AccessLevel.Player)
				{
					KinCityData data = KinCityManager.GetCityData(region.KinFactionCity);
					if (data == null) return;

					if (data.ControlingKin == IOBAlignment.None)
					{
						from.SendMessage("This city is controlled by the Golem Controller Lord!");
						return;
					}

					if (((PlayerMobile)from).IOBRealAlignment != data.ControlingKin)
					{
						from.SendMessage("You are not aligned with {0}", data.ControlingKin.ToString());
						return;
					}
				}

				from.CloseGump(typeof(KinCityControlGump));
				from.SendGump(new KinCityControlGump(region.KinFactionCity, from));
			}
			else
			{
				from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		/// <summary>
		/// Checks the range.
		/// </summary>
		/// <param name="from">From.</param>
		/// <returns></returns>
		public virtual bool CheckRange(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			return (from.Map == this.Map && from.InRange(GetWorldLocation(), 2));
		}

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}



}