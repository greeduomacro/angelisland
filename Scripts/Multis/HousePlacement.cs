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

/* Scripts/Multis/HousePlacement.cs
 *	ChangeLog: 
 *	5/8/08, Adam
 *		Move the NoHousingRegion check into the new HousePlacementRegionCheck() function,
 *		The old version of NoHousingRegion only checked the center of the house which meant 1/2 of your house could be in the NoHousingRegion.
 *		The HousePlacementRegionCheck() function makes all region checks on ALL regions at theat point and against ALL house tiles.
 *  5/1/07, Adam
 *      Make custom house placement based on TestCenter.Enabled == true
 *	10/16/06, Kit
 *		Made placement invalid if house layers overlap.
 *	8/01/06, weaver
 *		Removed idiotic check before HouseRegion cast.
 *	7/19/06, weaver	
 *		Added check before HouseRegion cast. 
 *	5/03/06, weaver	
 *		Disallowed placement on top of tents (they're special).
 *	9/20/05, Adam
 *		Update Placement to check ALL regions that contain the area in question.
 *		We can define NoHousing regions in Regions.xml.
 *		The old code only checked the FIRST region found.
 *  10/27/04, Lego
 *        added two grass tiles to the list 879 and 881
 *	10/7/04, Adam
 *		Remove Debugging info
 *	10/6/04, Adam
 *		ExceptionTiles()
 *		Fix issues, and add another tileID to the tile exception list for placement (122)
 *		Add debugging info (needs to come out)
 *	10/4/04, Adam
 *		Revert last fix as it's not quite right
 *	10/04/2004 - LegoEater
 *		fixed so can place over dirt tiles
 */

using System;
using System.Collections;
using Server;
using Server.Regions;

namespace Server.Multis
{
	public enum HousePlacementResult
	{
		Valid,
		BadRegion,
		BadLand,
		BadStatic,
		BadItem,
		NoSurface,
		BadRegionHidden,
		BadRegionTownship
	}

	public class HousePlacement
	{
		private const int YardSize = 5;

		// Any land tile which matches one of these ID numbers is considered a road and cannot be placed over.
		private static int[] m_RoadIDs = new int[]
			{
				0x071, 0x08C,
				0x0E8, 0x0EB,
				0x14C, 0x14F,
				0x161, 0x174,
				0x1F0, 0x1F3,
				0x26E, 0x279,
				0x27E, 0x281,
				0x324, 0x3AC,
				0x547, 0x556,
				0x597, 0x5A6,
				0x637, 0x63A,
				0x7AE, 0x7B1,
				0x442, 0x479, // Sand stones
				0x501, 0x510, // Sand stones
				0x009, 0x015, // Furrows
				0x150, 0x15C  // Furrows 
			};

		static bool ExceptionTiles(int TileID) 
		{     
			switch ( TileID )
			{
				case 0x79: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 121
				case 0x7a: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 122
				case 0x7b: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 123
				case 0x7c: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 124
				case 0x7d: /*Console.WriteLine(TileID.ToString());*/ return true;//grass tile 125
				case 0x7e: /*Console.WriteLine(TileID.ToString());*/ return true;//grass tile 126
				case 0x82: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 130
				case 0x83: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 131
				case 0x84: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 132
				case 0x85: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 133
				case 0x86: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 134
				case 0x87: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 135
				case 0x88: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 136
				case 0x89: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 137
				case 0x8a: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 138
				case 0x8b: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 139
				case 0x8c: /*Console.WriteLine(TileID.ToString());*/ return true;//dirt tile 140
                case 0x36f: /*Console.WriteLine(TileID.ToString());*/ return true;//grass tile 879
                case 0x371: /*Console.WriteLine(TileID.ToString());*/ return true;//grass tile 881
            }
			return false;
		}

