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
/* Changelog
 * 1/27/05 Darva
 *		Allow gates to be used by people other than owner.
 * 1/26/05 Darva,
 *		Prevent use of gate if in combat.
 * 1/25/05 Darva,
 *		Changes to constructor, for use with moonstone.cs
 */

using System;
using Server;
using Server.Network;
using Server.Engines.PartySystem;
using Server.Spells;

namespace Server.Items
{
	public class MoonstoneGate : Moongate
	{
		private Mobile m_Caster;
		private Point3D m_Destination;

		public MoonstoneGate( Point3D loc, Map map, Mobile caster, int hue, Point3D Destination ) : base( loc, map )
		{
			MoveToWorld( loc, map );
			Dispellable = false;
			Hue = hue;

			m_Caster = caster;
			m_Destination = Destination;
			base.Target = m_Destination;
			new InternalTimer( this ).Start();

			Effects.PlaySound( loc, map, 0x20E );
		}

		public MoonstoneGate( Serial serial ) : base( serial )
		{
		}

		public override void UseGate( Mobile m )
		{
				if ( SpellHelper.CheckCombat( m ) )
					m.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??

				else
					base.UseGate( m );
		
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

			Delete();
		}

		private class InternalTimer : Timer
		{
			private Item m_Item;

			public InternalTimer( Item item ) : base( TimeSpan.FromSeconds( 30.0 ) )
			{
				m_Item = item;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Item.Delete();
			}
		}
	}
}