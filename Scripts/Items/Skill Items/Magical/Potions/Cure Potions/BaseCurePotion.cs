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

/* Scripts/Items/Skill Items/Magical/Potions/Cure Potions/BaseCurePotion.cs
 * ChangeLog
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 */
 
using System;
using Server;

namespace Server.Items
{
	public class CureLevelInfo
	{
		private Poison m_Poison;
		private double m_Chance;

		public Poison Poison
		{
			get{ return m_Poison; }
		}

		public double Chance
		{
			get{ return m_Chance; }
		}

		public CureLevelInfo( Poison poison, double chance )
		{
			m_Poison = poison;
			m_Chance = chance;
		}
	}

	public abstract class BaseCurePotion : BasePotion
	{
		public abstract CureLevelInfo[] LevelInfo{ get; }

		public BaseCurePotion( PotionEffect effect ) : base( 0xF07, effect )
		{
		}

		public BaseCurePotion( Serial serial ) : base( serial )
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

		public void DoCure( Mobile from )
		{
			bool cure = false;

			CureLevelInfo[] info = LevelInfo;

			for ( int i = 0; i < info.Length; ++i )
			{
				CureLevelInfo li = info[i];

				if ( li.Poison == from.Poison && Scale( from, li.Chance ) > Utility.RandomDouble() )
				{
					cure = true;
					break;
				}
			}

			if ( cure && from.CurePoison( from ) )
			{
				from.SendLocalizedMessage( 500231 ); // You feel cured of poison!

				from.FixedEffect( 0x373A, 10, 15 );
				from.PlaySound( 0x1E0 );
			}
			else if ( !cure )
			{
				from.SendLocalizedMessage( 500232 ); // That potion was not strong enough to cure your ailment!
			}
		}

		public override void Drink( Mobile from )
		{
			//if ( Spells.Necromancy.TransformationSpell.UnderTransformation( from, typeof( Spells.Necromancy.VampiricEmbraceSpell ) ) )
			//{
			//	from.SendLocalizedMessage( 1061652 ); // The garlic in the potion would surely kill you.
			//}
			//else 
			if ( from.Poisoned )
			{
				DoCure( from );

				BasePotion.PlayDrinkEffect( from );

				from.FixedParticles( 0x373A, 10, 15, 5012, EffectLayer.Waist );
				from.PlaySound( 0x1E0 );

				this.Delete();
			}
			else
			{
				from.SendLocalizedMessage( 1042000 ); // You are not poisoned.
			}
		}
	}
}