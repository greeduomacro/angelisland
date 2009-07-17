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

/* Engines/Township/Fortifications/Tools.cs
 * CHANGELOG:
 * 11/16/08, Pix
 *		Refactored, rebalanced, and fixed stuff
 *	10/19/08, Pix
 *		Reduced logs for spear wall from 200 to 150, so people can carry stuff.
 *		Changed message to say walls need iron ingots (as opposed to just ingots).
 * 10/15/08, Pix.
 *		Added 100% home ownership check, under protest.
 * 10/14/08, Pix.
 *		Some code cleanup/consolidation.
 *		Change restrictions on wall placement - not within 1 of a township-owned house, not within 5 of a non-township-owned house.
 *		Fixed ingotsRequired >= comparison!
 * 10/10/08, Pix
 *		Initial.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Multis;
using Server.Targeting;

namespace Server.Township
{
	#region Township Wall Placer class

	class TownshipWallPlacer
	{
		public static bool TryPlace(Mobile from, int boardsRequired, int ingotsRequired, Item tool)
		{
			bool bReturn = false;

			if (tool.IsChildOf(from.Backpack) || tool.Parent == from)
			{
				PlayerMobile pm = from as PlayerMobile;

				if (pm != null)
				{
					Point3D targetPoint = from.Location;
					CustomRegion cr = CustomRegion.FindDRDTRegion(from.Map, targetPoint);
					if (cr is TownshipRegion)
					{
						TownshipRegion tsr = cr as TownshipRegion;
						if (tsr != null && tsr.TStone != null && tsr.TStone.Guild != null &&
							tsr.TStone.Guild == from.Guild)
						{
							//for Adam, check for 100% ownership
							if (1.0 == TownshipDeed.GetPercentageOfGuildedHousesInArea(tsr.TStone.TownshipCenter, tsr.TStone.Map, tsr.TStone.Extended ? TownshipStone.EXTENDED_RADIUS : TownshipStone.INITIAL_RADIUS, tsr.TStone.Guild, false))
							{
								//make sure we're not in a house, or right beside it
								BaseHouse house = null;
								bool bHouseRestriction = false;

								//find all the houses within 5 tiles of this location
								int radius = 5;
								Dictionary<BaseHouse, int> houseDict = new Dictionary<BaseHouse, int>();

								for (int i = radius * -1; i <= radius && house == null; i++)
								{
									for (int j = radius * -1; j <= radius && house == null; j++)
									{
										Point3D currentCheck = new Point3D(targetPoint.X + i, targetPoint.Y + j, targetPoint.Z);
										house = BaseHouse.FindHouseAt(currentCheck, from.Map, 16);
										if (house != null)
										{
											int distance = (int)Math.Sqrt(sqr(currentCheck.X - targetPoint.X) + sqr(currentCheck.Y - targetPoint.Y));
											if (houseDict.ContainsKey(house))
											{
												int saveddistance = houseDict[house];
												if (saveddistance > distance)
												{
													houseDict[house] = distance;
												}
											}
											else
											{
												houseDict.Add(house, distance);
											}
										}
									}
								}

								//now we have a list of all houses within the radius from the point where the wall is to be built and their distance from the point
								foreach (BaseHouse h in houseDict.Keys)
								{
									if (h.Owner != null)
									{
										if (tsr.TStone.Guild.IsMember(h.Owner) == false && houseDict[h] <= 5.0)
										{
											bHouseRestriction = true;
											break;
										}
//										else if (houseDict[h] <= 1.0)
//										{
//											bHouseRestriction = true;
//											break;
//										}
									}
								}

								if (bHouseRestriction == false)
								{
									//next check for teleporters
									bool bTeleporterRestriction = false;
									int iTeleporterDistanceRange = TownshipSettings.WallTeleporterDistance;

									IPooledEnumerable itemlist = from.Map.GetItemsInRange(targetPoint, iTeleporterDistanceRange);
									foreach (Item item in itemlist)
									{
										if (item is Teleporter)
										{
											bTeleporterRestriction = true;
											break;
										}
									}
									itemlist.Free();


									if (bTeleporterRestriction == false)
									{
										bool hasResources = false;
										Item[] boards = from.Backpack.FindItemsByType(typeof(Board), true);
										Item[] ingots = from.Backpack.FindItemsByType(typeof(IronIngot), true);

										int boardCount = 0;
										int ingotCount = 0;
										for (int i = 0; i < boards.Length; i++)
										{
											boardCount += boards[i].Amount;
										}
										for (int i = 0; i < ingots.Length; i++)
										{
											ingotCount += ingots[i].Amount;
										}

										hasResources = (boardCount >= boardsRequired) && (ingotCount >= ingotsRequired);

										if (hasResources)
										{
											from.Backpack.ConsumeTotal(typeof(Board), boardsRequired);
											from.Backpack.ConsumeTotal(typeof(IronIngot), ingotsRequired);
											//from.SendMessage("You place the wall.");

											bReturn = true;
										}
										else
										{
											from.SendMessage("You don't have the resources to build this wall.");
											from.SendMessage("You need {0} boards and {1} iron ingots.", boardsRequired, ingotsRequired);
										}
									}
									else
									{
										from.SendMessage("You can't place this here.");
										from.SendMessage("Certain areas are restricted from placement because they would block normal transportation and entrances/exits.");
									}
								}
								else
								{
									from.SendMessage("You can't place this in or by a house.");
									from.SendMessage("You can place outside of one tile from a house that the township owns,");
									from.SendMessage("but not within 6 tiles of a house that the township doesn't own.");
								}
							}
							else
							{
								from.SendMessage("Your guild must own all houses in this township to build fortifications..");
							}
						}
						else
						{
							from.SendMessage("You must place this within the township that your guild owns.");
						}
					}
					else
					{
						from.SendMessage("You must place this within the township that your guild owns.");
					}

				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}

			return bReturn;
		}

		private static int sqr(int num)
		{
			return ((num * num));
		}

	}

	#endregion

	#region Stone Wall Hammer

	class StoneWallCreationTool : Item
	{
		private int m_UsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[Constructable]
		public StoneWallCreationTool()
			: base(0x13E3)
		{
			Name = "Township Stone Wall Tool";
			m_UsesRemaining = 10;
			Hue = 819;
			Weight = 10;
		}

		public StoneWallCreationTool(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write((int)m_UsesRemaining);
		}
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					m_UsesRemaining = reader.ReadInt();
					break;
			}
			Weight = 10;
		}
		public override void OnDoubleClick(Mobile from)
		{
			if (TownshipWallPlacer.TryPlace(from, 50, 1000, this))
			{
				BaseFortificationWall wall = new StoneFortificationWall();
				wall.Place(from, from.Location);
				m_UsesRemaining -= 1;

				if (m_UsesRemaining <= 0)
				{
					this.Delete();
					from.SendMessage("Your tool is too damaged to use anymore.");
				}
			}
		}
	}

	#endregion

	#region Wooden wall tool
	
	class WoodenWallCreationTool : Item
	{
		private int m_UsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}


		[Constructable]
		public WoodenWallCreationTool()
			: base(0x1031)
		{
			Name = "Township Wooden Wall Tool";
			m_UsesRemaining = 10;
			Hue = 802;
			Weight = 10;
		}

		public WoodenWallCreationTool(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write((int)m_UsesRemaining);
		}
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					m_UsesRemaining = reader.ReadInt();
					break;
			}
			Weight = 10;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (TownshipWallPlacer.TryPlace(from, 400, 100, this))
			{
				BaseFortificationWall wall = new SpearFortificationWall();
				wall.Place(from, from.Location);
				m_UsesRemaining -= 1;

				if (m_UsesRemaining <= 0)
				{
					this.Delete();
					from.SendMessage("Your tool is too damaged to use anymore.");
				}
			}
		}
	}

	#endregion

	#region Wall Customization Tool

	public class WallCustomizationTool : Item
	{
		[Constructable]
		public WallCustomizationTool()
			: base(0xFC1)
		{
			Name = "Township Wall Customization Tool";
			Hue = 803;
			Weight = 10;
		}

		public WallCustomizationTool(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			Weight = 10;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.Backpack != null && IsChildOf(from.Backpack))
			{
				from.SendMessage("Select the wall to change.");
				from.Target = new WallChangeTarget(from);
			}
			else
			{
				from.SendMessage("This must be in your backpack to use.");
			}
		}
	}

	public class WallChangeTarget : Target
	{
		Mobile m_From = null;

		public WallChangeTarget(Mobile from)
			: base(2, true, TargetFlags.None)
		{
			m_From = from;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Item)
			{
				if (targeted is BaseFortificationWall)
				{
					BaseFortificationWall wall = targeted as BaseFortificationWall;
					wall.Change(from);
				}
				else
				{
					from.SendMessage("You must target a township wall that you own.");
				}
			}
			else
			{
				from.SendMessage("You must target a township wall that you own.");
			}
		}


	}

	#endregion

}
