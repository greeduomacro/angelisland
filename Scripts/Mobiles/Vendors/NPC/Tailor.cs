/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
//  /Scripts/Mobiles/Vendors/NPC/Tailor.cs
//	CHANGE LOG
//  03/29/2004 - Pulse
//		Removed ability for this NPC vendor to support Bulk Order Deeds
//		Tailors will no longer issue or accept these deeds.

using System;
using System.Collections;
using Server;
using Server.Engines.BulkOrders;

namespace Server.Mobiles
{
	public class Tailor : BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos{ get { return m_SBInfos; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.TailorsGuild; } }

		[Constructable]
		public Tailor() : base( "the tailor" )
		{
			SetSkill( SkillName.Tailoring, 64.0, 100.0 );
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add( new SBTailor() );
		}

		public override VendorShoeType ShoeType
		{
			get{ return Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes; }
		}

		#region Bulk Orders
		public override Item CreateBulkOrder( Mobile from, bool fromContextMenu )
		{
			PlayerMobile pm = from as PlayerMobile;

			if ( pm != null && pm.NextTailorBulkOrder == TimeSpan.Zero && (fromContextMenu || 0.2 > Utility.RandomDouble()) )
			{
				double theirSkill = pm.Skills[SkillName.Tailoring].Base;

				if ( theirSkill >= 70.1 )
					pm.NextTailorBulkOrder = TimeSpan.FromHours( 6.0 );
				else if ( theirSkill >= 50.1 )
					pm.NextTailorBulkOrder = TimeSpan.FromHours( 2.0 );
				else
					pm.NextTailorBulkOrder = TimeSpan.FromHours( 1.0 );

				if ( theirSkill >= 70.1 && ((theirSkill - 40.0) / 300.0) > Utility.RandomDouble() )
					return new LargeTailorBOD();

				return SmallTailorBOD.CreateRandomFor( from );
			}

			return null;
		}

		public override bool IsValidBulkOrder( Item item )
		{
			return ( item is SmallTailorBOD || item is LargeTailorBOD );
		}

		public override bool SupportsBulkOrders( Mobile from )
		{
			// The following line allows this NPC to support the BOD system. 
			//return ( from is PlayerMobile && from.Skills[SkillName.Tailoring].Base > 0 );
			// return false from this function to disable BOD support.
			return false;
		}

		public override TimeSpan GetNextBulkOrder( Mobile from )
		{
			if ( from is PlayerMobile )
				return ((PlayerMobile)from).NextTailorBulkOrder;

			return TimeSpan.Zero;
		}
		#endregion

		public Tailor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
