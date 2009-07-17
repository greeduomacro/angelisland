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

using System;
using Server;
using Server.Network;

namespace Server.Items
{
	public abstract class BaseHealPotion : BasePotion
	{
		public abstract int MinHeal { get; }
		public abstract int MaxHeal { get; }

		public BaseHealPotion( PotionEffect effect ) : base( 0xF0C, effect )
		{
		}

		public BaseHealPotion( Serial serial ) : base( serial )
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

		public void DoHeal( Mobile from )
		{
			int min = Scale( from, MinHeal );
			int max = Scale( from, MaxHeal );

			from.Heal( Utility.RandomMinMax( min, max ) );
		}

		public override void Drink( Mobile from )
		{
			if ( from.Hits < from.HitsMax )
			{
				if ( from.Poisoned || MortalStrike.IsWounded( from ) )
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x22, 1005000 ); // You can not heal yourself in your current state.
				}
				else
				{
					if ( from.BeginAction( typeof( BaseHealPotion ) ) )
					{
						DoHeal( from );

						BasePotion.PlayDrinkEffect( from );

						this.Delete();

						Timer.DelayCall( TimeSpan.FromSeconds( 10.0 ), new TimerStateCallback( ReleaseHealLock ), from );
					}
					else
					{
						from.LocalOverheadMessage( MessageType.Regular, 0x22, 500235 ); // You must wait 10 seconds before using another healing potion.
					}
				}
			}
			else
			{
				from.SendLocalizedMessage( 1049547 ); // You decide against drinking this potion, as you are already at full health.
			}
		}

		private static void ReleaseHealLock( object state )
		{
			((Mobile)state).EndAction( typeof( BaseHealPotion ) );
		}
	}
} 
