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

/* Scripts/Items/Maps/TreasureMap.cs
 * ChangeLog
 *	1/20/06, Adam
 *		Add new OnNPCBeginDig() function so that NPC's can dig treasure.
 *	5/07/05, Adam
 *		Remove hue and replace the the text 
 *			"for somewhere in Felucca" with "for somewhere in Ocllo"
 *		if the map is in Ocllo
 *	5/07/05, Kit
 *		Hued maps to faction orange if for withen ocllo
 *	4/17/05, Kitaras	
 *		Fixed bug regarding level 1 and 2 chests being set to themed
 *	4/14/04, Adam
 *		1. Put back lost change to treasure map drop rate
 *		2. Convert LootChance to a property, and attach it to CoreAI.TreasureMapDrop
 *	4/07/05, Kitaras
 *		Fixed static access variable issue
 *	4/03/05, Kitaras	
 *		Added check to spawn only one level 5 mob on themed chests
 *	3/30/05, Kitaras
 *		Redesigned system to use new TreasureThemes.cs control system for spawn generation.
 *	3/1/05, mith
 *		OnDoubleClick(): modified difficulty check so that if base Carto skill is less than midPoint, players gets failure message.
 *  12/05/04, Jade
 *      Reverted the chance to drop t-map back to 1%
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	4/29/04, mith
 *		Changed percentage chance for creature to drop t-map from 1% to 3.5%
 *
 *      8/30/04, Lego Eater
 *               changed the on singleclick so tmaps displayed properly (sorry for spelling)
 *
 *
 */

using System;
using System.IO;
using System.Collections;
using Server;
using Server.Engines;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Misc;

namespace Server.Items
{
	public class TreasureMap : MapItem
	{
		private int m_Level;
		private bool m_Completed;
		private Mobile m_Decoder;
		private Map m_Map;
		private Point2D m_Location;
		private bool m_Themed;
		private ChestThemeType m_type;
	
		[CommandProperty( AccessLevel.GameMaster )]
		public int Level{ get{ return m_Level; } set{ m_Level = value; InvalidateProperties(); } }
		
		//set theme type
		[CommandProperty( AccessLevel.GameMaster )]
		public ChestThemeType Theme{ get{ return m_type; } set{ m_type = value; InvalidateProperties(); } }

