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

/* Scripts/Engines/AngelIsland/AIGuardSpawn/AIPostGuard.cs
 * Created 4/1/04 by mith
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 1 lines removed.
 *  7/21/04, Adam
 *		1. Redo the setting of skills and setting of Damage 
 *  7/17/04, Adam
 *		1. Add NightSightScroll to drop
 *		2. Replace MindBlastScroll with FireballScroll
 *	5/23/04 smerX
 *		Enabled healing
 *	5/14/04, mith
 *		Modified FightMode from Aggressor to Closest.
 *		Added Speech.
 *	4/12/04 mith
 *		Converted stats/skills to use dynamic values defined in CoreAI.
 *	4/10/04 changes by mith
 * 		Added bag of reagents and scrolls to loot.
 *	4/1/04
 * 		Changed starting skills to be from a range of 70-80 rather than flat 75.0.
 */

using System;
using System.Collections;
using Server.Misc;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Mobiles
{
	public class AIPostGuard : BaseAIGuard
	{
		private TimeSpan m_SpeechDelay = TimeSpan.FromSeconds( 120.0 ); // time between speech
		public DateTime m_NextSpeechTime;

		[Constructable]
		public AIPostGuard() : base()
		{
			FightMode = FightMode.All | FightMode.Closest;

			InitStats( CoreAI.PostGuardStrength, 100, 100 );

			// Set the BroadSword damage
			SetDamage( 14, 25 );

			SetSkill( SkillName.Anatomy, CoreAI.PostGuardSkillLevel);
			SetSkill( SkillName.Tactics, CoreAI.PostGuardSkillLevel);
			SetSkill( SkillName.Swords, CoreAI.PostGuardSkillLevel);
			SetSkill( SkillName.MagicResist, CoreAI.PostGuardSkillLevel);
		}

		public AIPostGuard( Serial serial ) : base( serial )
		{
		}
		
		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( 12.0 ); } }
		public override int BandageMin{ get{ return 15; } }
		public override int BandageMax{ get{ return 30; } }


		public override bool OnBeforeDeath()
		{
			DropWeapon( 1, 1 );
			DropWeapon( 1, 1 );

			DropItem( new BagOfReagents( CoreAI.PostGuardNumRegDrop ) );
			DropItem( new Bandage( CoreAI.PostGuardNumBandiesDrop ) );
			DropItem( new ParalyzeScroll() ); 
			DropItem( new FireballScroll() ); 
			DropItem( new NightSightScroll() ); 

			return base.OnBeforeDeath();
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if ( DateTime.Now >= m_NextSpeechTime )
			{
				if ( m.Player && m.Alive && ((PlayerMobile)m).Inmate && m.Location != oldLocation && m.InRange(this, 8) )
				{
					switch ( Utility.Random( 5 ) )
					{
						case 0: 
						{
							this.Say( "Back to your cage wretched dog!" ); 
							break;
						}
						case 1: 
						{
							this.Say( "Thinking of escape eh?" );
							this.Say( "We�ll just see about that!" );
							break;
						}
						case 2:
						{
							this.Say( "*blows whistle*" );
							this.Say( "Escape! Escape!" );
							break;
						}
						case 3:
						{
							this.Say( "I see you�ve lost your way." );
							this.Say( "Shall I see you to the prison cemetery?" );
							break;
						}
						case 4:
						{
							this.Say( "Yes, run away!" );
							this.Say( "Ah, hah hah hah!" );
							break;
						}
					}
				
					m_NextSpeechTime = DateTime.Now + m_SpeechDelay;
				}
			}

			base.OnMovement( m, oldLocation );
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
	}
}
