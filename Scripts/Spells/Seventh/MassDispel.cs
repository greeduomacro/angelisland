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

/* Scripts\Spells\Seventh\MassDispel.cs
 * ChangeLog:
 *  7/23/06, Kit
 *		made targeted version work on genies.
 *  6/01/05, Kit
 *		Increased targeted mass dispell chance to 99% vs anything else besides demons which is now 80%
 *		made internal target public for AI targeting purposes
 *	4/27/05, Kit
 *		changed to if single mobile is targeted acts as a greater dispell(+25% percent chance of dispelling)
 *		if area is targeted acts per normal with area effect
 *	4/26/04, Adam
 *		Fixed dispelChance. Was backwards
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
*/

using System;
using System.Collections;
using Server.Misc;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Seventh
{
	public class MassDispelSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Mass Dispel", "Vas An Ort",
				SpellCircle.Seventh,
				263,
				9002,
				Reagent.Garlic,
				Reagent.MandrakeRoot,
				Reagent.BlackPearl,
				Reagent.SulfurousAsh
			);

		public MassDispelSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( object target )
		{
		
			if ( !Caster.CanSee( target ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
		
			bool singlemob = false;

			BaseCreature to;

			IPoint3D p = target as IPoint3D;
			if ( target == null )
				return;
			
			else if ( CheckSequence() )
			{
				SpellHelper.Turn( Caster, p);

				SpellHelper.GetSurfaceTop( ref p );

				ArrayList targets = new ArrayList();

				Map map = Caster.Map;
				
				if ( target is BaseCreature && (((BaseCreature)target).Summoned || target is Genie) && !((BaseCreature)target).IsAnimatedDead && Caster.CanBeHarmful( (Mobile)target, false ) )
				{
					to = (BaseCreature)p;
					singlemob = true;
					double dChance = (108 + ((100 * (Caster.Skills.Magery.Value - to.DispelDifficulty)) / (to.DispelFocus*2))) / 100;	
	
					if(dChance > 0.99) dChance = 0.99;
					//Console.WriteLine(dChance);

					if ( dChance > Utility.RandomDouble() )
					{
						Effects.SendLocationParticles( EffectItem.Create(to.Location, to.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
						Effects.PlaySound( to, to.Map, 0x201 );
						to.Delete();
					}
					else
					{
						to.FixedEffect( 0x3779, 10, 20 );
						Caster.SendLocalizedMessage( 1010084 ); // The creature resisted the attempt to dispel it!
						Caster.DoHarmful( to );					// and now he's pissed at you
					}
					FinishSequence();
				}
			

			
				if ( map != null && singlemob == false)
				{
					IPooledEnumerable eable = map.GetMobilesInRange( new Point3D( p ), 8 );

					foreach ( Mobile m in eable )
					{
						if ( m is BaseCreature && ((BaseCreature)m).Summoned && !((BaseCreature)m).IsAnimatedDead && Caster.CanBeHarmful( m, false ) )
							targets.Add( m );
					}

					eable.Free();
				}

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];

					BaseCreature bc = m as BaseCreature;

					if ( bc == null )
						continue;

					double dispelChance = (50.0 + ((100 * (Caster.Skills.Magery.Value - bc.DispelDifficulty)) / (bc.DispelFocus*2))) / 100;

					if ( dispelChance > Utility.RandomDouble() )
					{
						Effects.SendLocationParticles( EffectItem.Create( m.Location, m.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
						Effects.PlaySound( m, m.Map, 0x201 );
						m.Delete();
					}
					else
					{
						m.FixedEffect( 0x3779, 10, 20 );
						Caster.SendLocalizedMessage( 1010084 ); // The creature resisted the attempt to dispel it!
						Caster.DoHarmful( m );					// and now he's pissed at you
					}
				}
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private MassDispelSpell m_Owner;

			public InternalTarget( MassDispelSpell owner ) : base( 12, true, TargetFlags.Harmful )
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