		private static HousePlacementResult HousePlacementRegionCheck(Mobile from, Map map, Point3D testPoint)
		{
			Sector sector = map.GetSector(testPoint);
			ArrayList list = sector.Regions;

			for (int i = 0; i < list.Count; ++i)
			{
				Region region = (Region)list[i];

				if (region.Contains(testPoint))
				{
					if (region is HouseRegion)
						return HousePlacementResult.BadRegion;

					if (!region.AllowHousing(from, testPoint)) // Cannot place houses in dungeons, towns, treasure map areas etc
					{
						if (region is TreasureRegion)
							return HousePlacementResult.BadRegionHidden;

						return HousePlacementResult.BadRegion;
					}

					if (region is TownshipRegion)
					{
						if (((TownshipRegion)region).CanBuildHouseInTownship(from) == false)
						{
							return HousePlacementResult.BadRegionTownship;
						}
					}

					if (region is NoHousingRegion)
						return HousePlacementResult.BadRegion;
				}
			}

			return HousePlacementResult.Valid;
		}

		public static HousePlacementResult Check( Mobile from, int multiID, Point3D center, out ArrayList toMove )
		{
			// If this spot is considered valid, every item and mobile in this list will be moved under the house sign
			toMove = new ArrayList();

			Map map = from.Map;

			if ( map == null || map == Map.Internal )
				return HousePlacementResult.BadLand; // A house cannot go here

			if ( from.AccessLevel >= AccessLevel.GameMaster )
				return HousePlacementResult.Valid; // Staff can place anywhere

			if ( map == Map.Ilshenar )
				return HousePlacementResult.BadRegion; // No houses in Ilshenar

			// This holds data describing the internal structure of the house
			MultiComponentList mcl = MultiData.GetComponents( multiID );

			if ( multiID >= 0x13EC && multiID < 0x1D00 )
				HouseFoundation.AddStairsTo( ref mcl ); // this is a AOS house, add the stairs

			// Location of the nortwest-most corner of the house
			Point3D start = new Point3D( center.X + mcl.Min.X, center.Y + mcl.Min.Y, center.Z );

			// These are storage lists. They hold items and mobiles found in the map for further processing
			ArrayList items = new ArrayList(), mobiles = new ArrayList();

			// These are also storage lists. They hold location values indicating the yard and border locations.
			ArrayList yard = new ArrayList(), borders = new ArrayList();

			/* RULES:
			 * 
			 * 1) All tiles which are around the -outside- of the foundation must not have anything impassable.
			 * 2) No impassable object or land tile may come in direct contact with any part of the house.
			 * 3) Five tiles from the front and back of the house must be completely clear of all house tiles.
			 * 4) The foundation must rest flatly on a surface. Any bumps around the foundation are not allowed.
			 * 5) No foundation tile may reside over terrain which is viewed as a road.
			 */

			for ( int x = 0; x < mcl.Width; ++x )
			{
				for ( int y = 0; y < mcl.Height; ++y )
				{
					int tileX = start.X + x;
					int tileY = start.Y + y;

					Tile[] addTiles = mcl.Tiles[x][y];

					if ( addTiles.Length == 0 )
						continue; // There are no tiles here, continue checking somewhere else

					Point3D testPoint = new Point3D( tileX, tileY, center.Z );

					HousePlacementResult RegionCheck = HousePlacementRegionCheck(from, map, testPoint);
					if (RegionCheck != HousePlacementResult.Valid)
						return RegionCheck;

					Tile landTile = map.Tiles.GetLandTile( tileX, tileY );
					int landID = landTile.ID & 0x3FFF;

					Tile[] oldTiles = map.Tiles.GetStaticTiles( tileX, tileY, true );

					Sector sector = map.GetSector( tileX, tileY );

					items.Clear();

                    foreach (Item item in sector.Items.Values)
					{
                        if (item == null)
                            continue;

						if ( item.Visible && item.X == tileX && item.Y == tileY )
							items.Add( item );
					}

					mobiles.Clear();

					foreach (Mobile m in sector.Mobiles.Values)
					{
                        if (m == null)
                            continue;

						if ( m.X == tileX && m.Y == tileY )
							mobiles.Add( m );
					}

					int landStartZ = 0, landAvgZ = 0, landTopZ = 0;

					map.GetAverageZ( tileX, tileY, ref landStartZ, ref landAvgZ, ref landTopZ );

					bool hasFoundation = false;

					for ( int i = 0; i < addTiles.Length; ++i )
					{
						Tile addTile = addTiles[i];

						if ( addTile.ID == 0x4001 ) // Nodraw
							continue;

						TileFlag addTileFlags = TileData.ItemTable[addTile.ID & 0x3FFF].Flags;

						bool isFoundation = ( addTile.Z == 0 && (addTileFlags & TileFlag.Wall) != 0 );
						bool hasSurface = false;

						if ( isFoundation )
							hasFoundation = true;

						int addTileZ = center.Z + addTile.Z;
						int addTileTop = addTileZ + addTile.Height;

						if ( (addTileFlags & TileFlag.Surface) != 0 )
							addTileTop += 16;

						if ( addTileTop > landStartZ && landAvgZ > addTileZ )
							return HousePlacementResult.BadLand; // Broke rule #2

						if ( isFoundation && ((TileData.LandTable[landTile.ID & 0x3FFF].Flags & TileFlag.Impassable) == 0) && landAvgZ == center.Z )
							hasSurface = true;

						for ( int j = 0; j < oldTiles.Length; ++j )
						{
							Tile oldTile = oldTiles[j];
							ItemData id = TileData.ItemTable[oldTile.ID & 0x3FFF];

							if ( (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0)) && addTileTop > oldTile.Z && (oldTile.Z + id.CalcHeight) > addTileZ )
								return HousePlacementResult.BadStatic; // Broke rule #2
							else if ( isFoundation && !hasSurface && (id.Flags & TileFlag.Surface) != 0 && (oldTile.Z + id.CalcHeight) == center.Z )
								hasSurface = true;
						}

						for ( int j = 0; j < items.Count; ++j )
						{
							Item item = (Item)items[j];
							ItemData id = item.ItemData;

							if ( addTileTop > item.Z && (item.Z + id.CalcHeight) > addTileZ )
							{
								if ( item.Movable )
									toMove.Add( item );
								else if ( (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0)) )
									return HousePlacementResult.BadItem; // Broke rule #2
							}
							else if ( isFoundation && !hasSurface && (id.Flags & TileFlag.Surface) != 0 && (item.Z + id.CalcHeight) == center.Z )
							{
								hasSurface = true;
							}
						}

						if ( isFoundation && !hasSurface )
							return HousePlacementResult.NoSurface; // Broke rule #4

						for ( int j = 0; j < mobiles.Count; ++j )
						{
							Mobile m = (Mobile)mobiles[j];

							if ( addTileTop > m.Z && (m.Z + 16) > addTileZ )
								toMove.Add( m );
						}
					}

					for ( int i = 0; i < m_RoadIDs.Length; i += 2 )
					{
						if ( (landID >= m_RoadIDs[i] && landID <= m_RoadIDs[i + 1]) && (ExceptionTiles(landID) == false))
						{
							//Console.WriteLine(landID.ToString());
							return HousePlacementResult.BadLand; // Broke rule #5
						}
					}

					if ( hasFoundation )
					{
						for ( int xOffset = -1; xOffset <= 1; ++xOffset )
						{
							for ( int yOffset = -YardSize; yOffset <= YardSize; ++yOffset )
							{
								Point2D yardPoint = new Point2D( tileX + xOffset, tileY + yOffset );

								if ( !yard.Contains( yardPoint ) )
									yard.Add( yardPoint );
							}
						}

						for ( int xOffset = -1; xOffset <= 1; ++xOffset )
						{
							for ( int yOffset = -1; yOffset <= 1; ++yOffset )
							{
								if ( xOffset == 0 && yOffset == 0 )
									continue;

								// To ease this rule, we will not add to the border list if the tile here is under a base floor (z<=8)

								int vx = x + xOffset;
								int vy = y + yOffset;

								if ( vx >= 0 && vx < mcl.Width && vy >= 0 && vy < mcl.Height )
								{
									Tile[] breakTiles = mcl.Tiles[vx][vy];
									bool shouldBreak = false;

									for ( int i = 0; !shouldBreak && i < breakTiles.Length; ++i )
									{
										Tile breakTile = breakTiles[i];

										if ( breakTile.Height == 0 && breakTile.Z <= 8 && TileData.ItemTable[breakTile.ID & 0x3FFF].Surface )
											shouldBreak = true;
									}

									if ( shouldBreak )
										continue;
								}

								Point2D borderPoint = new Point2D( tileX + xOffset, tileY + yOffset );

								if ( !borders.Contains( borderPoint ) )
									borders.Add( borderPoint );
							}
						}
					}
				}
			}

