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

/* /Scripts/Mobiles/Guards/PatrolGuard.cs
 * created 6/10/04 by mith
 *	These are guards designed for patroling banks and other town areas without going *poof*
 * 
 * Changelog
 * 04/19/05, Kit
 *		Added check to onmovement not to attack player initially if they are hidden.
 *		updated bank closeing code to not have guard change direction of player(was causeing them to be paralyzed) 
 * 9/30/04, Pigpen
 * 		Fixed an issues where this guard would try to chase a hidden >player char if that char has more that 5 counts. Spamming Reveal etc.
 * 8/7/04, Old Salty
 * 		Patrol Guards now check to see if the region is guarded before attacking reds on sight.
 * 7/26/04, Old Salty
 * 		Added a few lines (97-100) to make the criminal turn, closing the bankbox, when the guard attacks.
 * 6/22/04, Old Salty
 * 		PatrolGuards now deal extra damage to NPC's like their *poof*guard counterparts.
 * 		PatrolGuards now respond more specifically to speech.
 * 6/21/04, Old Salty
 * 		Modified the search/reveal code so that guards can reveal players.
 */
	
using System;
using System.Collections;
using Server.Misc;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Regions;
using Server.Network;

namespace Server.Mobiles
{
	public class PatrolGuard : BaseGuard
	{
		private Timer m_AttackTimer;
		//private NetState m_NetState;
		private Mobile m_Focus;

		[Constructable]
		public PatrolGuard() : this( null )
		{
		}

		public PatrolGuard( Mobile target ) : base( target )
		{
		}

		public PatrolGuard( Serial serial ) : base( serial )
		{
		}
		
		public override void AlterMeleeDamageTo( Mobile to, ref int damage )
		{
			if ( !to.Player )
				damage = (int)(to.HitsMax * .6);
		}
		
