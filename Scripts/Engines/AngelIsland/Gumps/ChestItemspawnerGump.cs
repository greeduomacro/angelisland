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

// Engined/AngelIsland/ChestItemSpawnerGump.cs, last modified 2/28/05 by erlein.
// 2/28/04 erlein
//   Added change logging for all alterations to spawn list.
//   Now logs to /logs/spawnerchange.log
// 4/11/04 pixie
//   Initial Revision.
// 4/06/04 Created by Pixie;

using System; 
using System.Collections; 
using Server.Network; 
using Server.Gumps; 
using Server.Targeting;
using System.IO;
using Server.Mobiles;

namespace Server.Items 
{ 

   public class ChestItemSpawnerType 
   { 
      public static Type GetType( string name ) 
      { 
         return ScriptCompiler.FindTypeByName( name ); 
      } 
   } 


   public class ChestItemSpawnerGump : Gump 
   { 
	  private static ChestItemSpawner m_Spawner;
	  private static Mobile m_Person;

	  private SpawnerMemory m_SpawnBefore;
	  private SpawnerMemory m_SpawnAfter;

	  public static ChestItemSpawner LinkedSpawner
	  {
			get{ return m_Spawner; }
	  }

	  public static Mobile LinkedPerson
	  {
		get
		{
			return m_Person;
		}
		set
		{
			if(value is PlayerMobile)
				m_Person = value;
		}
	  }


      public ChestItemSpawnerGump( ChestItemSpawner spawner ) : base( 50, 50 ) 
      { 
		 m_Spawner = spawner;

		 m_SpawnBefore = new SpawnerMemory();
		 m_SpawnAfter = new SpawnerMemory();

         AddPage( 0 ); 

         AddBackground( 0, 0, 260, 371, 5054 ); 

         AddLabel( 95, 1, 0, "Items List" ); 

         AddButton( 5, 347, 0xFB1, 0xFB3, 0, GumpButtonType.Reply, 0 ); 
         AddLabel( 38, 347, 0x384, "Cancel" ); 

         AddButton( 5, 325, 0xFB7, 0xFB9, 1, GumpButtonType.Reply, 0 ); 
         AddLabel( 38, 325, 0x384, "Okay" ); 

         AddButton( 110, 347, 0xFA8, 0xFAA, 2, GumpButtonType.Reply, 0 ); 
         AddLabel( 143, 347, 0x384, "Total Respawn" ); 

 		 AddButton( 110, 325, 0xFA8, 0xFAA, 3, GumpButtonType.Reply, 0 ); 
         AddLabel( 143, 325, 0x384, "Set Container" ); 


         for ( int i = 0;  i < 13; i++ ) 
         { 
            AddButton( 5, ( 22 * i ) + 20, 0xFA5, 0xFA7, 4 + (i * 2), GumpButtonType.Reply, 0 ); 
			AddButton( 38, ( 22 * i ) + 20, 0xFA2, 0xFA4, 5 + (i * 2), GumpButtonType.Reply, 0 );

            AddImageTiled( 71, ( 22 * i ) + 20, 159, 23, 0x52 ); 
            AddImageTiled( 72, ( 22 * i ) + 21, 157, 21, 0xBBC ); 

            string str = ""; 

            if ( i < spawner.ItemsName.Count ) 
            { 
			   str = (string)spawner.ItemsName[i];
			   int count = m_Spawner.CountItems( str );

			   if(str!="")
					m_SpawnBefore.Add(str);

               AddLabel( 232, ( 22 * i ) + 20, 0, count.ToString() ); 
            } 

            AddTextEntry( 75, ( 22 * i ) + 21, 154, 21, 0, i, str ); 
         } 
      } 

      public ArrayList CreateArray( RelayInfo info, Mobile from ) 
      { 
         ArrayList itemsName = new ArrayList(); 

         for ( int i = 0;  i < 13; i++ ) 
         { 
            TextRelay te = info.GetTextEntry( i ); 

            if ( te != null ) 
            { 
               string str = te.Text; 

               if ( str.Length > 0 ) 
               { 
                  str = str.Trim(); 

                  Type type = ChestItemSpawnerType.GetType( str ); 
                   
                   
				  if ( type != null ) {
					 itemsName.Add( str );
					 m_SpawnAfter.Add(str);
				  }
                  else 
                     from.SendMessage( "{0} is not a valid type name.", str ); 
               } 
            } 
         } 

         return itemsName; 
	  }