			for ( int i = 0; i < borders.Count; ++i )
			{
				Point2D borderPoint = (Point2D)borders[i];

				Tile landTile = map.Tiles.GetLandTile( borderPoint.X, borderPoint.Y );
				int landID = landTile.ID & 0x3FFF;

				if ( (TileData.LandTable[landID].Flags & TileFlag.Impassable) != 0 )
					return HousePlacementResult.BadLand;

				for ( int j = 0; j < m_RoadIDs.Length; j += 2 )
				{
					if ( (landID >= m_RoadIDs[j] && landID <= m_RoadIDs[j + 1]) && (ExceptionTiles(landID) == false) )
					{
						//Console.WriteLine(landID.ToString());
						return HousePlacementResult.BadLand; // Broke rule #5
					}
				}

				Tile[] tiles = map.Tiles.GetStaticTiles( borderPoint.X, borderPoint.Y, true );

				for ( int j = 0; j < tiles.Length; ++j )
				{
					Tile tile = tiles[j];
					ItemData id = TileData.ItemTable[tile.ID & 0x3FFF];

					if ( id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0 && (tile.Z + id.CalcHeight) > (center.Z + 2)) )
						return HousePlacementResult.BadStatic; // Broke rule #1
				}

				Sector sector = map.GetSector( borderPoint.X, borderPoint.Y );