		public override bool OnBeforeDeath()
		{
			if ( m_Focus != null && m_Focus.Alive )
				new AvengeTimer( m_Focus ).Start(); // If a guard dies, three more guards will spawn

			return base.OnBeforeDeath();
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public override Mobile Focus
		{
			get
			{
				return m_Focus;
			}
			set
			{
				if ( Deleted )
					return;

				Mobile oldFocus = m_Focus;

				if ( oldFocus != value )
				{
					m_Focus = value;
					
					if ( value != null )
					{
						this.AggressiveAction( value );
						if (value.BankBox != null )
						value.BankBox.Close();
						value.Send( new MobileUpdate(value) ); //send a update packet to let client know BB is closed.
					}

					Combatant = value;

					if ( oldFocus != null && !oldFocus.Alive )
						Say( "Thou hast suffered thy punishment, scoundrel." );

					if ( value != null )
						Say( 500131 ); // Thou wilt regret thine actions, swine!

					if ( m_AttackTimer != null )
					{
						m_AttackTimer.Stop();
						m_AttackTimer = null;
					}

					if ( m_Focus != null )
					{
						m_AttackTimer = new AttackTimer( this );
						m_AttackTimer.Start();
						((AttackTimer)m_AttackTimer).DoOnTick();
					}
				}
			}
		}

		public override void OnSpeech( SpeechEventArgs e )
		{
			if ( e.Mobile is PlayerMobile && e.Mobile.Alive)
			{				
				if ( e.Speech.ToLower() == this.Name.ToLower() )
					this.Say( "Did you want to talk to me?" );
				
				if ( e.HasKeyword( 0x003b ) ) // *hi*
					this.Say( "Hello to thee, {0}.", e.Mobile.Name );
				
				if ( e.HasKeyword( 0x00fa ) ) // *bye*
					this.Say( "Fare thee well, {0}.", e.Mobile.Name );
				
				if ( e.HasKeyword( 0x0016 ) ) // *help* 
				{
					switch( Utility.Random( 5 ) )
					{
						case 0:
							this.Say( "I would assist thee, but I am tired." ); break;
						case 1:
							this.Say( "Alas, I cannot help thee, I am on my break." ); break;
						case 2:
							this.Say( "No help for thee today." ); break;
						case 3:
							this.Say( "We shall protect thee." ); break;
						case 4:
							this.Say( "What is my help worth to thee?" ); break;
					}
				}
			} 


		}

		public override void OnMovement( Mobile m, Point3D oldLocation  )
		{
			GuardedRegion reg = m.Region as GuardedRegion;
			if ( m.Player && m.AccessLevel == AccessLevel.Player && m.Alive && ( m.Kills >= 5 ) && reg != null && reg.IsGuarded && !m.Hidden) //Old Salty - removed a check for m.Criminal here
				Focus = m;

			base.OnMovement( m, oldLocation );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Focus );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Focus = reader.ReadMobile();

					if ( m_Focus != null )
					{
						m_AttackTimer = new AttackTimer( this );
						m_AttackTimer.Start();
					}

					break;
				}
			}
		}

		public override void OnAfterDelete()
		{
			if ( m_AttackTimer != null )
			{
				m_AttackTimer.Stop();
				m_AttackTimer = null;
			}

			base.OnAfterDelete();
		}

		private class AvengeTimer : Timer
		{
			private Mobile m_Focus;

			public AvengeTimer( Mobile focus ) : base( TimeSpan.FromSeconds( 2.5 ), TimeSpan.FromSeconds( 1.0 ), 3 )
			{
				m_Focus = focus;
			}

			protected override void OnTick()
			{
				BaseGuard.Spawn( m_Focus, m_Focus, 1, true );
			}
		}

		private class AttackTimer : Timer
		{
			private PatrolGuard m_Owner;

			public AttackTimer( PatrolGuard owner ) : base( TimeSpan.FromSeconds( 0.25 ), TimeSpan.FromSeconds( 0.1 ) )
			{
				m_Owner = owner;
			}

			public void DoOnTick()
			{
				OnTick();
			}

			protected override void OnTick()
			{
				if ( m_Owner.Deleted )
				{
					Stop();
					return;
				}

				m_Owner.Criminal = false;
				m_Owner.Kills = 0;
				m_Owner.Stam = m_Owner.StamMax;

				Mobile target = m_Owner.Focus;

				if ( target != null && (target.Deleted || !target.Alive || !m_Owner.CanBeHarmful( target )) )	
				{
					m_Owner.Focus = null;
					Stop();
					return;
				}
				else if ( m_Owner.Weapon is Fists )
				{
					m_Owner.Kill();
					Stop();
					return;
				}

				if ( target != null && m_Owner.Combatant != target )
					m_Owner.Combatant = target;

				if ( target == null )
				{
					Stop();
				}
				/* else
				{// <instakill>
					TeleportTo( target );
					target.BoltEffect( 0 );

					if ( target is BaseCreature )
						((BaseCreature)target).NoKillAwards = true;

					target.Damage( target.HitsMax, m_Owner );
					target.Kill(); // just in case, maybe Damage is overriden on some shard

					if ( target.Corpse != null && !target.Player )
						target.Corpse.Delete();

					m_Owner.Focus = null;
					Stop();
				}// </instakill> */
				else if ( !m_Owner.InRange( target, 20 ) )
				{
					m_Owner.Focus = null;
				}
				else if ( !m_Owner.InRange( target, 10 ) || !m_Owner.InLOS( target ) )
				{
					TeleportTo( target );
				}
				else if ( !m_Owner.InRange( target, 1 ) )
				{
					if ( !m_Owner.Move( m_Owner.GetDirectionTo( target ) | Direction.Running ) )
						TeleportTo( target );
				}
				else if ( !m_Owner.CanSee( target ) && Utility.Random( 50 ) == 0)
				{
					if ( Utility.Random( 10 ) == 0 )
					{
						target.RevealingAction();
						m_Owner.Say( "Ah, I have found you!" );
					}
					else
						m_Owner.Say( "Reveal!" );
				}
			}

			private void TeleportTo( Mobile target )
			{
				Point3D from = m_Owner.Location;
				Point3D to = target.Location;

				m_Owner.Location = to;

				Effects.SendLocationParticles( EffectItem.Create( from, m_Owner.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
				Effects.SendLocationParticles( EffectItem.Create(   to, m_Owner.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 5023 );

				m_Owner.PlaySound( 0x1FE );
			}
		}
	}
}
