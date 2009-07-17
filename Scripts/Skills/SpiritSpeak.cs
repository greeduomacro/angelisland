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

/* Scripts\Skills\SpiritSpeak.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	8/31/07, Adam
 *		Change CheckSkill() to check against a max skill of 100 instead of 120
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Items;
using Server.Spells;
using Server.Network;

namespace Server.SkillHandlers
{
	class SpiritSpeak
	{
		public static void Initialize()
		{
			SkillInfo.Table[32].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( Core.AOS )
			{
				Spell spell = new SpiritSpeakSpell( m );

				spell.Cast();

				if ( spell.IsCasting )
					return TimeSpan.FromSeconds( 5.0 );

				return TimeSpan.Zero;
			}

			m.RevealingAction();

			if ( m.CheckSkill( SkillName.SpiritSpeak, 0, 100 ) )
			{	
				if ( !m.CanHearGhosts )
				{
					Timer t = new SpiritSpeakTimer( m );
					double secs = m.Skills[SkillName.SpiritSpeak].Base / 50;
					secs *= 90;
					if ( secs < 15 )
						secs = 15;

					t.Delay = TimeSpan.FromSeconds( secs );//15seconds to 3 minutes
					t.Start();
					m.CanHearGhosts = true;
				}

				m.PlaySound( 0x24A );
				m.SendLocalizedMessage( 502444 );//You contact the neitherworld.
			}
			else
			{
				m.SendLocalizedMessage( 502443 );//You fail to contact the neitherworld.
				m.CanHearGhosts = false;
			}

			return TimeSpan.FromSeconds( 1.0 );
		}

		private class SpiritSpeakTimer : Timer
		{
			private Mobile m_Owner;
			public SpiritSpeakTimer( Mobile m ) : base( TimeSpan.FromMinutes( 2.0 ) )
			{
				m_Owner = m;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				m_Owner.CanHearGhosts = false;
				m_Owner.SendLocalizedMessage( 502445 );//You feel your contact with the neitherworld fading.
			}
		}

		private class SpiritSpeakSpell : Spell
		{
			private static SpellInfo m_Info = new SpellInfo( "Spirit Speak", "", SpellCircle.Second, 269 );

			public override bool BlockedByHorrificBeast{ get{ return false; } }

			public SpiritSpeakSpell( Mobile caster ) : base( caster, null, m_Info )
			{
			}

			public override bool ClearHandsOnCast{ get{ return false; } }

			public override TimeSpan GetCastDelay()
			{
				return TimeSpan.FromSeconds( 1.0 );
			}

			public override int GetMana()
			{
				return 0;
			}

			public override bool ConsumeReagents()
			{
				return true;
			}

			public override bool CheckFizzle()
			{
				return true;
			}

			public override bool CheckNextSpellTime{ get{ return false; } }

			public override void OnDisturb( DisturbType type, bool message )
			{
				Caster.NextSkillTime = DateTime.Now;

				base.OnDisturb( type, message );
			}

			public override bool CheckDisturb( DisturbType type, bool checkFirst, bool resistable )
			{
				if ( type == DisturbType.EquipRequest || type == DisturbType.UseRequest )
					return false;

				return true;
			}

			public override void SayMantra()
			{
				// Anh Mi Sah Ko
				Caster.PublicOverheadMessage( MessageType.Regular, 0x3B2, 1062074, "", false );
				Caster.PlaySound( 0x24A );
			}

			public override void OnCast()
			{
				Corpse toChannel = null;

				IPooledEnumerable eable = Caster.GetItemsInRange( 3 );
				foreach ( Item item in eable)
				{
					if ( item is Corpse && !((Corpse)item).Channeled )
					{
						toChannel = (Corpse)item;
						break;
					}
				}
				eable.Free();

				int max, min, mana, number;

				if ( toChannel != null )
				{
					min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.25);
					max = min + 4;
					mana = 0;
					number = 1061287; // You channel energy from a nearby corpse to heal your wounds.
				}
				else
				{
					min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.25);
					max = min + 4;
					mana = 10;
					number = 1061286; // You channel your own spiritual energy to heal your wounds.
				}

				if ( Caster.Mana < mana )
				{
					Caster.SendLocalizedMessage( 1061285 ); // You lack the mana required to use this skill.
				}
				else
				{
					Caster.CheckSkill( SkillName.SpiritSpeak, 0.0, 100.0 );

					if ( Utility.RandomDouble() > (Caster.Skills[SkillName.SpiritSpeak].Value / 100.0) )
					{
						Caster.SendLocalizedMessage( 502443 ); // You fail your attempt at contacting the netherworld.
					}
					else
					{
						if ( toChannel != null )
						{
							toChannel.Channeled = true;
							toChannel.Hue = 0x835;
						}

						Caster.Mana -= mana;
						Caster.SendLocalizedMessage( number );

						if ( min > max )
							min = max;

						Caster.Hits += Utility.RandomMinMax( min, max );

						Caster.FixedParticles( 0x375A, 1, 15, 9501, 2100, 4, EffectLayer.Waist );
					}
				}

				FinishSequence();
			}
		}
	}
}
