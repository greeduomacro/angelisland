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

/* Engines/Township/Fortifications/Walls.cs
 * CHANGELOG:
 * 11/30/08, Pix
 *		Added Alive checks.
 * 11/16/08, Pix
 *		Refactored, rebalanced, and fixed stuff
 * 10/19/08, Pix
 *		Spelling fix.
 * 10/17/08, Pix
 *		Reduced the skill requirement to repair the wall.
 * 10/17/08, Pix
 *		Fixed the timer for repair/damage to stop if they moved.
 * 10/15/08, Pix
 *		Changed that you need to be within 2 tiles of the wall to damage/repair it.
 * 10/15/08, Pix
 *		Added graphics.
 *		Added delays to repair/damage.
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
	#region Base Wall Class

	public class BaseFortificationWall : Item
	{
		#region Member Variables

		private DateTime m_PlacementDate = DateTime.MinValue;
		private Mobile m_Placer = null;
		private int m_OriginalMaxHits = 100;
		private int m_MaxHits = 100;
		private int m_Hits = 100;
		private SkillName m_RepairSkill = SkillName.Tinkering;
		DateTime m_LastRepair = DateTime.Now;
		DateTime m_LastDamage = DateTime.Now;

		private Mobile m_RepairWorker = null; //Note that this should not be serialized
		private Mobile m_DamageWorker = null; //Note that this should not be serialized

		#endregion

		#region Constructors

		public BaseFortificationWall()
			: base(0x27C)
		{
			Movable = false;
			Weight = 150;
		}

		public BaseFortificationWall(int itemID)
			: base(itemID)
		{
			Movable = false;
		}

		public BaseFortificationWall(Serial serial)
			: base(serial)
		{
		}

		#endregion

		#region Context Menu Entries

		public override void GetContextMenuEntries(Mobile from, System.Collections.ArrayList list)
		{
			base.GetContextMenuEntries(from, list);

			if (this.Parent == null)
			{
				list.Add(new InspectWallEntry(from, this));
				if (this.Placer == from)
				{
					list.Add(new RepairWallEntry(from, this, false, true)); //destroy
				}
				list.Add(new RepairWallEntry(from, this, true)); //normal repair
				list.Add(new RepairWallEntry(from, this, false)); //normal damage
			}
		}

		#endregion

		#region Properties
		
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Mobile Placer
		{
			get { return m_Placer; }
			set { m_Placer = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public DateTime PlacementDate
		{
			get { return m_PlacementDate; }
			set { m_PlacementDate = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Hits
		{
			get { return m_Hits; }
			set { m_Hits = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int MaxHits
		{
			get { return m_MaxHits; }
			set { m_MaxHits = value; if (m_Hits > m_MaxHits) m_Hits = m_MaxHits; }
		}
		[CommandProperty(AccessLevel.Counselor)]
		public int OriginalHits
		{
			get { return m_OriginalMaxHits; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public DateTime LastRepair
		{
			get { return m_LastRepair; }
			set { m_LastRepair = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public DateTime LastDamage
		{
			get { return m_LastDamage; }
			set { m_LastDamage = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SkillName RepairSkill
		{
			get { return m_RepairSkill; }
			set { m_RepairSkill = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Mobile CurrentRepairWorker
		{
			get { return m_RepairWorker; }
			set { m_RepairWorker = value; }
		}
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Mobile CurrentDamageWorker
		{
			get { return m_DamageWorker; }
			set { m_DamageWorker = value; }
		}

		#endregion

		#region Repair virtual functions

		public virtual int GetRepairAmount()
		{
			return 1;
		}
		public virtual Type GetRepairType()
		{
			return typeof(Board);
		}
		public virtual string GetRepairTypeDesc()
		{
			return "boards";
		}

		#endregion

		#region Placement

		public void Place(Mobile m, Point3D loc)
		{
			m.SendMessage("You begin constructing the wall.");
			new InternalPlacementTimer(m, loc, this).Start();
		}

		#endregion

		#region Change/CanChange
		public virtual void Change(Mobile from)
		{
			from.SendMessage("Change not implemented.");
		}

		public bool CanChange(Mobile from)
		{
			bool bReturn = false;

			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			CustomRegion cr = CustomRegion.FindDRDTRegion(from.Map, this.Location);
			if (cr is TownshipRegion)
			{
				TownshipRegion tsr = cr as TownshipRegion;
				if (tsr != null && tsr.TStone != null && tsr.TStone.Guild != null &&
					tsr.TStone.Guild == from.Guild)
				{
					bReturn = true;
				}
				else
				{
					from.SendMessage("You must be a member of this township to modify this wall.");
					bReturn = false;
				}
			}
			else
			{
				from.SendMessage("You must be within the township to modify this wall.");
				bReturn = false;
			}

			return bReturn;
		}
		#endregion

		#region Hits/Repair methods

		public void SetInitialHits()
		{
			if (m_Placer == null)
			{
				return;
			}

			int carp = (int)m_Placer.Skills[SkillName.Carpentry].Value;
			int tink = (int)m_Placer.Skills[SkillName.Tinkering].Value;
			int mine = (int)m_Placer.Skills[SkillName.Mining].Value;
			int jack = (int)m_Placer.Skills[SkillName.Lumberjacking].Value;

			int smit = (int)m_Placer.Skills[SkillName.Blacksmith].Value;
			int alch = (int)m_Placer.Skills[SkillName.Alchemy].Value;
			int item = (int)m_Placer.Skills[SkillName.ItemID].Value;
			int mace = (int)m_Placer.Skills[SkillName.Macing].Value;
			int scrb = (int)m_Placer.Skills[SkillName.Inscribe].Value;
			int dtct = (int)m_Placer.Skills[SkillName.DetectHidden].Value;
			int cart = (int)m_Placer.Skills[SkillName.Cartography].Value;

			int baseHits = 100;
			//"main" skills add the most
			baseHits += carp / 4; //+25 @ GM
			baseHits += tink / 4; //+25 @ GM
			baseHits += mine / 4; //+25 @ GM
			baseHits += jack / 4; //+25 @ GM
			//"support" skills add some more
			baseHits += smit / 10;//+10 @ GM
			baseHits += alch / 10;//+10 @ GM
			baseHits += item / 10;//+10 @ GM
			baseHits += mace / 10;//+10 @ GM
			baseHits += scrb / 10;//+10 @ GM
			baseHits += dtct / 10;//+10 @ GM
			baseHits += cart / 10;//+10 @ GM

			//Special bonuses for different (derived) walls
			baseHits += GetSpecialBuildBonus();

			m_OriginalMaxHits = baseHits;
			m_MaxHits = m_OriginalMaxHits;
			m_Hits = m_MaxHits;
			m_LastRepair = DateTime.Now;
			m_LastDamage = DateTime.Now;
		}

		public void BeginRepair(Mobile m, bool repair, bool full)
		{
			double daysPerRepair = 0.21;
			if (Misc.TestCenter.Enabled)
			{
				daysPerRepair = 1 / (24 * 60); //once per minute on TC
			}

			if ((repair && DateTime.Now < m_LastRepair.AddDays(daysPerRepair)) ||
				 (!repair && !full && DateTime.Now < m_LastDamage.AddDays(daysPerRepair)))
			{
				m.SendMessage("The wall has been worked on recently, this wall cannot be worked on yet.");
				return;
			}

			if (repair && this.CurrentRepairWorker != null)
			{
				m.SendMessage("Someone else is repairing this wall.");
				return;
			}
			else if (!repair && this.CurrentDamageWorker != null)
			{
				m.SendMessage("Someone else is damaging this wall.");
				return;
			}

			if (repair)
			{
				this.CurrentRepairWorker = m;
			}
			else
			{
				this.CurrentDamageWorker = m;
			}

			if (repair)
			{
				int amount = GetRepairAmount() * (m_MaxHits - m_Hits);
				if (m.Backpack.ConsumeTotal(GetRepairType(), amount) == false)
				{
					m.SendMessage("You need " + amount + " " + GetRepairTypeDesc() + " to repair this wall.");
					return;
				}
			}

			if (repair)
			{
				m.SendMessage("You begin repairing the wall.");
			}
			else
			{
				m.SendMessage("You begin damaging the wall.");
			}
			new InternalRepairDamageTimer(m, this, repair, full).Start();
		}

		public void EndRepair(Mobile m, bool repair, bool full)
		{
			if (repair)
			{
				m_LastRepair = DateTime.Now;
				m_RepairWorker = null; //safety!
			}
			else
			{
				m_LastDamage = DateTime.Now;
				m_DamageWorker = null; //safety!
			}

			double skillLevel = m.Skills[m_RepairSkill].Base;
			int hitsDiff = 20;

			if (repair == false)
			{
				if (skillLevel >= 90.0)
					hitsDiff += 25;
				else if (skillLevel >= 70.0)
					hitsDiff += 15;
				else
					hitsDiff += 10;
			}
			else
			{
				hitsDiff -= GetSpecialRepairBonus(m);
			}

			if (hitsDiff <= 0)
			{
				hitsDiff = 1;
			}

			if (repair)
			{
				if (m.CheckSkill(m_RepairSkill, 0, 75))
				{
					m.SendMessage("You repair the wall.");
					m_MaxHits -= (hitsDiff / 2);
					//repair repairs to full, hence the cost based on the difference
					m_Hits = m_MaxHits;
				}
				else
				{
					m.SendMessage("You seem to harm the wall.");
					m_MaxHits -= hitsDiff;
					m_Hits -= hitsDiff;
				}
			}
			else
			{
				if (full)
				{
					if (m == this.Placer)
					{
						m.SendMessage("You destroy the wall.");
						m_Hits = -1;
					}
				}
				else
				{
					if (m.CheckSkill(m_RepairSkill, 0, 75))
					{
						m.SendMessage("You harm the wall.");
						m_Hits -= hitsDiff;
					}
					else
					{
						m.SendMessage("You don't seem to have an effect.");
					}
				}
			}

			if (m_Hits < 0)
			{
				m.SendMessage("The wall crumbles down.");
				this.Delete();
			}
		}

		public virtual int GetSpecialBuildBonus()
		{
			return 0;
		}

		public virtual int GetSpecialRepairBonus(Mobile m)
		{
			if (m == this.m_Placer) //if the repairer is the placer, give them a bonus to repair
			{
				return 6;
			}
			else
			{
				return 0;
			}
		}

		#endregion

		#region Serialize/Deserialize

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); //version

			writer.Write(this.m_LastDamage);
			//Version 0 below :)
			writer.Write((int)this.m_RepairSkill);
			writer.Write(this.m_Placer);
			writer.Write(this.m_PlacementDate);
			writer.Write(this.m_OriginalMaxHits);
			writer.Write(this.m_MaxHits);
			writer.Write(this.m_LastRepair);
			writer.Write(this.m_Hits);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					m_LastDamage = reader.ReadDateTime();
					goto case 0;
				case 0:
					m_RepairSkill = (SkillName)reader.ReadInt();
					m_Placer = reader.ReadMobile();
					m_PlacementDate = reader.ReadDateTime();
					m_OriginalMaxHits = reader.ReadInt();
					m_MaxHits = reader.ReadInt();
					m_LastRepair = reader.ReadDateTime();
					m_Hits = reader.ReadInt();
					break;
			}
		}

		#endregion

		#region Notify
		public void NotifyOfDamager(Mobile damager)
		{
			if (damager == null) return; //safety

			Point3D targetPoint = this.Location;
			CustomRegion cr = CustomRegion.FindDRDTRegion(damager.Map, targetPoint);
			if (cr is TownshipRegion)
			{
				TownshipRegion tsr = cr as TownshipRegion;
				if (tsr != null && 
					tsr.TStone != null && 
					tsr.TStone.Guild != null &&
					tsr.TStone.Guild != damager.Guild)
				{
					tsr.TStone.Guild.GuildMessage(
						string.Format("{0} at {1} is damaging your township's wall.",
							damager.Name, damager.Location.ToString()));
				}
			}
		}
		#endregion

		#region Internal Placement Timer Class

		private class InternalPlacementTimer : Timer
		{
			private const int SECONDS_UNTIL_DONE = 120;
			private Mobile m_Mobile;
			private Point3D m_Location;
			private Point3D m_MobLoc;
			private BaseFortificationWall m_Wall;
			private Map m_Map;
			private int m_Count = 0;

			public InternalPlacementTimer(Mobile m, Point3D loc, BaseFortificationWall wall)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_Mobile = m;
				m_Map = m.Map;
				m_Location = loc;
				m_Wall = wall;
				m_MobLoc = new Point3D(m.Location);

				m_Mobile.Animate(11, 5, 1, true, false, 0);

				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Count++;

				if (!m_Mobile.Alive)
				{
					this.Stop();
					m_Wall.Delete();
				}
				else
				{
					if (m_Mobile.Hidden)
					{
						m_Mobile.Hidden = false;
					}

					bool bHasMoved = false;
					if (m_Mobile.Location != m_MobLoc)
					{
						bHasMoved = true;
					}

					if (bHasMoved || m_Mobile.Map == Map.Internal)
					{
						this.Stop();
						m_Mobile.SendMessage("You stop building the wall.");
						m_Wall.Delete();
					}
					else
					{
						if (m_Count > SECONDS_UNTIL_DONE || (Server.Misc.TestCenter.Enabled && m_Count > 10)) //1 minute to build the wall
						{
							this.Stop();
							m_Mobile.SendMessage("You build the wall.");

							m_Wall.PlacementDate = DateTime.Now;
							m_Wall.Placer = m_Mobile;
							m_Wall.SetInitialHits();
							m_Wall.MoveToWorld(m_Location, m_Map);
						}
						else if (m_Count % 5 == 0)
						{
							m_Mobile.Emote("*builds a wall*");
							m_Mobile.Animate(11, 5, 1, true, false, 0);
						}
					}
				}
			}
		}

		#endregion

		#region Internal RepairDamage Timer Clase
		private class InternalRepairDamageTimer : Timer
		{
			private const int SECONDS_UNTIL_DONE = 120;
			private Mobile m_Mobile;
			private Point3D m_MobLoc;
			private BaseFortificationWall m_Wall;
			private bool m_bRepair = true;
			private bool m_bFull = false;
			private int m_Count = 0;

			public InternalRepairDamageTimer(Mobile m, BaseFortificationWall wall, bool bRepair, bool bFull)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_Mobile = m;
				m_Wall = wall;
				m_bRepair = bRepair;
				m_bFull = bFull;
				m_MobLoc = new Point3D(m.Location);

				m_Mobile.Animate(11, 5, 1, true, false, 0);

				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Count++;

				if (!m_Mobile.Alive)
				{
					this.Stop();
					if (m_bRepair) m_Wall.CurrentRepairWorker = null;
					else m_Wall.CurrentDamageWorker = null;
				}
				else
				{
					if (m_Mobile.Hidden)
					{
						m_Mobile.Hidden = false;
					}

					bool bHasMoved = false;
					if (m_Mobile.Location != m_MobLoc)
					{
						bHasMoved = true;
					}

					if (bHasMoved || m_Mobile.Map == Map.Internal)
					{
						if (m_bRepair)
						{
							m_Mobile.SendMessage("You move and stop repairing the wall.");
						}
						else
						{
							m_Mobile.SendMessage("You move and stop damaging the wall.");
						}
						this.Stop();
						if (m_bRepair) m_Wall.CurrentRepairWorker = null;
						else m_Wall.CurrentDamageWorker = null;
					}
					else
					{
						if (m_Count > SECONDS_UNTIL_DONE || (Server.Misc.TestCenter.Enabled && m_Count > 10)) //1 minute to repair the wall
						{
							this.Stop();
							if (m_bRepair) m_Wall.CurrentRepairWorker = null;
							else m_Wall.CurrentDamageWorker = null;
							m_Wall.EndRepair(m_Mobile, m_bRepair, m_bFull);
						}
						else if (m_Count % 5 == 0)
						{
							if (m_bRepair)
							{
								m_Mobile.Emote("*repairs the wall*");
							}
							else
							{
								m_Mobile.Emote("*damages the wall*");
								m_Wall.NotifyOfDamager(m_Mobile);
							}
							m_Mobile.Animate(11, 5, 1, true, false, 0);
						}
					}
				}
			}
		}
		#endregion

	}

	#endregion

	#region Derived Wall Classes

	#region Stone Wall
	public class StoneFortificationWall : BaseFortificationWall
	{
		protected static int[] m_ids = new int[] 
			{
				//full walls:
				0x001B, //stone wall
				0x001C, //stone wall other direction
				0x001A, //stone wall corner
				//half walls:
				0x0025, //stone half-wall
				0x0026, //stone half-wall other direction
				0x0024 //stone half-wall corner
			};

		public StoneFortificationWall()
			: base(0x001B)
		{
			this.RepairSkill = SkillName.Tinkering;
			this.Weight = 200;
		}

		public StoneFortificationWall(Serial serial)
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
		}

		public override void Change(Mobile from)
		{
			if (CanChange(from))
			{
				int index = -1;
				for (int i = 0; i < m_ids.Length; i++)
				{
					if (m_ids[i] == this.ItemID)
					{
						index = i;
						break;
					}
				}

				if (index == -1)
				{
					this.ItemID = m_ids[0];
				}
				else
				{
					index++;
					if (index >= m_ids.Length)
					{
						index = 0;
					}
					this.ItemID = m_ids[index];
				}
			}
			else
			{
				from.SendMessage("You can't change this wall.");
			}
		}
	}
	#endregion

	#region Spear Wall
	public class SpearFortificationWall : BaseFortificationWall
	{
		protected static int[] m_ids = new int[] 
			{
				//full walls:
				0x221, //spear wall
				0x222, //spear wall other direction
				0x223, //spear wall corner
				0x227, //log wall
				0x228, //log wall other direction
				0x226, //log wall corner
				//half walls:
				0x423, //spear half-wall
				0x424, //spear half-wall other direction
				0x425 //spear half-wall corner
			};

		public SpearFortificationWall()
			: base(0x221)
		{
			this.RepairSkill = SkillName.Carpentry;
			this.Weight = 150;
		}

		public SpearFortificationWall(Serial serial)
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
		}

		public override void Change(Mobile from)
		{
			if (CanChange(from))
			{
				int index = -1;
				for (int i = 0; i < m_ids.Length; i++)
				{
					if (m_ids[i] == this.ItemID)
					{
						index = i;
						break;
					}
				}

				if (index == -1)
				{
					this.ItemID = m_ids[0];
				}
				else
				{
					index++;
					if (index >= m_ids.Length)
					{
						index = 0;
					}
					this.ItemID = m_ids[index];
				}
			}
			else
			{
				from.SendMessage("You can't change this wall.");
			}
		}

	}
	#endregion

	#endregion

	#region Context Menus

	#region Repair Wall Context Menu Entry

	public class RepairWallEntry : ContextMenuEntry
	{
		private BaseFortificationWall m_Wall;
		private Mobile m_From;
		private bool m_Repair = true;
		private bool m_Full = false;

		public RepairWallEntry(Mobile from, BaseFortificationWall wall, bool repair)
			: this(from, wall, repair, false)
		{
		}

		public RepairWallEntry(Mobile from, BaseFortificationWall wall, bool repair, bool full)
			: base(5121, 2)
		{
			m_Wall = wall;
			m_From = from;
			m_Repair = repair;
			m_Full = full;

			if (repair == false)
			{
				this.Number = 5009; //Smite

				if (full == true)
				{
					this.Number = 6275; //Demolish -- //5011; //Delete
				}
			}
			else
			{
				this.Number = 5121; //Refresh

				if (full == true)
				{
					this.Number = 5006; //Resurrect
				}
			}

			Enabled = true;
		}
		public override void OnClick()
		{
			m_Wall.BeginRepair(m_From, m_Repair, m_Full);
		}
	}

	#endregion

	#region Inspect Wall Context Menu Entry

	public class InspectWallEntry : ContextMenuEntry
	{
		private BaseFortificationWall m_Wall;
		private Mobile m_From;

		public InspectWallEntry(Mobile from, BaseFortificationWall wall)
			: base(6121, 6)
		{
			m_Wall = wall;
			m_From = from;
		}
		public override void OnClick()
		{
			if (m_From.AccessLevel > AccessLevel.Player)
			{
				m_From.SendMessage("StaffOnly: The wall has {0} of {1} hitpoints.", m_Wall.Hits, m_Wall.MaxHits);
			}

			double percentage = 0;
			if (m_Wall.MaxHits > 0) //protect divide by zero
			{
				percentage = ((double)m_Wall.Hits / (double)m_Wall.MaxHits) * 100.0;
			}

			string message = "You are unable to tell what shape the wall is in.";

			if (percentage == 100)
			{
				message = "The wall is in perfect condition.";
			}
			else if (percentage >= 75)
			{
				message = "The wall is in great condition.";
			}
			else if (percentage >= 50)
			{
				message = "The wall is in good condition.";
			}
			else if (percentage >= 25)
			{
				message = "The wall has taken quite a bit of damage.";
			}
			else
			{
				message = "The wall is close to collapsing.";
			}

			m_From.SendMessage(message);
		}
	}

	#endregion

	#endregion
}
