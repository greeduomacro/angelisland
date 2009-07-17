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

/* Scripts\Misc\Titles.cs
 * ChangeLog
 *	2/10.08, Adam
 *		Minor cleanup and comments
 *	1/20/08, Adam
 *      ComputeShopkeeperSkill based on vendor fees
 *	1/20/08, Adam
 *		Add support for the new Shopkeeper skill
 *	7/9/06, Pix
 *		Made IOB Titles independent of IOBEquipped
 * 08/27/05 TK
 *		Added " the Mortal" to title if PlayerMobile.Mortal = true (permadeath system)
 * 12/20/04, Pix
 *		Incorporated IOB Rank Titles.
 *	8/9/04, mith
 *		GetHighestSkill(), commented Core.AOS check, allowing players to choose which skill is displayed based on how skill locks are set.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Scripts.Commands;

namespace Server.Misc
{
	public class Titles
	{
		public const int MinFame = 0;
		public const int MaxFame = 15000;

		public static void AwardFame( Mobile m, int offset, bool message )
		{
			if ( offset > 0 )
			{
				if ( m.Fame >= MaxFame )
					return;

				offset -= m.Fame / 100;

				if ( offset < 0 )
					offset = 0;
			}
			else if ( offset < 0 )
			{
				if ( m.Fame <= MinFame )
					return;

				offset -= m.Fame / 100;

				if ( offset > 0 )
					offset = 0;
			}

			if ( (m.Fame + offset) > MaxFame )
				offset = MaxFame - m.Fame;
			else if ( (m.Fame + offset) < MinFame )
				offset = MinFame - m.Fame;

			m.Fame += offset;

			if ( message )
			{
				if ( offset > 40 )
					m.SendLocalizedMessage( 1019054 ); // You have gained a lot of fame.
				else if ( offset > 20 )
					m.SendLocalizedMessage( 1019053 ); // You have gained a good amount of fame.
				else if ( offset > 10 )
					m.SendLocalizedMessage( 1019052 ); // You have gained some fame.
				else if ( offset > 0 )
					m.SendLocalizedMessage( 1019051 ); // You have gained a little fame.
				else if ( offset < -40 )
					m.SendLocalizedMessage( 1019058 ); // You have lost a lot of fame.
				else if ( offset < -20 )
					m.SendLocalizedMessage( 1019057 ); // You have lost a good amount of fame.
				else if ( offset < -10 )
					m.SendLocalizedMessage( 1019056 ); // You have lost some fame.
				else if ( offset < 0 )
					m.SendLocalizedMessage( 1019055 ); // You have lost a little fame.
			}
		}

		public const int MinKarma = -15000;
		public const int MaxKarma =  15000;

		public static void AwardKarma( Mobile m, int offset, bool message )
		{
			if ( offset > 0 )
			{
				if ( m is PlayerMobile && ((PlayerMobile)m).KarmaLocked )
					return;

				if ( m.Karma >= MaxKarma )
					return;

				offset -= m.Karma / 100;

				if ( offset < 0 )
					offset = 0;
			}
			else if ( offset < 0 )
			{
				if ( m.Karma <= MinKarma )
					return;

				offset -= m.Karma / 100;

				if ( offset > 0 )
					offset = 0;
			}

			if ( (m.Karma + offset) > MaxKarma )
				offset = MaxKarma - m.Karma;
			else if ( (m.Karma + offset) < MinKarma )
				offset = MinKarma - m.Karma;

			bool wasPositiveKarma = ( m.Karma >= 0 );

			m.Karma += offset;

			if ( message )
			{
				if ( offset > 40 )
					m.SendLocalizedMessage( 1019062 ); // You have gained a lot of karma.
				else if ( offset > 20 )
					m.SendLocalizedMessage( 1019061 ); // You have gained a good amount of karma.
				else if ( offset > 10 )
					m.SendLocalizedMessage( 1019060 ); // You have gained some karma.
				else if ( offset > 0 )
					m.SendLocalizedMessage( 1019059 ); // You have gained a little karma.
				else if ( offset < -40 )
					m.SendLocalizedMessage( 1019066 ); // You have lost a lot of karma.
				else if ( offset < -20 )
					m.SendLocalizedMessage( 1019065 ); // You have lost a good amount of karma.
				else if ( offset < -10 )
					m.SendLocalizedMessage( 1019064 ); // You have lost some karma.
				else if ( offset < 0 )
					m.SendLocalizedMessage( 1019063 ); // You have lost a little karma.
			}

			if ( wasPositiveKarma && m.Karma < 0 && m is PlayerMobile && !((PlayerMobile)m).KarmaLocked )
			{
				((PlayerMobile)m).KarmaLocked = true;
				m.SendLocalizedMessage( 1042511, "", 0x22 ); // Karma is locked.  A mantra spoken at a shrine will unlock it again.
			}
		}

		/*	** OSI MODEL **
			1.  Legendary 120 
			2.  Elder 110 
			3.  Grandmaster 100 
			4.  Master 90 
			5.  Adept 80 
			6.  Expert 70 
			7.  Journeyman 60 
			8.  Apprentice 50 
			9.  Novice 40 
			10. Neophyte 30 
			No Title 29 or below 
		 */
		public static string ComputeTitle( Mobile beholder, Mobile beheld )
		{
			StringBuilder title = new StringBuilder();
			try
			{
				int fame = beheld.Fame;
				int karma = beheld.Karma;

				bool showSkillTitle = beheld.ShowFameTitle && ((beholder == beheld) || (fame >= 5000));

				/*if ( beheld.Kills >= 5 )
				{
					title.AppendFormat( beheld.Fame >= 10000 ? "The Murderer {1} {0}" : "The Murderer {0}", beheld.Name, beheld.Female ? "Lady" : "Lord" );
				}
				else*/
				if (beheld.ShowFameTitle || (beholder == beheld))
				{
					for (int i = 0; i < m_FameEntries.Length; ++i)
					{
						FameEntry fe = m_FameEntries[i];

						if (fame <= fe.m_Fame || i == (m_FameEntries.Length - 1))
						{
							KarmaEntry[] karmaEntries = fe.m_Karma;

							for (int j = 0; j < karmaEntries.Length; ++j)
							{
								KarmaEntry ke = karmaEntries[j];

								if (karma <= ke.m_Karma || j == (karmaEntries.Length - 1))
								{
									title.AppendFormat(ke.m_Title, beheld.Name, beheld.Female ? "Lady" : "Lord");
									break;
								}
							}

							break;
						}
					}
				}
				else
				{
					title.Append(beheld.Name);
				}

				if (beheld is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile)beheld;

					if (pm.Mortal)
						title.Append(" the Mortal");

					string iobtitle = "";
					if (pm.IOBAlignment != IOBAlignment.None)
					{
						try
						{
							iobtitle = IOBRankTitle.rank[(int)pm.IOBAlignment, (int)pm.IOBRank];
						}
						catch (Exception exc)
						{
							LogHelper.LogException(exc);
							System.Console.WriteLine("Caught exception in Titles: " + exc.Message);
							System.Console.WriteLine(exc.StackTrace);
						}

						if (iobtitle.Length > 0)
						{
							title.AppendFormat(", {0}", iobtitle);
						}
					}
				}

				string customTitle = beheld.Title;

				if (customTitle != null && (customTitle = customTitle.Trim()).Length > 0)
				{
					title.AppendFormat(" {0}", customTitle);
				}
				else if (showSkillTitle && beheld.Player)
				{
					Skill highest = GetHighestSkill(beheld);// beheld.Skills.Highest;

					if (highest != null && highest.BaseFixedPoint >= 300)
					{
						string skillLevel = (string)Utility.GetArrayCap(m_Levels, (highest.BaseFixedPoint - 300) / 100);
						string skillTitle = highest.Info.Title;

						if (beheld.Female)
						{
							if (skillTitle.EndsWith("man"))
								skillTitle = skillTitle.Substring(0, skillTitle.Length - 3) + "woman";
						}

						title.AppendFormat(", {0} {1}", skillLevel, skillTitle);
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}
			
			return title.ToString();
		}

		/* ** SHOPKEEPER MODEL **
		 * if Flannery was paying max 680 per UO Day, that's about 8,160 per earth day (680*12)
		 * in a month that's 244,800, and in 6 months that's 1,468,800.
		 * So, we'll call 1.5 mil paid in fees Legendary
			1.  Legendary: 1500000 * .00080 1200.0
			2.  Elder: 1375000  * .00080 1100.0
			3.  Grandmaster: 1250000  * .00080 1000.0
			4.  Master: 1125000  * .00080 900.0
			5.  Adept: 1000000  * .00080 800.0 
			6.  Expert: 875000  * .00080 700.0
			7.  Journeyman:  750000  * .00080 600.0 
			8.  Apprentice: 625000  * .00080 500.0 
			9.  Novice: 500000  * .00080 400.0 
			10. Neophyte: 375000  * .00080 300.0
		*/
		private static int ComputeShopkeeperSkill(Mobile m)
		{	// [21:44] Akarius Alexios: She said 480-680, with the top end being fully stocked.
			if (m is PlayerMobile && (m as PlayerMobile).Shopkeeper == true)
			{
				PlayerMobile pm = m as PlayerMobile;
                if (pm.ShopkeeperPoints < 375000)
                    return 300;

                return (int)(pm.ShopkeeperPoints * .00080);
			}
			else
				return 0;
		}

		private static Skill ShopkeeperSkill(Mobile m)
		{
			try
			{
				Mobile tmp = new Mobile();
				Skill sx = new Skill(tmp.Skills, new SkillInfo(-1, "Shopkeeper", 0, 0, 0, "Shopkeeper", null, 0, 0, 0, 0), 100, 1200, SkillLock.Locked);
				sx.BaseFixedPoint = ComputeShopkeeperSkill(m);
				tmp.Delete();
				return sx;
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
			}

			return null;
		}

		private static Skill GetHighestSkill( Mobile m )
		{
			// handle any Angel Island special skills
			if (m is PlayerMobile && (m as PlayerMobile).Shopkeeper == true)
				return ShopkeeperSkill(m);

			// okay, do normal skills processing
			Skills skills = m.Skills;

			//if ( !Core.AOS )
				//return skills.Highest;

			Skill highest = null;

			for ( int i = 0; i < m.Skills.Length; ++i )
			{
				Skill check = m.Skills[i];

				if ( highest == null || check.BaseFixedPoint > highest.BaseFixedPoint )
					highest = check;
				else if ( highest != null && highest.Lock != SkillLock.Up && check.Lock == SkillLock.Up && check.BaseFixedPoint == highest.BaseFixedPoint )
					highest = check;
			}

			return highest;
		}

		private static string[] m_Levels = new string[]
			{
				"Neophyte",
				"Novice",
				"Apprentice",
				"Journeyman",
				"Expert",
				"Adept",
				"Master",
				"Grandmaster",
				"Elder",
				"Legendary"
			};

		private static FameEntry[] m_FameEntries = new FameEntry[]
			{
				new FameEntry( 1249, new KarmaEntry[]
				{
					new KarmaEntry( -10000, "The Outcast {0}" ),
					new KarmaEntry( -5000, "The Despicable {0}" ),
					new KarmaEntry( -2500, "The Scoundrel {0}" ),
					new KarmaEntry( -1250, "The Unsavory {0}" ),
					new KarmaEntry( -625, "The Rude {0}" ),
					new KarmaEntry( 624, "{0}" ),
					new KarmaEntry( 1249, "The Fair {0}" ),
					new KarmaEntry( 2499, "The Kind {0}" ),
					new KarmaEntry( 4999, "The Good {0}" ),
					new KarmaEntry( 9999, "The Honest {0}" ),
					new KarmaEntry( 10000, "The Trustworthy {0}" )
				} ),
				new FameEntry( 2499, new KarmaEntry[]
				{
					new KarmaEntry( -10000, "The Wretched {0}" ),
					new KarmaEntry( -5000, "The Dastardly {0}" ),
					new KarmaEntry( -2500, "The Malicious {0}" ),
					new KarmaEntry( -1250, "The Dishonorable {0}" ),
					new KarmaEntry( -625, "The Disreputable {0}" ),
					new KarmaEntry( 624, "The Notable {0}" ),
					new KarmaEntry( 1249, "The Upstanding {0}" ),
					new KarmaEntry( 2499, "The Respectable {0}" ),
					new KarmaEntry( 4999, "The Honorable {0}" ),
					new KarmaEntry( 9999, "The Commendable {0}" ),
					new KarmaEntry( 10000, "The Estimable {0}" )
				} ),
				new FameEntry( 4999, new KarmaEntry[]
				{
					new KarmaEntry( -10000, "The Nefarious {0}" ),
					new KarmaEntry( -5000, "The Wicked {0}" ),
					new KarmaEntry( -2500, "The Vile {0}" ),
					new KarmaEntry( -1250, "The Ignoble {0}" ),
					new KarmaEntry( -625, "The Notorious {0}" ),
					new KarmaEntry( 624, "The Prominent {0}" ),
					new KarmaEntry( 1249, "The Reputable {0}" ),
					new KarmaEntry( 2499, "The Proper {0}" ),
					new KarmaEntry( 4999, "The Admirable {0}" ),
					new KarmaEntry( 9999, "The Famed {0}" ),
					new KarmaEntry( 10000, "The Great {0}" )
				} ),
				new FameEntry( 9999, new KarmaEntry[]
				{
					new KarmaEntry( -10000, "The Dread {0}" ),
					new KarmaEntry( -5000, "The Evil {0}" ),
					new KarmaEntry( -2500, "The Villainous {0}" ),
					new KarmaEntry( -1250, "The Sinister {0}" ),
					new KarmaEntry( -625, "The Infamous {0}" ),
					new KarmaEntry( 624, "The Renowned {0}" ),
					new KarmaEntry( 1249, "The Distinguished {0}" ),
					new KarmaEntry( 2499, "The Eminent {0}" ),
					new KarmaEntry( 4999, "The Noble {0}" ),
					new KarmaEntry( 9999, "The Illustrious {0}" ),
					new KarmaEntry( 10000, "The Glorious {0}" )
				} ),
				new FameEntry( 10000, new KarmaEntry[]
				{
					new KarmaEntry( -10000, "The Dread {1} {0}" ),
					new KarmaEntry( -5000, "The Evil {1} {0}" ),
					new KarmaEntry( -2500, "The Dark {1} {0}" ),
					new KarmaEntry( -1250, "The Sinister {1} {0}" ),
					new KarmaEntry( -625, "The Dishonored {1} {0}" ),
					new KarmaEntry( 624, "{1} {0}" ),
					new KarmaEntry( 1249, "The Distinguished {1} {0}" ),
					new KarmaEntry( 2499, "The Eminent {1} {0}" ),
					new KarmaEntry( 4999, "The Noble {1} {0}" ),
					new KarmaEntry( 9999, "The Illustrious {1} {0}" ),
					new KarmaEntry( 10000, "The Glorious {1} {0}" )
				} )
			};
	}

	public class FameEntry
	{
		public int m_Fame;
		public KarmaEntry[] m_Karma;

		public FameEntry( int fame, KarmaEntry[] karma )
		{
			m_Fame = fame;
			m_Karma = karma;
		}
	}

	public class KarmaEntry
	{
		public int m_Karma;
		public string m_Title;

		public KarmaEntry( int karma, string title )
		{
			m_Karma = karma;
			m_Title = title;
		}
	}
}