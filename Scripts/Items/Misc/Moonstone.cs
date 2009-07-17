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

/* Scripts\Items\Misc\Moonstone.cs
 * CHANGELOG
 *  7/6/08, Adam
 *      - fix CanFit logic to check the destination of the gate and not the source
 *      - fix auto naming to ignore empty region names
 *      - Log no friends marking a moonstone in a house
 *  09/13/05 Taran Kain
 *		Added an overload of Mark() to allow for copying from runes.
 *	3/6/05: Pix
 *		Added special checking.
 * 2/7/05	Darva,
 *		Fixed masked values for directions, allowing moonstone placement after
 *		running.
 * 1/26/05 Darva,
 *		Stone drops one step away from player, in the direction they're facing.
 * 1/25/05 Darva,
 *		Total rewrite, moonstones now can be marked, and create gates for players with no magery.
 */

using System;
using System.Collections;
using Server;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Multis;
using Server.Scripts.Commands;

namespace Server.Items
{
	public class Moonstone : Item
	{
		private Point3D m_Destination;
		private String m_Description;
		private bool m_Marked;

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public bool Marked
		{
			get
			{
				return m_Marked;
			}
			set
			{
				m_Marked = value;
				InvalidateProperties();
			}
		}

		
		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public Point3D Destination
		{
			get
			{
				return m_Destination;
			}
			set
			{
				m_Destination = value;
				InvalidateProperties();
			}
		}
//		public override int LabelNumber{ get{ return 1041490 + (int)m_Type; } }

		[Constructable]
		public Moonstone( ) : base( 0xF8B )
		{
			Weight = 1.0;
			m_Description = "An unmarked moonstone";
			m_Marked = false;
		}

		public Moonstone( Serial serial ) : base( serial )
		{
		}

		public override void OnSingleClick( Mobile from )
		{
				LabelTo(from, m_Description);
		//	base.OnSingleClick( from );
		}

        // log non frineds marking in a house
        public void LogMark(Mobile m)
        {
            // log non frineds marking in a house
            try
            {
                ArrayList regions = Region.FindAll(m.Location, m.Map);
                for (int ix = 0; ix < regions.Count; ix++)
                {
                    if (regions[ix] is Regions.HouseRegion == false)
                        continue;

                    Regions.HouseRegion hr = regions[ix] as Regions.HouseRegion;
                    BaseHouse bh = hr.House;

                    if (bh != null)
                    {
                        if (bh.IsFriend(m) == false)
                        {
                            LogHelper Logger = new LogHelper("mark.log", false, true);
                            Logger.Log(LogType.Mobile, m);
                            Logger.Log(LogType.Item, this);
                            Logger.Finish();
                        }
                    }
                }
            }
            catch (Exception ex) { LogHelper.LogException(ex); }
        }

		public void Mark(Mobile m)
		{
            // log non frineds marking in a house
            LogMark(m);

			Mark(m.Location, m.Region, m.Map);
		}
		public void Mark(Point3D loc, Region reg, Map map)
		{
			m_Destination = loc;
			m_Description = "a moonstone for {0}";
            if (reg != map.DefaultRegion && reg.Name != null && reg.Name.Length > 0)
				m_Description = string.Format(m_Description, reg);
			else
				m_Description = string.Format(m_Description, "an unknown location.");
			m_Marked = true;
			
		}
        
