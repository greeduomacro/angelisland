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

/* Scripts/Engines/ChampionSpawn/Modes/ChampRandom.cs
 *	ChangeLog:
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
	public class ChampRandom : ChampEngine
	{
		[Constructable]
		public ChampRandom() : base()
		{					
			// default constructor
			// assgin initial values
			PickChamp();
			//Switch graphics on
			Graphics = true;				

			// Switch restart on, 5 min delay
			m_bRestart = true;
			m_RestartDelay = TimeSpan.FromMinutes(5);
		}
		
		public void PickChamp()
		{
			// Randomly pick one of the 5 main big champs
			switch( Utility.Random(5) )
			{
				case 0:
				{
					SpawnType =  ChampLevelData.SpawnTypes.Abyss ; 
					break;
				}
				case 1:
				{
					SpawnType =  ChampLevelData.SpawnTypes.Arachnid ; 
					break;
				}
				case 2:
				{
					SpawnType =  ChampLevelData.SpawnTypes.ColdBlood ; 
					break;
				}
				case 3:
				{
					SpawnType =  ChampLevelData.SpawnTypes.UnholyTerror; 
					break;
				}
				case 4:
				{
					SpawnType =  ChampLevelData.SpawnTypes.VerminHorde; 
					break;
				}								
			}
		}

		public ChampRandom( Serial serial ) : base( serial )
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
		
		public  override void Restart()
		{			
			// When the champ re-starts, we want to choose a new champ first
			PickChamp();
			base.Restart();
		}		

		public override void OnSingleClick(Mobile from)
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
			{			
				// this is a gm, allow normal text from base and champ indicator
				LabelTo( from, "Random Champ" );
				base.OnSingleClick(from);
			}
		}
	}
}