				foreach (Item item in sector.Items.Values)
				{
                    if (item == null)
                        continue;

					if ( item.X != borderPoint.X || item.Y != borderPoint.Y || item.Movable )
						continue;

					ItemData id = item.ItemData;

					if ( id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0 && (item.Z + id.CalcHeight) > (center.Z + 2)) )
						return HousePlacementResult.BadItem; // Broke rule #1
				}
			}

			for ( int i = 0; i < yard.Count; ++i )
			{
				Point2D yardPoint = (Point2D)yard[i];

				IPooledEnumerable eable = map.GetMultiTilesAt( yardPoint.X, yardPoint.Y );

				foreach ( Tile[] tile in eable )
				{
					for ( int j = 0; j < tile.Length; ++j )
					{
						if ( (TileData.ItemTable[tile[j].ID & 0x3FFF].Flags & (TileFlag.Impassable | TileFlag.Surface)) != 0 )
						{
							eable.Free();
							return HousePlacementResult.BadStatic; // Broke rule #3
						}
					}
				}

				eable.Free();
			}

			return HousePlacementResult.Valid;

			/*if ( blockedLand || blockedStatic || blockedItem )
			{
				from.SendLocalizedMessage( 1043287 ); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
			}
			else if ( !foundationHasSurface )
			{
				from.SendMessage( "The house could not be created here.  Part of the foundation would not be on any surface." );
			}
			else
			{
				BaseHouse house = GetHouse( from );
				house.MoveToWorld( center, from.Map );
				this.Delete();

				for ( int i = 0; i < toMove.Count; ++i )
				{
					object o = toMove[i];

					if ( o is Mobile )
						((Mobile)o).Location = house.BanLocation;
					else if ( o is Item )
						((Item)o).Location = house.BanLocation;
				}
			}*/
		}
	}
}