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

/* Scripts/Engines/IOBSystem/Gumps/KinCityStatusGump.cs
 * CHANGELOG:
 *	06/27/09, plasma
 *		Added LB indicator
 *	05/25/09, plasma
 *		Changed colour to gray, changed to use get kin desription method.
 *	02/10/08 - Plasma,
 *		Initial creation
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Engines.IOBSystem;
using System.Text;

namespace Server.Gumps
{

	/// <summary>
	/// 10 min temp gump jobby for testing.  Will make this pretty, nice etc for players laterrrrr!
	/// </summary>
	public class KinCityStatusGump : Gump
	{
		private enum Buttons
		{
			Ok = 1
		}

		public KinCityStatusGump()
			: base(25, 25)
		{

			this.Closable = true;
			this.Disposable = true;
			this.Dragable = true;
			this.Resizable = false;
			this.AddPage(0);
			this.AddImage(1, 6, 1228);
			this.AddButton(25, 268, 247, 249, (int)Buttons.Ok, GumpButtonType.Reply, 0);

			StringBuilder html = new StringBuilder("<basefont color=gray><p><center>City Status</center></p><p>");
			foreach (KinFactionCities e in Enum.GetValues(typeof(KinFactionCities)))
			{
				KinCityData cd = KinCityManager.GetCityData(e);
				if (cd == null) continue;

				html.Append(string.Format("<P><STRONG>{0}</STRONG> : ", e.ToString()));

				if (cd.ControlingKin == IOBAlignment.None)
				{
					html.Append("Golem Controller King</P>");
				}
				else
				{
					html.Append(string.Format("{0}</P>", IOBSystem.GetIOBName(cd.ControlingKin)));
					if (cd.GuardOption == KinCityData.GuardOptions.LordBritish)
						html.Append("<p>Lord British is guarding this city.</p>");
					foreach (KinCityData.BeneficiaryData vd in cd.BeneficiaryDataList)
						html.Append(string.Format("<P> * {0}</P>", vd.Pm.Name));
				}
			}
			html.Append("</P></P></basefont>");
			this.AddHtml(26, 40, 348, 214, html.ToString(), (bool)false, (bool)true);

		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{

		}
	}

}

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class CityStatusBoard : Item
	{
		[Constructable]
		public CityStatusBoard()
			: base(0x1E5E)
		{
			Movable = false;
			Hue = 0x2FF;
		}

		public CityStatusBoard(Serial serial)
			: base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{
			this.LabelTo(from, "City Status Board");
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (CheckRange(from))
			{
				from.SendGump(new KinCityStatusGump());
			}
			else
			{
				from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public virtual bool CheckRange(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			return (from.Map == this.Map && from.InRange(GetWorldLocation(), 2));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

}

