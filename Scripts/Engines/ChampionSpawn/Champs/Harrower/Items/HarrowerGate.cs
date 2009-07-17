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

/* Scripts\Engines\ChampionSpawn\Champs\Harrower\Items\HarrowerGate.cs
 * CHANGELOG
 *  01/05/07, Plasma
 *      Changed CannedEvil namespace to ChampionSpawn for cleanup!
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using Server;

namespace Server.Items
{
	public class HarrowerGate : Moongate
	{
		private Mobile m_Harrower;

		public override int LabelNumber{ get{ return 1049498; } } // dark moongate

		public HarrowerGate( Mobile harrower, Point3D loc, Map map, Point3D targLoc, Map targMap ) : base( targLoc, targMap )
		{
			m_Harrower = harrower;

			Dispellable = false;
			ItemID = 0x1FD4;
			Light = LightType.Circle300;

			MoveToWorld( loc, map );
		}

		public HarrowerGate( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Harrower );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Harrower = reader.ReadMobile();

					if ( m_Harrower == null )
						Delete();

					break;
				}
			}

			if ( Light != LightType.Circle300 )
				Light = LightType.Circle300;
		}
	}
}