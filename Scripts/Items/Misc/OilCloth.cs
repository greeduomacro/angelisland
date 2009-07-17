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

/* Scripts/Items/Misc/OilCloth.cs
 * ChangeLog:
 *	11/29/05, erlein
 *		Altered to HueMod back to default (-1) rather than alter body type.
 * 
 */

using System;
using Server;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class OilCloth : Item, IScissorable, IDyable
	{
		public override int LabelNumber{ get{ return 1041498; } } // oil cloth

		[Constructable]
		public OilCloth() : this( 1 )
		{
		}

		[Constructable]
		public OilCloth( int amount ) : base( 0x175D )
		{
			Weight = 1.0;
			Hue = 2001;
			Stackable = true;
			Amount = amount;
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new OilCloth( amount ), amount );
		}

		public bool Dye( Mobile from, DyeTub sender )
		{
			if ( Deleted )
				return false;

			Hue = sender.DyedHue;

			return true;
		}

		public bool Scissor( Mobile from, Scissors scissors )
		{
			if ( Deleted || !from.CanSee( this ) )
				return false;

			base.ScissorHelper( from, new Bandage(), 1 );

			return true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.BeginTarget( -1, false, TargetFlags.None, new TargetCallback( OnTarget ) );
				from.SendLocalizedMessage( 1005424 ); // Select the weapon or armor you wish to use the cloth on.
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}

		public void OnTarget( Mobile from, object obj )
		{
			// TODO: Need details on how oil cloths should get consumed here

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( obj is BaseWeapon )
			{
				BaseWeapon weapon = (BaseWeapon)obj;

				if ( weapon.RootParent != from )
				{
					from.SendLocalizedMessage( 1005425 ); // You may only wipe down items you are holding or carrying.
				}
				else if ( weapon.Poison == null || weapon.PoisonCharges <= 0 )
				{
					from.LocalOverheadMessage( Network.MessageType.Regular, 0x3B2, 1005422 ); // Hmmmm... this does not need to be cleaned.
				}
				else
				{
					if ( weapon.PoisonCharges < 2 )
						weapon.PoisonCharges = 0;
					else
						weapon.PoisonCharges -= 2;

					if ( weapon.PoisonCharges > 0 )
						from.SendLocalizedMessage( 1005423 ); // You have removed some of the caustic substance, but not all.
					else
						from.SendLocalizedMessage( 1010497 ); // You have cleaned the item.
				}
			}
			else if ( obj == from && obj is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)obj;

				if ( pm.HueMod == 0 )		//( pm.BodyMod == 183 || pm.BodyMod == 184 )
				{
					pm.SavagePaintExpiration = TimeSpan.Zero;

					pm.HueMod = -1;
					pm.BodyMod = 0;
					
					from.SendLocalizedMessage( 1040006 ); // You wipe away all of your body paint.

					Consume();
				}
				else
				{
					from.LocalOverheadMessage( Network.MessageType.Regular, 0x3B2, 1005422 ); // Hmmmm... this does not need to be cleaned.
				}
			}
			else
			{
				from.SendLocalizedMessage( 1005426 ); // The cloth will not work on that.
			}
		}

		public OilCloth( Serial serial ) : base( serial )
		{
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