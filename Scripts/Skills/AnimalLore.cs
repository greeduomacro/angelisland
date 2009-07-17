/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property 
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 * 
 * 
 * 
 */

/* ./Scripts/Skills/AnimalLore.cs
 *	ChangeLog :
 *	03/28/07 Taran Kain
 *		Added custom pages section - nonfunctional, needs redesign
 *	12/08/06 Taran Kain
 *		Made the gump closable.
 *	12/07/06 Taran Kain
 *		Added skill and stat locks.
 *		Changed the way the gump handles pages.
 *		Changed "---" to only display when skill == 0.0 (prev showed when skill < 10.0)
 *		Added TC-only code to allow players to see all genes.
 *	11/20/06 Taran Kain
 *		Made the gumps play nice with new loyalty values.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 5 lines removed.
 *  6/6/04 - Old Salty
 *		Altered necessary skill levels to match 100 max skill rather than 120.  I left in a chance to fail at 100.0 
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

namespace Server.SkillHandlers
{
	public class AnimalLore
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.AnimalLore].Callback = new SkillUseCallback(OnUse);
		}

		public static TimeSpan OnUse(Mobile m)
		{
			m.Target = new InternalTarget();

			m.SendLocalizedMessage(500328); // What animal should I look at?

			return TimeSpan.FromSeconds(1.0);
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (!from.Alive)
				{
					from.SendLocalizedMessage(500331); // The spirits of the dead are not the province of animal lore.
				}
				else if (targeted is BaseCreature)
				{
					BaseCreature c = (BaseCreature)targeted;

					if (!c.IsDeadPet)
					{
						if (c.Body.IsAnimal || c.Body.IsMonster || c.Body.IsSea)
						{
							if ((!c.Controlled || !c.Tamable) && from.Skills[SkillName.AnimalLore].Base < 80.0) //changed to 80 from 100 by Old Salty
							{
								from.SendLocalizedMessage(1049674); // At your skill level, you can only lore tamed creatures.
							}
							else if (!c.Tamable && from.Skills[SkillName.AnimalLore].Base < 90.0) //changed to 90 from 110 by Old Salty
							{
								from.SendLocalizedMessage(1049675); // At your skill level, you can only lore tamed or tameable creatures.
							}
							else if (!from.CheckTargetSkill(SkillName.AnimalLore, c, 0.0, 120.0)) //unchanged by Old Salty to allow failure at GM skill
							{
								from.SendLocalizedMessage(500334); // You can't think of anything you know offhand.
							}
							else
							{
								from.CloseGump(typeof(AnimalLoreGump));
								from.SendGump(new AnimalLoreGump(c, from));
							}
						}
						else
						{
							from.SendLocalizedMessage(500329); // That's not an animal!
						}
					}
					else
					{
						from.SendLocalizedMessage(500331); // The spirits of the dead are not the province of animal lore.
					}
				}
				else
				{
					from.SendLocalizedMessage(500329); // That's not an animal!
				}
			}
		}
	}

	public class AnimalLoreGump : Gump
	{
		private Mobile m_User;
		private BaseCreature m_Target;
		private int m_Page;

		private enum ButtonID
		{
			NextPage = 100,
			PrevPage = 102,
			StrLock = 1001,
			DexLock = 1002,
			IntLock = 1003,
			SkillLock = 2000
		}

		private static string FormatSkill(BaseCreature c, SkillName name)
		{
			Skill skill = c.Skills[name];

			if (skill.Base == 0)
				return "<div align=right>---</div>";

			return String.Format("<div align=right>{0:F1}</div>", skill.Base);
		}

		private static string FormatAttributes(int cur, int max)
		{
			if (max == 0)
				return "<div align=right>---</div>";

			return String.Format("<div align=right>{0}/{1}</div>", cur, max);
		}

		private static string FormatStat(int val)
		{
			if (val == 0)
				return "<div align=right>---</div>";

			return String.Format("<div align=right>{0}</div>", val);
		}

		private static string FormatElement(int val)
		{
			if (val <= 0)
				return "<div align=right>---</div>";

			return String.Format("<div align=right>{0}%</div>", val);
		}

		private const int LabelColor = 0x24E5;

		private const int NumStaticPages = 3;

		private int NumTotalPages
		{
			get
			{
				int genes = 0;
				foreach (PropertyInfo pi in m_Target.GetType().GetProperties())
				{
					GeneAttribute attr = (GeneAttribute)Attribute.GetCustomAttribute(pi, typeof(GeneAttribute), true);
					if (attr == null)
						continue;
					if (m_User.AccessLevel < AccessLevel.Counselor && !Server.Misc.TestCenter.Enabled)
					{
						if (attr.Visibility == GeneVisibility.Invisible)
							continue;
						if (attr.Visibility == GeneVisibility.Tame && m_User != m_Target.ControlMaster)
							continue;
					}

					genes++;
				}

				return NumStaticPages + (int)Math.Ceiling(genes / 9.0);
			}
		}

		public AnimalLoreGump(BaseCreature c, Mobile user)
			: this(c, user, 0)
		{
		}

		public AnimalLoreGump(BaseCreature c, Mobile user, int page)
			: base(250, 50)
		{
			m_User = user;
			m_Target = c;
			m_Page = page;
			if (m_Page < 0)
				m_Page = 0;
			if (m_Page >= NumTotalPages)
				m_Page = NumTotalPages - 1;

			AddPage(0);

			AddImage(100, 100, 2080);
			AddImage(118, 137, 2081);
			AddImage(118, 207, 2081);
			AddImage(118, 277, 2081);
			AddImage(118, 347, 2083);

			AddHtml(147, 108, 210, 18, String.Format("<center><i>{0}</i></center>", c.Name), false, false);

			AddButton(240, 77, 2093, 2093, 2, GumpButtonType.Reply, 0);

			AddImage(140, 138, 2091);
			AddImage(140, 335, 2091);

			AddPage(0);
			switch (m_Page)
			{
				case 0:
				{
					#region Attributes
					AddImage(128, 152, 2086);
					AddHtmlLocalized(147, 150, 160, 18, 1049593, 200, false, false); // Attributes

					AddHtmlLocalized(153, 168, 160, 18, 1049578, LabelColor, false, false); // Hits
					AddHtml(280, 168, 75, 18, FormatAttributes(c.Hits, c.HitsMax), false, false);

					AddHtmlLocalized(153, 186, 160, 18, 1049579, LabelColor, false, false); // Stamina
					AddHtml(280, 186, 75, 18, FormatAttributes(c.Stam, c.StamMax), false, false);

					AddHtmlLocalized(153, 204, 160, 18, 1049580, LabelColor, false, false); // Mana
					AddHtml(280, 204, 75, 18, FormatAttributes(c.Mana, c.ManaMax), false, false);

					AddHtmlLocalized(153, 222, 160, 18, 1028335, LabelColor, false, false); // Strength
					AddHtml(320, 222, 35, 18, FormatStat(c.Str), false, false);
					AddStatLock(355, 222, c.StrLock, ButtonID.StrLock);

					AddHtmlLocalized(153, 240, 160, 18, 3000113, LabelColor, false, false); // Dexterity
					AddHtml(320, 240, 35, 18, FormatStat(c.Dex), false, false);
					AddStatLock(355, 240, c.DexLock, ButtonID.DexLock);

					AddHtmlLocalized(153, 258, 160, 18, 3000112, LabelColor, false, false); // Intelligence
					AddHtml(320, 258, 35, 18, FormatStat(c.Int), false, false);
					AddStatLock(355, 258, c.IntLock, ButtonID.IntLock);

					AddImage(128, 278, 2086);
					AddHtmlLocalized(147, 276, 160, 18, 3001016, 200, false, false); // Miscellaneous

					AddHtmlLocalized(153, 294, 160, 18, 1049581, LabelColor, false, false); // Armor Rating
					AddHtml(320, 294, 35, 18, FormatStat(c.VirtualArmor), false, false);

					AddHtmlLocalized(153, 312, 160, 18, 3000120, LabelColor, false, false); // Gender
					AddHtml(280, 312, 75, 18, String.Format("<div align=right>{0}</div>", c.Female ? "Female" : "Male"), false, false);

					break;
					#endregion
				}
				case 1:
				{
					#region Skills
					AddImage(128, 152, 2086);
					AddHtmlLocalized(147, 150, 160, 18, 3001030, 200, false, false); // Combat Ratings

					AddHtmlLocalized(153, 168, 160, 18, 1044103, LabelColor, false, false); // Wrestling
					AddHtml(320, 168, 35, 18, FormatSkill(c, SkillName.Wrestling), false, false);
					AddSkillLock(355, 168, c, SkillName.Wrestling, ButtonID.SkillLock + (int)SkillName.Wrestling);

					AddHtmlLocalized(153, 186, 160, 18, 1044087, LabelColor, false, false); // Tactics
					AddHtml(320, 186, 35, 18, FormatSkill(c, SkillName.Tactics), false, false);
					AddSkillLock(355, 186, c, SkillName.Tactics, ButtonID.SkillLock + (int)SkillName.Tactics);

					AddHtmlLocalized(153, 204, 160, 18, 1044086, LabelColor, false, false); // Magic Resistance
					AddHtml(320, 204, 35, 18, FormatSkill(c, SkillName.MagicResist), false, false);
					AddSkillLock(355, 204, c, SkillName.MagicResist, ButtonID.SkillLock + (int)SkillName.MagicResist);

					AddHtmlLocalized(153, 222, 160, 18, 1044061, LabelColor, false, false); // Anatomy
					AddHtml(320, 222, 35, 18, FormatSkill(c, SkillName.Anatomy), false, false);
					AddSkillLock(355, 222, c, SkillName.Anatomy, ButtonID.SkillLock + (int)SkillName.Anatomy);

					AddHtmlLocalized(153, 240, 160, 18, 1044090, LabelColor, false, false); // Poisoning
					AddHtml(320, 240, 35, 18, FormatSkill(c, SkillName.Poisoning), false, false);
					AddSkillLock(355, 240, c, SkillName.Poisoning, ButtonID.SkillLock + (int)SkillName.Poisoning);

					AddImage(128, 260, 2086);
					AddHtmlLocalized(147, 258, 160, 18, 3001032, 200, false, false); // Lore & Knowledge

					AddHtmlLocalized(153, 276, 160, 18, 1044085, LabelColor, false, false); // Magery
					AddHtml(320, 276, 35, 18, FormatSkill(c, SkillName.Magery), false, false);
					AddSkillLock(355, 276, c, SkillName.Magery, ButtonID.SkillLock + (int)SkillName.Magery);

					AddHtmlLocalized(153, 294, 160, 18, 1044076, LabelColor, false, false); // Evaluating Intelligence
					AddHtml(320, 294, 35, 18, FormatSkill(c, SkillName.EvalInt), false, false);
					AddSkillLock(355, 294, c, SkillName.EvalInt, ButtonID.SkillLock + (int)SkillName.EvalInt);

					AddHtmlLocalized(153, 312, 160, 18, 1044106, LabelColor, false, false); // Meditation
					AddHtml(320, 312, 35, 18, FormatSkill(c, SkillName.Meditation), false, false);
					AddSkillLock(355, 312, c, SkillName.Meditation, ButtonID.SkillLock + (int)SkillName.Meditation);

					break;
					#endregion
				}
				case 2:
				{
					#region Misc
					AddImage(128, 152, 2086);
					AddHtmlLocalized(147, 150, 160, 18, 1049563, 200, false, false); // Preferred Foods

					int foodPref = 3000340;

					if ((c.FavoriteFood & FoodType.FruitsAndVegies) != 0)
						foodPref = 1049565; // Fruits and Vegetables
					else if ((c.FavoriteFood & FoodType.GrainsAndHay) != 0)
						foodPref = 1049566; // Grains and Hay
					else if ((c.FavoriteFood & FoodType.Fish) != 0)
						foodPref = 1049568; // Fish
					else if ((c.FavoriteFood & FoodType.Meat) != 0)
						foodPref = 1049564; // Meat

					AddHtmlLocalized(153, 168, 160, 18, foodPref, LabelColor, false, false);

					AddImage(128, 188, 2086);
					AddHtmlLocalized(147, 186, 160, 18, 1049569, 200, false, false); // Pack Instincts

					int packInstinct = 3000340;

					if ((c.PackInstinct & PackInstinct.Canine) != 0)
						packInstinct = 1049570; // Canine
					else if ((c.PackInstinct & PackInstinct.Ostard) != 0)
						packInstinct = 1049571; // Ostard
					else if ((c.PackInstinct & PackInstinct.Feline) != 0)
						packInstinct = 1049572; // Feline
					else if ((c.PackInstinct & PackInstinct.Arachnid) != 0)
						packInstinct = 1049573; // Arachnid
					else if ((c.PackInstinct & PackInstinct.Daemon) != 0)
						packInstinct = 1049574; // Daemon
					else if ((c.PackInstinct & PackInstinct.Bear) != 0)
						packInstinct = 1049575; // Bear
					else if ((c.PackInstinct & PackInstinct.Equine) != 0)
						packInstinct = 1049576; // Equine
					else if ((c.PackInstinct & PackInstinct.Bull) != 0)
						packInstinct = 1049577; // Bull

					AddHtmlLocalized(153, 204, 160, 18, packInstinct, LabelColor, false, false);

					AddImage(128, 224, 2086);
					AddHtmlLocalized(147, 222, 160, 18, 1049594, 200, false, false); // Loyalty Rating

					// loyalty redo
					int loyaltyval = (int)c.Loyalty / 10;
					if (loyaltyval < 0)
						loyaltyval = 0;
					if (loyaltyval > 11)
						loyaltyval = 11;
					AddHtmlLocalized(153, 240, 160, 18, (!c.Controlled || c.Loyalty == PetLoyalty.None) ? 1061643 : 1049594 + loyaltyval, LabelColor, false, false);

					break;
					#endregion
				}
				default: // rest of the pages are filled with genes - be sure to adjust "pg" calc in here when adding pages
				{
                    int nextpage = 3;

                    // idea for later - flesh out custom pages more, a string[] is hackish

                    //List<string[]> custompages = c.GetAnimalLorePages();
                    //if (custompages != null && page >= nextpage && page < (nextpage + custompages.Count))
                    //{
                    //    foreach (string[] s in custompages)
                    //    {
                    //        for (int i = 0; i < s.Length; i++)
                    //        {
                    //            AddHtml(153, 168 + 18 * i, 150, 18, s[i], false, false);
                    //        }
                    //    }

                    //    nextpage += custompages.Count;
                    //}

					#region Genetics
                    if (page >= nextpage)
                    {
                        List<PropertyInfo> genes = new List<PropertyInfo>();

                        foreach (PropertyInfo pi in c.GetType().GetProperties())
                        {
                            GeneAttribute attr = (GeneAttribute)Attribute.GetCustomAttribute(pi, typeof(GeneAttribute), true);
                            if (attr == null)
                                continue;
                            if (m_User.AccessLevel < AccessLevel.Counselor && !Server.Misc.TestCenter.Enabled)
                            {
                                if (attr.Visibility == GeneVisibility.Invisible)
                                    continue;
                                if (attr.Visibility == GeneVisibility.Tame && m_User != c.ControlMaster)
                                    continue;
                            }

                            genes.Add(pi);
                        }

                        int pg = m_Page - nextpage;

                        AddImage(128, 152, 2086);
                        AddHtml(147, 150, 160, 18, "Genetics", false, false);

                        for (int i = 0; i < 9; i++)
                        {
                            if (pg * 9 + i >= genes.Count)
                                break;

                            GeneAttribute attr = (GeneAttribute)Attribute.GetCustomAttribute(genes[pg * 9 + i], typeof(GeneAttribute), true);
                            AddHtml(153, 168 + 18 * i, 120, 18, attr.Name, false, false);
                            AddHtml(240, 168 + 18 * i, 115, 18, String.Format("<div align=right>{0:G3}</div>", c.DescribeGene(genes[pg * 9 + i], attr)), false, false);
                        }
                    }
					break;
					#endregion
				}
			}

			if (m_Page < NumTotalPages - 1)
				AddButton(340, 358, 5601, 5605, (int)ButtonID.NextPage, GumpButtonType.Reply, 0);
			if (m_Page > 0)
				AddButton(317, 358, 5603, 5607, (int)ButtonID.PrevPage, GumpButtonType.Reply, 0);
		}

		private void AddSkillLock(int x, int y, BaseCreature c, SkillName skill, ButtonID buttonID)
		{
			if (m_Target.ControlMaster != m_User && m_User.AccessLevel < AccessLevel.GameMaster)
				return; // no fooling around with wild/other people's critters!

			Skill sk = c.Skills[skill];

			if (sk != null)
			{
				int buttonID1, buttonID2;
				int xOffset, yOffset;

				switch (sk.Lock)
				{
					default:
					case SkillLock.Up: buttonID1 = 0x983; buttonID2 = 0x983; xOffset = 3; yOffset = 4; break;
					case SkillLock.Down: buttonID1 = 0x985; buttonID2 = 0x985; xOffset = 3; yOffset = 4; break;
					case SkillLock.Locked: buttonID1 = 0x82C; buttonID2 = 0x82C; xOffset = 2; yOffset = 2; break;
				}

				AddButton(x + xOffset, y + yOffset, buttonID1, buttonID2, (int)buttonID, GumpButtonType.Reply, 0);
			}
		}

		private void AddStatLock(int x, int y, StatLockType setting, ButtonID buttonID)
		{
			if (m_Target.ControlMaster != m_User && m_User.AccessLevel < AccessLevel.GameMaster)
				return; // no fooling around with wild/other people's critters!

			int buttonID1, buttonID2;
			int xOffset, yOffset;

			switch (setting)
			{
				default:
				case StatLockType.Up: buttonID1 = 0x983; buttonID2 = 0x983; xOffset = 3; yOffset = 4; break;
				case StatLockType.Down: buttonID1 = 0x985; buttonID2 = 0x985; xOffset = 3; yOffset = 4; break;
				case StatLockType.Locked: buttonID1 = 0x82C; buttonID2 = 0x82C; xOffset = 2; yOffset = 2; break;
			}

			AddButton(x + xOffset, y + yOffset, buttonID1, buttonID2, (int)buttonID, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			switch ((ButtonID)info.ButtonID)
			{
				case ButtonID.NextPage:
				{
					m_Page++;
					break; // gump will be resent at end of OnResponse
				}
				case ButtonID.PrevPage:
				{
					m_Page--;
					break; // gump will be resent at end of OnResponse
				}
				case ButtonID.StrLock:
				{
					switch (m_Target.StrLock)
					{
						case StatLockType.Down: m_Target.StrLock = StatLockType.Locked; break;
						case StatLockType.Locked: m_Target.StrLock = StatLockType.Up; break;
						case StatLockType.Up: m_Target.StrLock = StatLockType.Down; break;
					}
					break;
				}
				case ButtonID.DexLock:
				{
					switch (m_Target.DexLock)
					{
						case StatLockType.Down: m_Target.DexLock = StatLockType.Locked; break;
						case StatLockType.Locked: m_Target.DexLock = StatLockType.Up; break;
						case StatLockType.Up: m_Target.DexLock = StatLockType.Down; break;
					}
					break;
				}
				case ButtonID.IntLock:
				{
					switch (m_Target.IntLock)
					{
						case StatLockType.Down: m_Target.IntLock = StatLockType.Locked; break;
						case StatLockType.Locked: m_Target.IntLock = StatLockType.Up; break;
						case StatLockType.Up: m_Target.IntLock = StatLockType.Down; break;
					}
					break;
				}
				default:
				{
					if (info.ButtonID >= (int)ButtonID.SkillLock)
					{
						int skill = info.ButtonID - (int)ButtonID.SkillLock;
						Skill sk = null;

						if (skill >= 0 && skill < m_Target.Skills.Length)
							sk = m_Target.Skills[skill];

						if (sk != null)
						{
							switch (sk.Lock)
							{
								case SkillLock.Up: sk.SetLockNoRelay(SkillLock.Down); sk.Update(); break;
								case SkillLock.Down: sk.SetLockNoRelay(SkillLock.Locked); sk.Update(); break;
								case SkillLock.Locked: sk.SetLockNoRelay(SkillLock.Up); sk.Update(); break;
							}
						}
					}
					else
						return;

					break;
				}
			}

			
			m_User.SendGump(new AnimalLoreGump(m_Target, m_User, m_Page));
		}
	}
}
