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

/* Scripts/Spells/Base/Spell.cs
	ChangeLog:
	7/4/04, Pix
		Added caster to SpellHelper.Damage call so corpse wouldn't stay blue to this caster
	5/5/04 changes by smerX:
		scaled the lower end of damages
	3/25/04 changes by smerX:
		Added DamageDelay of 0.65 seconds
	3/18/04 code changes by smerX:
		Removed DamageDelay
		Amended damage calc to ("int damage = (highestStat - lowestStat) / 2")
			This produces normal p15 damage 
*/
using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Fifth
{
	public class MindBlastSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Mind Blast", "Por Corp Wis",
				SpellCircle.Fifth,
				218,
				Core.AOS ? 9002 : 9032,
				Reagent.BlackPearl,
				Reagent.MandrakeRoot,
				Reagent.Nightshade,
				Reagent.SulfurousAsh
			);

		public MindBlastSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
			if ( Core.AOS )
				m_Info.LeftHandEffect = m_Info.RightHandEffect = 9002;
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		private void AosDelay_Callback( object state )
		{
			object[] states = (object[])state;
			Mobile caster = (Mobile)states[0];
			Mobile target = (Mobile)states[1];
			Mobile defender = (Mobile)states[2];
			int damage = (int)states[3];

			if ( caster.HarmfulCheck( defender ) )
			{
			 	SpellHelper.Damage( TimeSpan.FromSeconds( 0.65 ), target, Utility.RandomMinMax( damage, damage + 4 ), 0, 0, 100, 0, 0 );

				target.FixedParticles( 0x374A, 10, 15, 5038, 1181, 2, EffectLayer.Head );
				target.PlaySound( 0x213 );
			}
		}

//		public override bool DelayedDamage{ get{ return !Core.AOS; } }
// DamageDelay was above ^ Commented out by smerX

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( Core.AOS )
			{
				if ( Caster.CanBeHarmful( m ) && CheckSequence() )
				{
					Mobile from = Caster, target = m;

					SpellHelper.Turn( from, target );

					SpellHelper.CheckReflect( (int)this.Circle, ref from, ref target );

					int damage = (int)((Caster.Skills[SkillName.Magery].Value + Caster.Int) / 5);

					if ( damage > 60 )
						damage = 60;

					Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), 
						new TimerStateCallback( AosDelay_Callback ),
						new object[]{ Caster, target, m, damage } );
				}
			}
			else if ( CheckHSequence( m ) )
			{
				Mobile from = Caster, target = m;

				SpellHelper.Turn( from, target );

				SpellHelper.CheckReflect( (int)this.Circle, ref from, ref target );

				// Algorithm: (highestStat - lowestStat) / 2 [- 50% if resisted]


				int rawHighStat = target.RawStr, rawLowStat = target.RawStr;
				int damageMod = 0;

				if ( target.RawDex > rawHighStat )
					rawHighStat = target.RawDex;

				if ( target.RawDex < rawLowStat )
					rawLowStat = target.RawDex;

				if ( target.RawInt > rawHighStat )
					rawHighStat = target.RawInt;

				if ( target.RawInt < rawLowStat )
					rawLowStat = target.RawInt;
				
				if ( rawHighStat > 150 )
					rawHighStat = 150;

				if ( rawLowStat > 150 ) 
					rawLowStat = 150;
					
				int rawFormula = (rawHighStat - rawLowStat) / 2;
				
				if ( rawFormula == 0 )
					damageMod = 1;
				else if ( rawFormula >= 38 )
					damageMod = 0;
				else if ( rawFormula >= 36 )
					damageMod = -5;
				else if ( rawFormula >= 33 )
					damageMod = -10;
				else if ( rawFormula >= 30 )
					damageMod = -15;
				else if ( rawFormula >= 28 )
					damageMod = -18;
				else
					damageMod = -20;


				int highestStat = target.Str, lowestStat = target.Str;

				if ( target.Dex > highestStat )
					highestStat = target.Dex;

				if ( target.Dex < lowestStat )
					lowestStat = target.Dex;

				if ( target.Int > highestStat )
					highestStat = target.Int;

				if ( target.Int < lowestStat )
					lowestStat = target.Int;

				if ( highestStat > 150 )
					highestStat = 150;

				if ( lowestStat > 150 ) 
					lowestStat = 150;

				int damage = ((highestStat - lowestStat) / 2) + damageMod;  // smerxifully scaled damage

				if ( damage > 45 )
					damage = 45;
				else if ( damage <= 0 )
					damage = 1;

				if ( CheckResisted( target ) )
				{
					damage /= 2;
					target.SendLocalizedMessage( 501783 ); // You feel yourself resisting magical energy.
				}

				from.FixedParticles( 0x374A, 10, 15, 2038, EffectLayer.Head );

				target.FixedParticles( 0x374A, 10, 15, 5038, EffectLayer.Head );
				target.PlaySound( 0x213 );

				//Pixie: 7/4/04: added caster to this so corpse wouldn't stay blue to this caster
				SpellHelper.Damage( TimeSpan.FromSeconds( 0.65 ), target, Caster, damage, 0, 0, 100, 0, 0 );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private MindBlastSpell m_Owner;

			public InternalTarget( MindBlastSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}