	  public override void OnResponse( NetState state, RelayInfo info )
	  {

		 LinkedPerson = state.Mobile; // erl: link person responding to spawner

		 if ( m_Spawner.Deleted )
			return;

         switch ( info.ButtonID ) 
         { 
            case 0: // Closed 
            { 
               break; 
            } 
            case 1: // Okay 
			{
			   m_Spawner.ItemsName = CreateArray( info, state.Mobile );

			   // erl: compare the two lists of spawner creatures and
			   // log any changes

			   m_SpawnBefore.Compare(m_SpawnAfter);

			   break;
			}
			case 2: // Complete respawn
			{
			   m_Spawner.Respawn();

			   break;
			}
			case 3: //Set Container
			{
				//bring up target cursor
				state.Mobile.SendMessage("Target Container to spawn into");

				state.Mobile.Target = new ContainerTarget(m_Spawner);

				break;
			}
			default:
			{
			   int buttonID = info.ButtonID - 4;
			   int index = buttonID / 2;
			   int type = buttonID % 2;

			   TextRelay entry = info.GetTextEntry( index );

			   if ( entry != null && entry.Text.Length > 0 )
			   {
				  if ( type == 0 ) // Spawn item
					 m_Spawner.Spawn( entry.Text );
				  else // Remove items
					 m_Spawner.RemoveItems( entry.Text );

				  m_Spawner.ItemsName = CreateArray( info, state.Mobile );
			   }

			   // erl: compare the two lists of spawner creatures and
			   // log any changes

			   m_SpawnBefore.Compare(m_SpawnAfter);

			   break;
			}
		 }
	  }


		private class ContainerTarget : Target
		{
			ChestItemSpawner m_spawner;

			public ContainerTarget(ChestItemSpawner spawner) : base( -1, true, TargetFlags.None )
			{
				m_spawner = spawner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if( o is Container )
				{
					m_spawner.SpawnContainer = (Container)o;
					from.SendMessage( string.Format("Assigning {0} to spawner", o) );
				}
				else
				{
					from.SendMessage("Target needs to be a container!!");
				}
				from.SendGump( new ChestItemSpawnerGump( m_spawner ) );
			}
		}


		// erl: SpawnerMemory class to hold, compare and log changes
		//
		// 02/28/05

		private class SpawnerMemory
		{
			// Holds creature info.

			public string[] m_Names;
			public int[] m_Counts;
			public bool[] m_IsChecked;

			private ChestItemSpawner m_Spawner;

			// Add to or create entry in memory
			
			public void Add (string name)
			{
         		int cpos;

				for(cpos=0; cpos<13; cpos++) {

					if(m_Names[cpos] == null)
						break;

					if(m_Names[cpos] == name) {
						m_Counts[cpos]++;
						return;
					}
				}

				if(cpos == 13)		// Should never happen
					return;

				m_Names[cpos] = name;
				m_Counts[cpos] = 1;
			}

			// Return any instances of name type passed stored

			public int Retrieve(string name)
			{

				for(int cpos=0; cpos<13; cpos++) {

					if(m_Names[cpos] == null)
						return 0;

					if(m_Names[cpos] == name) {
						m_IsChecked[cpos] = true;
						return m_Counts[cpos];
					}
				}

				return(0);
			}

			// Instance the SpawnerMemory, generating start creature
			// list

			public SpawnerMemory()
			{
				m_Spawner = LinkedSpawner;

				m_Names = new string[13];
				m_Counts = new int[13];
				m_IsChecked = new bool[13];
			}

			// Compare SpawnerMemory passed with this instance
			// and logs any changes to file

			public void Compare(SpawnerMemory sm)
			{
				ArrayList DiffList = new ArrayList();

				for(int i=0; i < 13; i++) {
					if(sm.Retrieve(m_Names[i])!=Retrieve(m_Names[i]))
 						DiffList.Add(m_Names[i] + " changed, " + Retrieve(m_Names[i]) + " to " + sm.Retrieve(m_Names[i]));
				}

				for(int i=0; i < 13; i++) {
					if(!sm.m_IsChecked[i] && sm.m_Names[i]!=null)
						DiffList.Add(sm.m_Names[i] + " changed, 0 to " + sm.m_Counts[i]);
				}

				if(DiffList.Count > 0) {

					// There are differences, so log them!

					StreamWriter LogFile = new StreamWriter( "logs/spawnerchange.log", true );
					
					foreach(string difference in DiffList) {
						// Log entries here
						LogFile.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", DateTime.Now, LinkedPerson.Account, m_Spawner.Location.X, m_Spawner.Location.Y, m_Spawner.Location.Z, difference);
					}

					LogFile.Close();

				}

				return;
			}

		}


   }

}
