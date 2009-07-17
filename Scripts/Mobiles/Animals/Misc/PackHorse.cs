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

/* Scripts/Mobiles/Animals/Misc/PackHorse.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server.Mobiles;
using Server.Items;
using Server.ContextMenus;

namespace Server.Mobiles
{
	[CorpseName( "a horse corpse" )]
	public class PackHorse : BaseCreature
	{
		[Constructable]
		public PackHorse() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.25, 0.5 )
		{
			Name = "a pack horse";
			Body = 291;
			BaseSoundID = 0xA8;

			SetStr( 44, 120 );
			SetDex( 36, 55 );
			SetInt( 6, 10 );

			SetHits( 61, 80 );
			SetStam( 81, 100 );
			SetMana( 0 );

			SetDamage( 5, 11 );



			SetSkill( SkillName.MagicResist, 25.1, 30.0 );
			SetSkill( SkillName.Tactics, 29.3, 44.0 );
			SetSkill( SkillName.Wrestling, 29.3, 44.0 );

			Fame = 0;
			Karma = 200;

			VirtualArmor = 16;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;

			Container pack = Backpack;

			if ( pack != null )
				pack.Delete();

			pack = new StrongBackpack();
			pack.Movable = false;

			AddItem( pack );
		}

		public override int Meat{ get{ return 3; } }
		public override int Hides{ get{ return 10; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public PackHorse( Serial serial ) : base( serial )
		{
		}

		#region Pack Animal Methods
		public override bool OnBeforeDeath()
		{
			if ( !base.OnBeforeDeath() )
				return false;

			PackAnimal.CombineBackpacks( this );

			return true;
		}

		public override bool IsSnoop( Mobile from )
		{
			if ( PackAnimal.CheckAccess( this, from ) )
				return false;

			return base.IsSnoop( from );
		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			if ( CheckFeed( from, item ) )
				return true;

			if ( PackAnimal.CheckAccess( this, from ) )
			{
				AddToBackpack( item );
				return true;
			}

			return base.OnDragDrop( from, item );
		}

		public override bool CheckNonlocalDrop( Mobile from, Item item, Item target )
		{
			return PackAnimal.CheckAccess( this, from );
		}

		public override bool CheckNonlocalLift( Mobile from, Item item )
		{
			return PackAnimal.CheckAccess( this, from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			PackAnimal.TryPackOpen( this, from );
		}

		public override void GetContextMenuEntries( Mobile from, System.Collections.ArrayList list )
		{
			base.GetContextMenuEntries( from, list );

			PackAnimal.GetContextMenuEntries( this, from, list );
		}
		#endregion

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

	public class PackAnimalBackpackEntry : ContextMenuEntry
	{
		private BaseCreature m_Animal;
		private Mobile m_From;

		public PackAnimalBackpackEntry( BaseCreature animal, Mobile from ) : base( 6145, 3 )
		{
			m_Animal = animal;
			m_From = from;

			if ( animal.IsDeadPet )
				Enabled = false;
		}

		public override void OnClick()
		{
			PackAnimal.TryPackOpen( m_Animal, m_From );
		}
	}

	public class PackAnimal
	{
		public static void GetContextMenuEntries( BaseCreature animal, Mobile from, System.Collections.ArrayList list )
		{
			if ( CheckAccess( animal, from ) )
				list.Add( new PackAnimalBackpackEntry( animal, from ) );
		}

		public static bool CheckAccess( BaseCreature animal, Mobile from )
		{
			if ( from == animal || from.AccessLevel >= AccessLevel.GameMaster )
				return true;

			if ( from.Alive && animal.Controlled && !animal.IsDeadPet && (from == animal.ControlMaster || from == animal.SummonMaster) )
				return true;

			return false;
		}

		public static void CombineBackpacks( BaseCreature animal )
		{
			//if ( animal.IsBonded || animal.IsDeadPet )
			//	return;

			Container pack = animal.Backpack;

			if ( pack != null )
			{
				Container newPack = new Backpack();

				for ( int i = pack.Items.Count - 1; i >= 0; --i )
				{
					if ( i >= pack.Items.Count )
						continue;

					newPack.DropItem( (Item)pack.Items[i] );
				}

				pack.DropItem( newPack );
			}
		}

		public static void TryPackOpen( BaseCreature animal, Mobile from )
		{
			if ( animal.IsDeadPet )
				return;

			Container item = animal.Backpack;

			if ( item != null )
				from.Use( item );
		}
	}
}
