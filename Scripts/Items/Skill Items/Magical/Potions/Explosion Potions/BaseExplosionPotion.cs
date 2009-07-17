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

/* /Scripts/Items/SkillItems/Magical/Potions/Explosion Potions/BaseExplosionPotion.cs
 * ChangeLog:
 *  05/01/06 Taran Kain
 *		Fixed that goddamn z-axis problem. Targets must be within (Range * 8) z-units of the explosion to be affected.
 *	4/4/06, weaver
 *		Added parameter to CanBeHarmful() check to allow harm after the 
 *		death of the potion thrower.
 *	6/3/05, Adam
 *		Add in ExplosionPotionThreshold to control the tossers 
 *		health requirement
 *	5/4/05, Adam
 *		Make the targeting based on your stam and HP.
 *		If you have taken any damage, either in HP or stamina, you will not be able to toss
 *		a heat seeking explosion potion, and the potion will resolve to the targeted X,Y instead.
 *	4/27/05, erlein
 *		Added a check on target location to see if mobile can be spawned there to
 *		resolve "through the wall" problem.
 *	4/26/05, Pix
 *		Made explode pots targetting method toggleable based on CoreAI/ConsoleManagement setting.
 *	4/24/05, Pix
 *		Change to how targeting works.
 *	4/18/05, Pix
 *		Now resets swing timer on use and on target.
 *	8/26/04, Pix
 *		Added additional random 0-2 second delay on explosion potions.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Network;
using Server.Targeting;
using Server.Spells;

namespace Server.Items
{
	public abstract class BaseExplosionPotion : BasePotion
	{
		public abstract int MinDamage { get; }
		public abstract int MaxDamage { get; }

		public override bool RequireFreeHand{ get{ return false; } }

		private static bool LeveledExplosion = false; // Should explosion potions explode other nearby potions?
		private static bool InstantExplosion = false; // Should explosion potions explode on impact?
		private const int   ExplosionRange   = 2;     // How long is the blast radius?

		public BaseExplosionPotion( PotionEffect effect ) : base( 0xF0D, effect )
		{
		}

		public BaseExplosionPotion( Serial serial ) : base( serial )
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

		public virtual object FindParent( Mobile from )
		{
			Mobile m = this.HeldBy;

			if ( m != null && m.Holding == this )
				return m;

			object obj = this.RootParent;

			if ( obj != null )
				return obj;

			if ( Map == Map.Internal )
				return from;

			return this;
		}

		private Timer m_Timer;

		private ArrayList m_Users;

		public override void Drink( Mobile from )
		{
			if ( Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)) )
			{
				from.SendLocalizedMessage( 1062725 ); // You can not use a purple potion while paralyzed.
				return;
			}

			//reset from's swingtimer
			BaseWeapon weapon = from.Weapon as BaseWeapon;
			if ( weapon != null )
			{
				from.NextCombatTime = DateTime.Now + weapon.GetDelay( from );
			}


			ThrowTarget targ = from.Target as ThrowTarget;

			if ( targ != null && targ.Potion == this )
				return;

			from.RevealingAction();

			if ( m_Users == null )
				m_Users = new ArrayList();

			if ( !m_Users.Contains( from ) )
				m_Users.Add( from );

			from.Target = new ThrowTarget( this );

			if ( m_Timer == null )
			{
				from.SendLocalizedMessage( 500236 ); // You should throw it now!
				int numberoftics = 3; //minimum
				numberoftics += Utility.Random(0, 3); // add 0,1,or 2 tics to make the total time 3-5 seconds
				m_Timer = Timer.DelayCall( TimeSpan.FromSeconds( 0.75 ), TimeSpan.FromSeconds( 1.0 ), numberoftics+1, new TimerStateCallback( Detonate_OnTick ), new object[]{ from, numberoftics } );
			}
		}

		private void Detonate_OnTick( object state )
		{
			if ( Deleted )
				return;

			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];
			int timer = (int)states[1];

			object parent = FindParent( from );

			if ( timer == 0 )
			{
				Point3D loc;
				Map map;

				if ( parent is Item )
				{
					Item item = (Item)parent;

					loc = item.GetWorldLocation();
					map = item.Map;
				}
				else if ( parent is Mobile )
				{
					Mobile m = (Mobile)parent;

					loc = m.Location;
					map = m.Map;
				}
				else
				{
					return;
				}

				Explode( from, true, loc, map );
			}
			else
			{
				if ( parent is Item )
				{
					((Item)parent).PublicOverheadMessage( MessageType.Regular, 0x22, false, timer.ToString() );
				}
				else if ( parent is Mobile )
				{
					((Mobile)parent).PublicOverheadMessage( MessageType.Regular, 0x22, false, timer.ToString() );
				}

				states[1] = timer - 1;
			}
		}

		private void Reposition_OnTick( object state )
		{
			if ( Deleted )
				return;

			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];
			IPoint3D p = (IPoint3D)states[1];
			Map map = (Map)states[2];

			Point3D loc = new Point3D( p );

			if ( InstantExplosion )
				Explode( from, true, loc, map );
			else
				MoveToWorld( loc, map );
		}

		static public bool CheckHealth(int current, int max, double percent)
		{
			return ((double)current) >= (((double)max) * percent);
		}

		private class ThrowTarget : Target
		{
			private BaseExplosionPotion m_Potion;

			public BaseExplosionPotion Potion
			{
				get{ return m_Potion; }
			}

			public ThrowTarget( BaseExplosionPotion potion ) : base( 12, true, TargetFlags.None )
			{
				m_Potion = potion;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				//reset from's swingtimer
				BaseWeapon weapon = from.Weapon as BaseWeapon;
				if ( weapon != null )
				{
					from.NextCombatTime = DateTime.Now + weapon.GetDelay( from );
				}


				if ( m_Potion.Deleted || m_Potion.Map == Map.Internal )
					return;

				IPoint3D p = targeted as IPoint3D;

				if ( p == null )
					return;

				Map map = from.Map;

				if ( map == null )
					return;

				SpellHelper.GetSurfaceTop( ref p );

				// erl: 04/27/05, spawn mobile check at target location
				if(!map.CanSpawnMobile( p.X, p.Y, p.Z ) && !(p is Mobile))
				{
					from.SendLocalizedMessage( 501942 );
					return;
				}

				from.RevealingAction();

				IEntity to;
				bool bMobile = CoreAI.ExplosionPotionTargetMethod == CoreAI.EPTM.MobileBased;

				// Adam: You must be in top condition to target the player directly
				//if( bMobile && (from.Stam >= from.StamMax) && (from.Hits >= from.HitsMax) )
				if( bMobile && CheckHealth(from.Stam, from.StamMax, CoreAI.ExplosionPotionThreshold) && CheckHealth(from.Hits, from.HitsMax, CoreAI.ExplosionPotionThreshold) )
				{ 
					if ( p is Mobile )
						to = (Mobile)p;
					else
						to = new Entity( Serial.Zero, new Point3D( p ), map );

					Effects.SendMovingEffect( from, to, m_Potion.ItemID & 0x3FFF, 7, 0, false, false, m_Potion.Hue, 0 );

					m_Potion.Internalize();

					Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( m_Potion.Reposition_OnTick ), new object[]{ from, p, map } );
				}
				else
				{
					to = new Entity( Serial.Zero, new Point3D( p ), map );

					Effects.SendMovingEffect( from, to, m_Potion.ItemID & 0x3FFF, 7, 0, false, false, m_Potion.Hue, 0 );

					m_Potion.Internalize();

					Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( m_Potion.Reposition_OnTick ), new object[]{ from, to, map } );
				}
			}
		}

		public void Explode( Mobile from, bool direct, Point3D loc, Map map )
		{
			if ( Deleted )
				return;

			Delete();

			for ( int i = 0; m_Users != null && i < m_Users.Count; ++i )
			{
				Mobile m = (Mobile)m_Users[i];
				ThrowTarget targ = m.Target as ThrowTarget;

				if ( targ != null && targ.Potion == this )
					Target.Cancel( m );
			}

			if ( map == null )
				return;

			Effects.PlaySound( loc, map, 0x207 );
			Effects.SendLocationEffect( loc, map, 0x36BD, 20 );

			int alchemyBonus = 0;

			if ( direct )
				alchemyBonus = (int)(from.Skills.Alchemy.Value / (Core.AOS ? 5 : 10));

			IPooledEnumerable eable = LeveledExplosion ? map.GetObjectsInRange( loc, ExplosionRange ) : map.GetMobilesInRange( loc, ExplosionRange );
			ArrayList toExplode = new ArrayList();

			int toDamage = 0;

			foreach ( object o in eable )
			{
				if (o is IPoint3D)
				{
					IPoint3D i = o as IPoint3D;
					if (Math.Abs(i.Z - this.Z) > ExplosionRange * 8)
						continue;
				}

				if ( o is Mobile )
				{
					toExplode.Add( o );
					++toDamage;
				}
				else if ( o is BaseExplosionPotion && o != this )
				{
					toExplode.Add( o );
				}
			}

			eable.Free();

			int min = Scale( from, MinDamage );
			int max = Scale( from, MaxDamage );

			for ( int i = 0; i < toExplode.Count; ++i )
			{
				object o = toExplode[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					// wea: added parameter to CanBeHarmful() check to allow harm after the 
					// death of the potion thrower

					if(	from == null || (SpellHelper.ValidIndirectTarget( from, m ) && from.CanBeHarmful( m, false, false, true ))	)
					{
						if ( from != null )
							from.DoHarmful( m );

						int damage = Utility.RandomMinMax( min, max );

						damage += alchemyBonus;

						if ( !Core.AOS && damage > 40 )
							damage = 40;
						else if ( Core.AOS && toDamage > 2 )
							damage /= toDamage - 1;

						AOS.Damage( m, from, damage, 0, 100, 0, 0, 0 );
					}
				}
				else if ( o is BaseExplosionPotion )
				{
					BaseExplosionPotion pot = (BaseExplosionPotion)o;

					pot.Explode( from, false, pot.GetWorldLocation(), pot.Map );
				}
			}
		}
	}
}
