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

using System;
using Server.Targeting;
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0xF52, 0xF51 )]
	public class ThrowingDagger : Item
	{
		[Constructable]
		public ThrowingDagger() : base( 0xF52 )
		{
			Weight = 1.0;
			Layer = Layer.OneHanded;
			Name = "a throwing dagger";
		}

		public ThrowingDagger( Serial serial ) : base( serial )
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

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.Items.Contains( this ) )
			{
				InternalTarget t = new InternalTarget( this );
				from.Target = t;
			}
			else
			{
				from.SendMessage( "You must be holding that weapon to use it." );
			}
		}

		private class InternalTarget : Target
		{
			private ThrowingDagger m_Dagger;

			public InternalTarget( ThrowingDagger dagger ) : base( 10, false, TargetFlags.Harmful )
			{
				m_Dagger = dagger;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Dagger.Deleted )
				{
					return;
				}
				else if ( !from.Items.Contains( m_Dagger ) )
				{
					from.SendMessage( "You must be holding that weapon to use it." );
				}
				else if ( targeted is Mobile )
				{
					Mobile m = (Mobile)targeted;

					if ( m != from && from.HarmfulCheck( m ) )
					{
						Direction to = from.GetDirectionTo( m );

						from.Direction = to;

						from.Animate( from.Mounted ? 26 : 9, 7, 1, true, false, 0 );

						if ( Utility.RandomDouble() >= (Math.Sqrt( m.Dex / 100.0 ) * 0.8) )
						{
							from.MovingEffect( m, 0x1BFE, 7, 1, false, false, 0x481, 0 );

							AOS.Damage( m, from, Utility.Random( 5, from.Str / 10 ), 100, 0, 0, 0, 0 );

							m_Dagger.MoveToWorld( m.Location, m.Map );
						}
						else
						{
							int x = 0, y = 0;

							switch ( to & Direction.Mask )
							{
								case Direction.North: --y; break;
								case Direction.South: ++y; break;
								case Direction.West: --x; break;
								case Direction.East: ++x; break;
								case Direction.Up: --x; --y; break;
								case Direction.Down: ++x; ++y; break;
								case Direction.Left: --x; ++y; break;
								case Direction.Right: ++x; --y; break;
							}

							x += Utility.Random( -1, 3 );
							y += Utility.Random( -1, 3 );

							x += m.X;
							y += m.Y;

							m_Dagger.MoveToWorld( new Point3D( x, y, m.Z ), m.Map );

							from.MovingEffect( m_Dagger, 0x1BFE, 7, 1, false, false, 0x481, 0 );

							from.SendMessage( "You miss." );
						}
					}
				}
			}
		}
	}
} 
