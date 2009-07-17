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
/* ChangeLog:
	3/30/05, Kit
		Initial Creation.
*/

using System;
using Server.Targeting;
using Server.Network;
using Server.Regions;
using Server.Items;

namespace Server.Spells.Third
{
	public class GenieTeleport : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"GenieTeleport", "Rel Por",
				SpellCircle.Third,
				215,
				9031,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public GenieTeleport( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override bool CheckCast()
		{
			if ( Server.Misc.WeightOverloading.IsOverloaded( Caster ) )
			{
				Caster.SendLocalizedMessage( 502359, "", 0x22 ); // Thou art too encumbered to move.
				return false;
			}

			return SpellHelper.CheckTravel( Caster, TravelCheckType.TeleportFrom );
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( IPoint3D p )
		{
			IPoint3D orig = p;
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop( ref p );

			if ( Server.Misc.WeightOverloading.IsOverloaded( Caster ) )
			{
				Caster.SendLocalizedMessage( 502359, "", 0x22 ); // Thou art too encumbered to move.
			}
			else if ( !SpellHelper.CheckTravel( Caster, TravelCheckType.TeleportFrom ) )
			{
			}
			else if ( !SpellHelper.CheckTravel( Caster, map, new Point3D( p ), TravelCheckType.TeleportTo ) )
			{
			}
			else if ( map == null || !map.CanSpawnMobile( p.X, p.Y, p.Z ) )
			{
				Caster.SendLocalizedMessage( 501942 ); // That location is blocked.
			}
			else if ( SpellHelper.CheckMulti( new Point3D( p ), map ) )
			{
				Caster.SendLocalizedMessage( 501942 ); // That location is blocked.
			}
			else if ( CheckSequence() )
			{
				SpellHelper.Turn( Caster, orig );

				Mobile m = Caster;

				Point3D from = m.Location;
				Point3D to = new Point3D( p );
				m.Location = to;
				m.ProcessDelta();

			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y, from.Z + 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y, from.Z ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y, from.Z - 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X, from.Y + 1, from.Z + 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X, from.Y + 1, from.Z ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X, from.Y + 1, from.Z - 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y + 1, from.Z + 11 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y + 1, from.Z + 7 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y + 1, from.Z + 3 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y + 1, from.Z - 1 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( from.X + 1, from.Y, from.Z + 4 ), m.Map, 0x3728, 13 );
			
			

			

			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y, to.Z ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y, to.Z - 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X, to.Y + 1, to.Z + 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X, to.Y + 1, to.Z ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X, to.Y + 1, to.Z - 4 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y + 1, to.Z + 11 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y + 1, to.Z + 7 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y + 1, to.Z + 3 ), m.Map, 0x3728, 13 );
			Effects.SendLocationEffect( new Point3D( to.X + 1, to.Y + 1, to.Z - 1 ), m.Map, 0x3728, 13 );

			m.PlaySound( 0x1FE );
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private GenieTeleport m_Owner;

			public InternalTarget( GenieTeleport owner ) : base( 12, true, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				IPoint3D p = o as IPoint3D;

				if ( p != null )
					m_Owner.Target( p );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}