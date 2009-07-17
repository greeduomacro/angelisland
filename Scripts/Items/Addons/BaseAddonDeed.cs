/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Items/Addons/BaseAddonDeed.cs
 * ChangeLog
 *  9/03/06 Taran Kain
 *		Changed targeting process, removed BlockingDoors().
 *  9/01/06 Taran Kain
 *		Added call to new BaseAddon.OnPlaced() hook
 *	5/17/05, erlein
 *		Altered BlockingObject() to perform single test instance instead of one per addon type
 *		that doesn't block.
 *	9/19/04, mith
 *		InternalTarget.OnTarget(): Added call to ConfirmAddonPlacementGump to allow user to cancel placement without losing original deed.
 *	9/18/04, Adam
 *		Add the new function BlockingObject() to determine if something would block the door.
 * 		Pass the result of BlockingObject() to addon.CouldFit().
 *  8/5/04, Adam
 * 		Changed item to LootType.Regular from LootType.Newbied.
 */

using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Targeting;

namespace Server.Items
{
	[Flipable( 0x14F0, 0x14EF )]
	public abstract class BaseAddonDeed : Item
	{
		public abstract BaseAddon Addon{ get; }

		public BaseAddonDeed() : base( 0x14F0 )
		{
			Weight = 1.0;

			if ( !Core.AOS )
				LootType = LootType.Regular;
		}

		public BaseAddonDeed( Serial serial ) : base( serial )
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

			if ( Weight == 0.0 )
				Weight = 1.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.SendMessage("Target where thy wouldst build thy addon, or target thyself to build it where thou'rt standing.");
				from.Target = new InternalTarget( this );
			}
			else
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
		}

		public void Place(IPoint3D p, Map map, Mobile from)
		{
			if ( p == null || map == null || this.Deleted )
				return;

			if ( IsChildOf( from.Backpack ) )
			{
				BaseAddon addon = Addon; // this creates an instance, don't use Addon (capital A) more than once!
				
				Server.Spells.SpellHelper.GetSurfaceTop( ref p );

				ArrayList houses = null;

				AddonFitResult res = addon.CouldFit( addon.BlocksDoors, p, map, from, ref houses );

				if ( res == AddonFitResult.Valid )
					addon.MoveToWorld( new Point3D(p), map );
				else if ( res == AddonFitResult.Blocked )
					from.SendLocalizedMessage( 500269 ); // You cannot build that there.
				else if ( res == AddonFitResult.NotInHouse )
					from.SendLocalizedMessage( 500274 ); // You can only place this in a house that you own!
				else if ( res == AddonFitResult.DoorsNotClosed )
					from.SendMessage( "You must close all house doors before placing this." );
				else if ( res == AddonFitResult.DoorTooClose )
					from.SendLocalizedMessage( 500271 ); // You cannot build near the door.

				if ( res == AddonFitResult.Valid )
				{
					Delete();

					if ( houses != null )
					{
						foreach ( Server.Multis.BaseHouse h in houses )
						{
							h.Addons.Add( addon );
							addon.OnPlaced(from, h);
						}

						from.SendGump( new ConfirmAddonPlacementGump( from, addon ) );
					}
				}
				else
				{
					addon.Delete();
				}
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}

		private class InternalTarget : Target
		{
			private BaseAddonDeed m_Deed;
			private bool m_SecondTime;
			private Point3D m_Point;

			public InternalTarget( BaseAddonDeed deed ) : this( deed, false, Point3D.Zero )
			{
			}

			private InternalTarget( BaseAddonDeed deed, bool secondtime, Point3D point ) : base( -1, true, TargetFlags.None )
			{
				m_Deed = deed;
				m_SecondTime = secondtime;
				m_Point = point;

				CheckLOS = false;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if (from == targeted)
				{
					if (m_SecondTime)
					{
						m_Deed.Place(m_Point as IPoint3D, from.Map, from);
						
						return;
					}
					else
					{
						from.Target = new InternalTarget(m_Deed, true, from.Location);
						from.SendMessage("Now, walk thyself away from this place and target thyself again.");

						return;
					}
				}

				m_Deed.Place(targeted as IPoint3D, from.Map, from);
			}
		}
	}
}


