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

/* Scripts/Engines/ChampionSpawn/Modes/ChampMini.cs
 *	ChangeLog:
 *	11/01/2006, plasma
 *		Added WipeMonsters() back in due to ebb and flow system
 *	10/29/2006, plasma
 *		Removed WipeMonsters() call as added to core engine
 *	10/29/2006, plasma
 *		Added WipeMonsters() call in AdvanceLevel()
 *	10/28/2006, plasma
 *		Initial creation
 * 
 **/
using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.ChampionSpawn
{	
	// Mini champ spawns.  These are represented by a lever which is thrown to activate the spawn
	public class ChampMini : ChampEngine
	{
		[Constructable]
		public ChampMini() : base()
		{					
			// change the sign graphic to a lever (off position)
			ItemID = 0x108d;
			// and make visible
			Visible=true;
			//asign first mini spawn as default
			SpawnType = ChampLevelData.SpawnTypes.Mini_Deceit;
		}		

		public ChampMini( Serial serial ) : base( serial )
		{
		}

		// #region serialize

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize (writer);

			writer.Write( (int) 0 );
			
		}
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:break;
			}
		}
		// #endregion
		
		protected override void Activate()
		{
			//make sure our lever gets updated on Active prop change
			base.Activate ();
			if( m_bActive )
				this.ItemID = 0x108c; // on position
			else
					this.ItemID = 0x108d;  // off position
		}

		protected override void Expire()
		{
			if( Level == 0 )
			{
				//for a mini champ, a level down from 0 switchs off the champ
				double f =((double) Kills) / ((double)Lvl_MaxKills);
				if ( f*100 < 20 )
				{
					// They didn't even get 20% of level 0, stop champ totally	!
					Kills = 0;
					StopSlice();
					WipeMonsters();
					Active=false;
					InvalidateProperties();
				}
				else
				{
					Kills=0;
				}
				m_ExpireTime = DateTime.Now + Lvl_ExpireDelay;
			}
			else
				base.Expire ();
		}
		
		protected override void AdvanceLevel()
		{
			// if the champ has just completed, set the lever back!
			if( IsFinalLevel )			
				this.ItemID = 0x108d;				

			//wipe the spawn on level up
			WipeMonsters();
			// call base
			base.AdvanceLevel();
			if( m_Type == ChampLevelData.SpawnTypes.Mini_Wrong )
			{
				// wrong speaks!
				switch ( Level )
				{
					case 1: PublicOverheadMessage( 0, 0x3B2, false, "The host of deformities thickens." ); break;
					case 2: PublicOverheadMessage( 0, 0x3B2, false, "Magical abominations rise from the Beast's Lair." ); break;
					case 3: PublicOverheadMessage( 0, 0x3B2, false, "The Beast Beckons!" ); break;
				}
			}
		}


		public override void OnSingleClick(Mobile from)
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
			{				
				// this is a gm, just allow normal text from base
				LabelTo( from, "Mini Champ" );
				base.OnSingleClick(from);
			}
			else
			{
				if(m_bActive)
					LabelTo( from, "a stuck, rusty lever" );
				else
					LabelTo( from, "a rusty lever" );
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
			{
				// this isn't a player, so just show the props to them!
				base.OnDoubleClick(from);
			}
			else
			{
				if( from.InRange( this, 2 ) )
				{
					// otherwise, we need to flick the lever if the spawn isn't
					// already active, and start a spawn.
					if( !m_bActive )
					{
						// swap graphic
						this.ItemID = 0x108c;
						// start champ
						Active = true;
					}
					else
					{
						// the champ is already on, display message indicating stuck lever!
						from.SendMessage( "The lever seems to be stuck!");
					}
				}
				else
					from.SendMessage("You are not close enough to use that");
			}
		}

	}
}
