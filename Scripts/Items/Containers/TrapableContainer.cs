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

/* Scripts/Items/Containers/TrapableContainer.cs
 * CHANGELOG:
 *	2/2/07, Pix
 *		Changed animations for explosion and poison traps to be at acceptable z levels.
 *	3/4/06, Pix
 *		Now staff never trip traps.
 *	5/9/05, Adam
 *		Push the Deco flag down to the Container level
 *		Pack old property from serialization routines.
 *	10/30/04, Darva
 *		Fixed CanDetonate
 *	10/25/04, Pix
 *		Reversed the change to m_Enabled made on 10/19.
 *		Also, now serialize/deserialize the m_Enabled flag.
 *	10/23/04, Darva
 *			Added CanDetonate check, which currently stops the trap from going
 *			off if it's on a vendor.
 *    10/19/04, Darva
 *			Set m_Enabled to false after trap goes off.
 *	9/25/04, Adam
 *		Create Version 3 of TrapableContainers that support the Deco attribute.
 *			Most/many containers are derived from TrapableContainer, so they will all get 
 *			the benefits for free.
 *	9/1/04, Pixie
 *		Added TinkerTrapableAttribute so we can mark containers as tinkertrapable or not.
 *  8/8/04, Pixie
 *		Added functionality for tripping the trap if you fail to disarm it.
 *	5/22/2004, Pixie
 *		made tinker traps one-use only
 *	5/22/2004, Pixie
 *		Tweaked poison trap levels up (now GM tinkers always make lethal poison traps)
 *		Changed so tinker-made traps don't get disabled when they're tripped.
 *		Changed sound effects to the right ones for dart/poison traps
 *  5/18/2004, Pixie
 *		Fixed re-enabling of tinker traps, added values to
 *		serialize/deserialize
 *	5/18/2004, Pixie
 *		Added Handling of tinker traps, added dart and poison traps
 */

using System;
using Server.Mobiles;

