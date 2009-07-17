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

/* Scripts/Items/Misc/TribalPaint.cs
 * Changelog
 *	11/29/05, erlein
 *		Removed alteration of body type to fix sprite problem with sitting whilst
 *		paint is applied.
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 */
using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
	public class TribalPaint : Item
	{
		public override int LabelNumber{ get{ return 1040000; } } // savage kin paint

		[Constructable]
		public TribalPaint() : base( 0x9EC )
		{
			Hue = 2101;
			Weight = 2.0;
		}

		public TribalPaint( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				if ( !from.CanBeginAction( typeof( Spells.Fifth.IncognitoSpell ) ) )
				{
					from.SendLocalizedMessage( 501698 ); // You cannot disguise yourself while incognitoed.
				}
				else if ( !from.CanBeginAction( typeof( Spells.Seventh.PolymorphSpell ) ) )
				{
					from.SendLocalizedMessage( 501699 ); // You cannot disguise yourself while polymorphed.
				}
				//else if ( Spells.Necromancy.TransformationSpell.UnderTransformation( from ) )
				//{
					//from.SendLocalizedMessage( 501699 ); // You cannot disguise yourself while polymorphed.
				//}
				else if ( from.HueMod != -1 || from.FindItemOnLayer( Layer.Helm ) is OrcishKinMask )
				{
					from.SendLocalizedMessage( 501605 ); // You are already disguised.
				}
				else
				{
					// erl: removed to fix sprite animation problem     :	from.BodyMod = ( from.Female ? 184 : 183 );	:
					from.HueMod = 0;

					if ( from is PlayerMobile )
						((PlayerMobile)from).SavagePaintExpiration = TimeSpan.FromDays( 7.0 );

					from.SendLocalizedMessage( 1042537 ); // You now bear the markings of the savage tribe.  Your body paint will last about a week or you can remove it with an oil cloth.

					Consume();
				}
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}