        private bool IsSpecial(Point3D location)
        {
            Region region = Server.Region.Find(location, Map.Felucca);
            if (region != null)
            {
                if (Server.Regions.Jail.IsInSpecial(region.Name))
                {
                    return true;
                }
            }
            return false;
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.Region is FeluccaDungeon)
			{
				from.SendMessage("Moonstones do not work in dungeons");
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( 
                IsSpecial(m_Destination) ||
                IsSpecial(from.Location) || 
                !SpellHelper.CheckTravel( from.Map, from.Location, TravelCheckType.GateFrom, from) || 
                !SpellHelper.CheckTravel( from.Map, m_Destination, TravelCheckType.GateTo, from) )
			{
				from.SendMessage("Something interferes with the moonstone.");
			}
			else if (m_Marked == false)
			{
				from.SendMessage( "That stone has not yet been marked.");
			}
			else if ( from.Mounted )
			{
				from.SendLocalizedMessage( 1005399 ); // You can not bury a stone while you sit on a mount.
			}
			else if ( !from.Body.IsHuman )
			{
				from.SendLocalizedMessage( 1005400 ); // You can not bury a stone in this form.
			}
			else if ( from.Criminal )
			{
				from.SendLocalizedMessage( 1005403 ); // The magic of the stone cannot be evoked by the lawless.
			}
            else if (from.Map.CanFit(m_Destination, 16) == false)
			{
				from.SendLocalizedMessage( 501942 ); // That location is blocked.
			}
			else
			{
				Movable = false;
				Point3D StoneLoc;
				StoneLoc = from.Location;
				int StoneDir = 0;
				StoneDir = (int)from.Direction;
				if (StoneDir >= 128)
					StoneDir = StoneDir - 128;
				switch ( StoneDir )
				{
					case 0:
						StoneLoc.Y--;
					break;
					case 1:
						StoneLoc.Y--;
						StoneLoc.X++;
					break;
					case 2:
						StoneLoc.X++;
					break;
					case 3:
						StoneLoc.Y++;
						StoneLoc.X++;
					break;
					case 4:
						StoneLoc.Y++;
					break;
					case 5:
						StoneLoc.Y++;
						StoneLoc.X--;
					break;
					case 6:  //West
						StoneLoc.X--;
					break;
					case 7: //Mask, UP?
						StoneLoc.X--;
						StoneLoc.Y--;
					break;
					default:  //Anything else, leave it alone.
					break;
				}
				MoveToWorld( StoneLoc, from.Map );

				from.Animate( 32, 5, 1, true, false, 0 );

				new SettleTimer( this, StoneLoc, from.Map, from, m_Destination ).Start();
			}
		}


		private class SettleTimer : Timer
		{
			private Item m_Stone;
			private Point3D m_Location;
			private Map m_Map;
			private Mobile m_Caster;
			private int m_Count;
			private Point3D m_Destination;

			public SettleTimer( Item stone, Point3D loc, Map map, Mobile caster, Point3D destination ) : base( TimeSpan.FromSeconds( 2.5 ), TimeSpan.FromSeconds( 1.0 ) )
			{
				m_Stone = stone;

				m_Location = loc;
				m_Map = map;
				m_Caster = caster;
				m_Destination = destination;
			}

			protected override void OnTick()
			{
				++m_Count;

				if ( m_Count == 1 )
				{
					m_Stone.PublicOverheadMessage( MessageType.Regular, 0x3B2, 1005414 ); // The stone settles into the ground.
				}
				else if ( m_Count >= 10 )
				{
					m_Stone.Location = new Point3D( m_Stone.X, m_Stone.Y, m_Stone.Z - 1 );

					if ( m_Count == 16 )
					{
						if ( !m_Map.CanFit( m_Location, 16) || !m_Map.CanFit (m_Destination, 16) )
						{
							m_Stone.Movable = true;
							m_Caster.AddToBackpack( m_Stone );
							Stop();
							return;
						}

						int hue = m_Stone.Hue;

						if ( hue == 0 )
							hue = Utility.RandomBirdHue();

						new MoonstoneGate( m_Location, m_Map, m_Caster, hue, m_Destination );
						new MoonstoneGate( m_Destination, m_Map, m_Caster, hue, m_Location );

						m_Stone.Delete();
						Stop();
					}
				}
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (Point3D) m_Destination );
			writer.Write( (string) m_Description );
			writer.Write( (bool) m_Marked );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					Delete();
					break;
				}
				case 1:
				{
					m_Destination = reader.ReadPoint3D();
					m_Description = reader.ReadString();
					m_Marked = reader.ReadBool();
					break;
				}
			}
		}
	}
}