namespace Server.Items
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class TinkerTrapableAttribute : System.Attribute 
	{				
		public TinkerTrapableAttribute() 
		{		
		}
	}

	public enum TrapType
	{
		None,
		MagicTrap,
		ExplosionTrap,
		DartTrap,
		PoisonTrap
	}

	public abstract class TrapableContainer : BaseContainer, ITelekinesisable
	{
		private TrapType m_TrapType;
		private int m_TrapPower;
		private bool m_Enabled;
		private bool m_TinkerMade;
		private TrapType m_LastTrapType;
		
		[CommandProperty( AccessLevel.GameMaster )]
		public TrapType TrapType
		{
			get
			{
				return m_TrapType;
			}
			set
			{
				m_TrapType = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int TrapPower
		{
			get
			{
				return m_TrapPower;
			}
			set
			{
				m_TrapPower = value;
			}
		}

		public TrapableContainer( int itemID ) : base( itemID )
		{
			m_Enabled = true;
			m_TinkerMade = false;
		}

		public TrapableContainer( Serial serial ) : base( serial )
		{
			m_Enabled = true;
			m_TinkerMade = false;
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool TrapEnabled
		{
			get
			{ 
				return m_Enabled; 
			}
			set
			{ 
				//if( m_LastTrapType != TrapType.None )
				//{
				//	m_TrapType = m_LastTrapType;
				//}
				m_Enabled = value; 
			}
		}
		
		[CommandProperty( AccessLevel.GameMaster )]
		public bool TinkerMadeTrap
		{
			get{ return m_TinkerMade; }
			set{ m_TinkerMade = value; }
		}
		private bool CanDetonate (Mobile from)
		{
			//Added to check if trap is allowed to detonate.
			// from is not used currently, but might be necessary
			// in the future.
			object oParent;
			oParent = this.Parent;
			while (oParent != null)
			{
				if (oParent is PlayerVendor)
					return false; 
				if (oParent is BaseContainer)
					oParent = ((BaseContainer)oParent).Parent;
				else
					return true;
			}
			return true;

		}
		public virtual bool ExecuteTrap( Mobile from )
		{
			//Pix: have staff be able to open container w/o tripping trap
			if( from.AccessLevel > AccessLevel.Player ) return false;

			if ( m_TrapType != TrapType.None && m_Enabled && CanDetonate(from) )
			{
				m_LastTrapType = m_TrapType;
				switch ( m_TrapType )
				{
					case TrapType.ExplosionTrap:
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							AOS.Damage( from, m_TrapPower, 0, 100, 0, 0, 0 );
							from.SendLocalizedMessage( 503000 ); // Your skin blisters from the heat!
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x307 );
						//Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x36BD, 15 );
						Effects.SendLocationEffect(loc, Map, 0x36BD, 15, 10);

						break;
					}
					case TrapType.MagicTrap:
					{
						if ( from.InRange( GetWorldLocation(), 1 ) )
							AOS.Damage( from, m_TrapPower, 0, 100, 0, 0, 0 );

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x307 );

						Effects.SendLocationEffect( new Point3D( loc.X - 1, loc.Y, loc.Z ), Map, 0x36BD, 15 );
						Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y, loc.Z ), Map, 0x36BD, 15 );

						Effects.SendLocationEffect( new Point3D( loc.X, loc.Y - 1, loc.Z ), Map, 0x36BD, 15 );
						Effects.SendLocationEffect( new Point3D( loc.X, loc.Y + 1, loc.Z ), Map, 0x36BD, 15 );

						Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z + 11 ), Map, 0x36BD, 15 );

						break;
					}
					case TrapType.DartTrap:
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							AOS.Damage( from, m_TrapPower/2, 100, 0, 0, 0, 0 );
							from.SendLocalizedMessage( 502380 ); // A dart embeds...
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x223 );
						//What effect?!?
						//Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x36BD, 15 );
						break;
					}
					case TrapType.PoisonTrap:
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							Poison p = Poison.Lesser;
							if( m_TrapPower >= 30 )
								p = Poison.Regular;
							if( m_TrapPower >= 60 )
								p = Poison.Greater;
							if( m_TrapPower >= 90 )
								p = Poison.Deadly;
							if( m_TrapPower >= 100 )
								p = Poison.Lethal;

							from.ApplyPoison( from, p );
							from.SendLocalizedMessage( 503004 ); // You are enveloped...
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x231 );
						//Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x11A6, 20 ); 
						Effects.SendLocationEffect(new Point3D(loc.X + 1, loc.Y + 1, loc.Z + 1), Map, 0x113A, 10, 20); 
						//Effects.SendLocationEffect(loc, Map, 0x113A, 10, 20);
						break;
					}
				}

				m_TrapType = TrapType.None;
				return true;
			}

			return false;
		}

		//Virtual function so that we can have child classes
		// implement different things based on how that class
		// wants to behave.
		// returns false if nothing happens, otherwise return true
		public virtual bool OnFailDisarm( Mobile from )
		{
			//base class does nothing
			return false;
		}

		public virtual void OnTelekinesis( Mobile from )
		{
			Effects.SendLocationParticles( EffectItem.Create( Location, Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 5022 );
			Effects.PlaySound( Location, Map, 0x1F5 );

			if ( !ExecuteTrap( from ) )
				base.DisplayTo( from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.AccessLevel > AccessLevel.Player || from.InRange( this.GetWorldLocation(), 2 ) )
			{
				if ( !ExecuteTrap( from ) )
					base.OnDoubleClick( from );
			}
			else
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 5 ); // version

			writer.Write( (bool) m_Enabled );
			//writer.Write( (bool) false );	// removed in version 5 
			writer.Write( (bool) m_TinkerMade );
			writer.Write( (int) m_LastTrapType );
			writer.Write( (int) m_TrapPower );
			writer.Write( (int) m_TrapType );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 5:
				{
					// removed the deco field (case 3)
					goto case 4;
				}
				case 4:
				{
					m_Enabled = reader.ReadBool();

					goto case 3;
				}
				case 3:
				{	// obsolete deco field
					bool dummy;
					if (version < 5)
						dummy = reader.ReadBool();
					goto case 2;
				}
				case 2:
				{
					m_TinkerMade = reader.ReadBool();
					m_LastTrapType = (TrapType)reader.ReadInt();

					goto case 1;
				}
				case 1:
				{
					m_TrapPower = reader.ReadInt();

					goto case 0;
				}

				case 0:
				{
					m_TrapType = (TrapType)reader.ReadInt();

					break;
				}
			}
		}
	}
}