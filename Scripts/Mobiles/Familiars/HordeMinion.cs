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

/* Scripts/Mobiles/Familiars/HordeMinion.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 5 lines removed.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a horde minion corpse" )]
	public class HordeMinionFamiliar : BaseFamiliar
	{
		public HordeMinionFamiliar()
		{
			Name = "a horde minion";
			Body = 776;
			BaseSoundID = 0x39D;

			SetStr( 100 );
			SetDex( 110 );
			SetInt( 100 );

			SetHits( 70 );
			SetStam( 110 );
			SetMana( 0 );

			SetDamage( 5, 10 );



			SetSkill( SkillName.Wrestling, 70.1, 75.0 );
			SetSkill( SkillName.Tactics, 50.0 );

			ControlSlots = 1;

			Container pack = Backpack;

			if ( pack != null )
				pack.Delete();

			pack = new StrongBackpack();
			pack.Movable = false;

			AddItem( pack );
		}

		private DateTime m_NextPickup;

		public override void OnThink()
		{
			base.OnThink();

			if ( DateTime.Now < m_NextPickup )
				return;

			m_NextPickup = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 5, 10 ) );

			Container pack = this.Backpack;

			if ( pack == null )
				return;

			ArrayList list = new ArrayList();

			IPooledEnumerable eable = this.GetItemsInRange( 2 );
			foreach ( Item item in eable)
			{
				if ( item.Movable && item.Stackable )
					list.Add( item );
			}
			eable.Free();

			int pickedUp = 0;

			for ( int i = 0; i < list.Count; ++i )
			{
				Item item = (Item)list[i];

				if ( !pack.CheckHold( this, item, false, true ) )
					return;

				bool rejected;
				LRReason reject;

				NextActionTime = DateTime.Now;

				Lift( item, item.Amount, out rejected, out reject );

				if ( rejected )
					continue;

				Drop( this, Point3D.Zero );

				if ( ++pickedUp == 3 )
					break;
			}
		}

		private void ConfirmRelease_Callback( Mobile from, bool okay, object state )
		{
			if ( okay )
				EndRelease( from );
		}

		public override void BeginRelease( Mobile from )
		{
			Container pack = this.Backpack;

			if ( pack != null && pack.Items.Count > 0 )
				from.SendGump( new WarningGump( 1060635, 30720, 1061672, 32512, 420, 280, new WarningGumpCallback( ConfirmRelease_Callback ), null ) );
			else
				EndRelease( from );
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

		public HordeMinionFamiliar( Serial serial ) : base( serial )
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
