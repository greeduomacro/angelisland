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

/* Items/SkillItems/Tinkering/Spyglass.cs
 * CHANGELOG:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;

namespace Server.Items
{
	[Flipable( 0x14F5, 0x14F6 )]
	public class Spyglass : Item
	{
		[Constructable]
		public Spyglass() : base( 0x14F5 )
		{
			Weight = 3.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1008155 ); // You peer into the heavens, seeking the moons...

			from.Send( new MessageLocalizedAffix( from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 1008146 + (int)Clock.GetMoonPhase( Map.Trammel, from.X, from.Y ), "", AffixType.Prepend, "Trammel : ", "" ) );
			from.Send( new MessageLocalizedAffix( from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 1008146 + (int)Clock.GetMoonPhase( Map.Felucca, from.X, from.Y ), "", AffixType.Prepend, "Felucca : ", "" ) );

			PlayerMobile player = from as PlayerMobile;

			if ( player != null )
			{
				QuestSystem qs = player.Quest;

				if ( qs is WitchApprenticeQuest )
				{
					FindIngredientObjective obj = qs.FindObjective( typeof( FindIngredientObjective ) ) as FindIngredientObjective;

					if ( obj != null && !obj.Completed && obj.Ingredient == Ingredient.StarChart )
					{
						int hours, minutes;
						Clock.GetTime( from.Map, from.X, from.Y, out hours, out minutes );

						if ( hours < 5 || hours > 17 )
						{
							player.SendLocalizedMessage( 1055040 ); // You gaze up into the glittering night sky.  With great care, you compose a chart of the most prominent star patterns.

							obj.Complete();
						}
						else
						{
							player.SendLocalizedMessage( 1055039 ); // You gaze up into the sky, but it is not dark enough to see any stars.
						}
					}
				}
			}
		}

		public Spyglass( Serial serial ) : base( serial )
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