		//set if map is themed or not
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Themed{ get{ return m_Themed; } set{ m_Themed = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Completed{ get{ return m_Completed; } set{ m_Completed = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Decoder{ get{ return m_Decoder; } set{ m_Decoder = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Map ChestMap{ get{ return m_Map; } set{ m_Map = value; InvalidateProperties(); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Point2D ChestLocation{ get{ return m_Location; } set{ m_Location = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public static double LootChance{ get{ return CoreAI.TreasureMapDrop; }  }

		private static Point2D[] m_Locations;

		public static Point2D GetRandomLocation()
		{
			if ( m_Locations == null )
				LoadLocations();

			if ( m_Locations.Length > 0 )
				return m_Locations[Utility.Random( m_Locations.Length )];

			return Point2D.Zero;
		}
	
		
		private static void LoadLocations()
		{
			string filePath = Path.Combine( Core.BaseDirectory, "Data/treasure.cfg" );

			ArrayList list = new ArrayList();

			if ( File.Exists( filePath ) )
			{
				using ( StreamReader ip = new StreamReader( filePath ) )
				{
					string line;

					while ( (line = ip.ReadLine()) != null )
					{
						try
						{
							string[] split = line.Split( ' ' );

							int x = Convert.ToInt32( split[0] ), y = Convert.ToInt32( split[1] );

							list.Add( new Point2D( x, y ) );
						}
						catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
					}
				}
			}

			m_Locations = (Point2D[])list.ToArray( typeof( Point2D ) );
		}

		//old constructer [add treasuremap <level> <map>
		[Constructable]
		public TreasureMap( int level, Map map) : this(level, map, false, ChestThemeType.None )
		{
		}

		//new constructor [add treasuremap <level> <map> <theme>
		[Constructable]
		public TreasureMap( int level, Map map, ChestThemeType type) : this(level, map, true, type )
		{
		}

		[Constructable]
		public TreasureMap(int level, Map map, bool themed, ChestThemeType type) 
		{
			m_Level = level;
			m_Map = map;
			m_Location = GetRandomLocation();
			m_Themed = themed;
			m_type = type;

			Width = 300;
			Height = 300;

			int width = 600;
			int height = 600;

			int x1 = m_Location.X - Utility.RandomMinMax( width / 4, (width / 4) * 3 );
			int y1 = m_Location.Y - Utility.RandomMinMax( height / 4, (height / 4) * 3 );

			if ( x1 < 0 )
				x1 = 0;

			if ( y1 < 0 )
				y1 = 0;

			int x2 = x1 + width;
			int y2 = y1 + height;

			if ( x2 >= 5120 )
				x2 = 5119;

			if ( y2 >= 4096 )
				y2 = 4095;

			x1 = x2 - width;
			y1 = y2 - height;

			Bounds = new Rectangle2D( x1, y1, width, height );
			
			//if for ocllo hue map for pansy thunters :)
			//if( m_Location.X >= 3304 && m_Location.Y >= 2340 && m_Location.X < 3845 && m_Location.Y < 2977)
				//Hue = 0x90;
	
			Protected = true;

			AddWorldPin( m_Location.X, m_Location.Y );
		}
		
		
		public TreasureMap( Serial serial ) : base( serial )
		{
		}

		public void OnBeginDig( Mobile from )
		{
			if ( m_Completed )
			{
				from.SendLocalizedMessage( 503014 ); // This treasure hunt has already been completed.
			}
			else if ( from != m_Decoder )
			{
				from.SendLocalizedMessage( 503016 ); // Only the person who decoded this map may actually dig up the treasure.
			}
			else if ( !from.CanBeginAction( typeof( TreasureMap ) ) )
			{
				from.SendLocalizedMessage( 503020 ); // You are already digging treasure.
			}
			else
			{
				from.SendLocalizedMessage( 503033 ); // Where do you wish to dig?
				from.Target = new DigTarget( this );
			}
		}

		public void OnNPCBeginDig(Mobile from)
		{
			TreasureMap m_Map = this;
			Point2D loc = m_Map.m_Location;
			int x = loc.X, y = loc.Y;
			Map map = m_Map.m_Map;
			int z = map.GetAverageZ( x, y );

			if ( from.BeginAction( typeof( TreasureMap ) ) )
			{
				new DigTimer( from, m_Map, new Point3D( x, y, z - 14 ), map, z ,m_Map.m_type).Start();
			}
		}

		private class DigTarget : Target
		{
			private TreasureMap m_Map;

			public DigTarget( TreasureMap map ) : base( 6, true, TargetFlags.None )
			{
				m_Map = map;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Map.Deleted )
					return;

				if ( m_Map.m_Completed )
				{
					from.SendLocalizedMessage( 503014 ); // This treasure hunt has already been completed.
				}
				else if ( from != m_Map.m_Decoder )
				{
					from.SendLocalizedMessage( 503016 ); // Only the person who decoded this map may actually dig up the treasure.
				}
				else if ( !from.CanBeginAction( typeof( TreasureMap ) ) )
				{
					from.SendLocalizedMessage( 503020 ); // You are already digging treasure.
				}
				else
				{
					IPoint3D p = targeted as IPoint3D;

					if ( p is Item )
						p = ((Item)p).GetWorldLocation();

					int maxRange;
					double skillValue = from.Skills[SkillName.Mining].Value;

					if ( skillValue >= 100.0 )
						maxRange = 4;
					else if ( skillValue >= 81.0 )
						maxRange = 3;
					else if ( skillValue >= 51.0 )
						maxRange = 2;
					else
						maxRange = 1;

					Point2D loc = m_Map.m_Location;
					int x = loc.X, y = loc.Y;
					Map map = m_Map.m_Map;

					if ( map == from.Map && Utility.InRange( new Point3D( p ), new Point3D( loc, 0 ), maxRange ) )
					{
						if ( from.Location.X == loc.X && from.Location.Y == loc.Y )
						{
							from.SendLocalizedMessage( 503030 ); // The chest can't be dug up because you are standing on top of it.
						}
						else if ( map != null )
						{
							int z = map.GetAverageZ( x, y );

                            if (!map.CanFit(x, y, z, 16, CanFitFlags.checkBlocksFit | CanFitFlags.checkMobiles | CanFitFlags.requireSurface))
							{
								from.SendLocalizedMessage( 503021 ); // You have found the treasure chest but something is keeping it from being dug up.
							}
							else if ( from.BeginAction( typeof( TreasureMap ) ) )
							{
								new DigTimer( from, m_Map, new Point3D( x, y, z - 14 ), map, z ,m_Map.m_type).Start();
							}
							else
							{
								from.SendLocalizedMessage( 503020 ); // You are already digging treasure.
							}
						}
					}
					else if ( Utility.InRange( new Point3D( p ), from.Location, 8 ) ) // We're close, but not quite
					{
						from.SendLocalizedMessage( 503032 ); // You dig and dig but no treasure seems to be here.
					}
					else
					{
						from.SendLocalizedMessage( 503035 ); // You dig and dig but fail to find any treasure.
					}
				}
			}

		}

		private class DigTimer : Timer
		{
			private Mobile m_From;
			private TreasureMap m_TreasureMap;
			private Map m_Map;
			private TreasureMapChest m_Chest;
			private int m_Count;
			private int m_Z;
			private DateTime m_NextSkillTime;
			private DateTime m_NextSpellTime;
			private DateTime m_NextActionTime;
			private DateTime m_LastMoveTime;
			private ChestThemeType type;
			private bool themed;
			public DigTimer( Mobile from, TreasureMap treasureMap, Point3D p, Map map, int z, ChestThemeType m_type) : base( TimeSpan.Zero, TimeSpan.FromSeconds( 1.0 ) )
			{
				
				m_From = from;
				m_TreasureMap = treasureMap;
				m_Map = map;
				m_Z = z;
				type = m_type;
				themed = m_TreasureMap.m_Themed;

				if (themed == false) themed = TreasureTheme.GetIsThemed(m_TreasureMap.Level);
					m_TreasureMap.m_Themed = themed;

				if(themed == true && type == ChestThemeType.None)
				{
					type = (ChestThemeType)TreasureTheme.GetThemeType(m_TreasureMap.Level); 
				}

				m_TreasureMap.m_type = type;
				m_Chest = new TreasureMapChest( from, m_TreasureMap.m_Level , themed, type );
				m_Chest.MoveToWorld( p, map );
				
				m_NextSkillTime = from.NextSkillTime;
				m_NextSpellTime = from.NextSpellTime;
				m_NextActionTime = from.NextActionTime;
				m_LastMoveTime = from.LastMoveTime;
			}

			protected override void OnTick()
			{
				if ( m_NextSkillTime != m_From.NextSkillTime || m_NextSpellTime != m_From.NextSpellTime || m_NextActionTime != m_From.NextActionTime )
				{
					Stop();
					m_From.EndAction( typeof( TreasureMap ) );
					m_Chest.Delete();
				}
				else if ( m_LastMoveTime != m_From.LastMoveTime )
				{
					m_From.SendLocalizedMessage( 503023 ); // You cannot move around while digging up treasure. You will need to start digging anew.

					Stop();
					m_From.EndAction( typeof( TreasureMap ) );
					m_Chest.Delete();
				}
				/*else if ( !m_Map.CanFit( m_Chest.X, m_Chest.Y, m_Z, 16, true, true ) )
				{
					m_From.SendLocalizedMessage( 503024 ); // You stop digging because something is directly on top of the treasure chest.

					Stop();
					m_From.EndAction( typeof( TreasureMap ) );
					m_Chest.Delete();
				}*/
				else
				{
					m_From.RevealingAction();

					m_Count++;

					m_Chest.Location = new Point3D( m_Chest.Location.X, m_Chest.Location.Y, m_Chest.Location.Z + 1 );
					m_From.Direction = m_From.GetDirectionTo( m_Chest.GetWorldLocation() );

					if ( m_Count == 14 )
					{
						Stop();
						m_From.EndAction( typeof( TreasureMap ) );

						m_TreasureMap.Completed = true;

					
						// checks to see if the map is a themed map and if so gets the theme type based on level of map
						// and sends appropriate theme message/warning
						
												
						// checks to see if the map is a themed map and already has a theme set
						// and sends appropriate theme message/warning
						if(themed == true && type != ChestThemeType.None) m_From.SendMessage(TreasureTheme.GetThemeMessage(type));
							
						

						if ( m_TreasureMap.Level >= 2 )
						{
							//generates 1 of the highest mobs for pirate or undead iob chests
							TreasureTheme.Spawn( m_TreasureMap.Level, m_Chest.Location, m_Chest.Map, null, themed, type,true,true); 
							//generates guardian spawn numbers based on if themed or not
							for ( int i = 0; i < TreasureTheme.GetGuardianSpawn(themed, type); ++i ) 
							{
								if(type == ChestThemeType.Undead || type == ChestThemeType.Pirate) 
								{
									//spawns rest of pirate or undead initial guardian spawn with out highest rank mobs appereing
									TreasureTheme.Spawn( m_TreasureMap.Level, m_Chest.Location, m_Chest.Map, null, themed, type, true,false);
								}
							else
								//not pirate or undead chest spawn as per normal random guardians
								TreasureTheme.Spawn( m_TreasureMap.Level, m_Chest.Location, m_Chest.Map, null, themed, type, false, false);
							}
						}
					}
					else
					{
						if ( m_From.Body.IsHuman && !m_From.Mounted )
							m_From.Animate( 11, 5, 1, true, false, 0 );

						new SoundTimer( m_From, 0x125 + (m_Count % 2) ).Start();
					}
				}
			}

			private class SoundTimer : Timer
			{
				private Mobile m_From;
				private int m_SoundID;

				public SoundTimer( Mobile from, int soundID ) : base( TimeSpan.FromSeconds( 0.9 ) )
				{
					m_From = from;
					m_SoundID = soundID;
				}

				protected override void OnTick()
				{
					m_From.PlaySound( m_SoundID );
				}
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !m_Completed && m_Decoder == null )
			{
				double midPoint = 0.0;

				switch ( m_Level )
				{
					case 1: midPoint =  27.0; break;
					case 2: midPoint =  71.0; break;
					case 3: midPoint =  81.0; break;
					case 4: midPoint =  91.0; break;
					case 5: midPoint = 100.0; break;
				}

				double minSkill = midPoint - 30.0;
				double maxSkill = midPoint + 30.0;

				if ( from.Skills[SkillName.Cartography].Value < midPoint )
				{
					from.SendLocalizedMessage( 503013 ); // The map is too difficult to attempt to decode.
					return;
				}

				if ( from.CheckSkill( SkillName.Cartography, minSkill, maxSkill ) )
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 503019 ); // You successfully decode a treasure map!
					Decoder = from;

					from.PlaySound( 0x249 );
					base.OnDoubleClick( from );
				}
				else
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 503018 ); // You fail to make anything of the map.
				}
			}
			else if ( m_Completed )
			{
				from.SendLocalizedMessage( 503014 ); // This treasure hunt has already been completed.
				base.OnDoubleClick( from );
			}
			else
			{
				from.SendLocalizedMessage( 503017 ); // The treasure is marked by the red pin. Grab a shovel and go dig it up!
				base.OnDoubleClick( from );
			}
		}

		public override int LabelNumber{ get{ return (m_Decoder != null ? 1041516 + m_Level : 1041510 + m_Level); } }

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			list.Add( m_Map == Map.Felucca ? 1041502 : 1041503 ); // for somewhere in Felucca : for somewhere in Trammel

			if ( m_Completed )
				list.Add( 1041507, m_Decoder == null ? "someone" : m_Decoder.Name ); // completed by ~1_val~
		}
// changed this 
            // public override void OnSingleClick( Mobile from )
		//{
			//if ( m_Completed )
				//from.Send( new MessageLocalizedAffix( Serial, ItemID, MessageType.Label, 0x3B2, 3, 1048030, "", AffixType.Append, String.Format( " completed by {0}", m_Decoder == null ? "someone" : m_Decoder.Name ), "" ) );
			//else if ( m_Decoder != null )
				//LabelTo( from, 1041516 + m_Level );
			//else
				//LabelTo( from, 1041522, String.Format( "#{0}\t \t#{1}", 1041510 + m_Level, m_Map == Map.Felucca ? 1041502 : 1041503 ) );
		//}
//to this
		public override void OnSingleClick( Mobile from )
		{
			if ( m_Completed )
				from.Send( new MessageLocalizedAffix( Serial, ItemID, MessageType.Label, 0x3B2, 3, 1048030, "", AffixType.Append, String.Format( " completed by {0}", m_Decoder == null ? "someone" : m_Decoder.Name ), "" ) );
			else if ( m_Decoder != null )
				LabelTo( from, 1041516 + m_Level );
			else
			{
				//LabelTo( from, 1041522, String.Format( "#{0}\t \t#{1}", 1041510 + m_Level, m_Map == Map.Felucca ? 1041502 : 1041503 ) );
				LabelTo( from, 1041510 + m_Level );
				if( m_Location.X >= 3304 && m_Location.Y >= 2340 && m_Location.X < 3845 && m_Location.Y < 2977)
					LabelTo( from, "for somewhere in Ocllo" );
				else
					LabelTo(from, m_Map == Map.Felucca ? 1041502 : 1041503 );
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
			writer.Write( m_Themed );
			writer.Write( (int)m_type );
		
			writer.Write( m_Level );
			writer.Write( m_Completed );
			writer.Write( m_Decoder );
			writer.Write( m_Map );
			writer.Write( m_Location );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
 				{
 				m_Themed = reader.ReadBool();
 				m_type = (ChestThemeType)reader.ReadInt();
 				goto case 0;
 				}
	
				case 0:
				{
					m_Level = (int)reader.ReadInt();
					m_Completed = reader.ReadBool();
					m_Decoder = reader.ReadMobile();
					m_Map = reader.ReadMap();
					m_Location = reader.ReadPoint2D();

					break;
				}
			}
			if ( version < 1)
			{
 			m_Themed = false;
 			m_type = ChestThemeType.None;
 			}
		}
	}
}