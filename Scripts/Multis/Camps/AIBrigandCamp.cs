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

// /Scripts/Multis/Camps/AIBrigandCamp.cs
// Created 3/27/04 by mith
// ChangeLog
// 3/27/04 
//	Copied from BankerCamp.cs. Replaced Bankers with Brigands. We spawn 4 brigands with a wander range of 5.
//	Removed doors and sign post from wagon.

using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Multis
{
	public class AIBrigandCamp : BaseCamp
	{
		[Constructable]
		public AIBrigandCamp() : base( 0x1F6 )	// BankerCamp type, seemed most appropriate
		{
		}

		public override void AddComponents()
		{
			/* 
			BaseDoor west, east;

			AddItem( west = new LightWoodGate( DoorFacing.WestCW ), -4, 4, 7 );
			AddItem( east = new LightWoodGate( DoorFacing.EastCCW ), -3, 4, 7 );

			west.Link = east;
			east.Link = west;

			AddItem( new Sign( SignType.Mage, SignFacing.West ), -5, 5, -4 );
			*/

			AddMobile( new Brigand(), 5, -4,  3, 7 );
			AddMobile( new Brigand(), 5,  4, -2, 0 );
			AddMobile( new Brigand(), 5, -2, -3, 0 );
			AddMobile( new Brigand(), 5,  2, -3, 0 );
		}

		public AIBrigandCamp( Serial serial ) : base( serial )
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