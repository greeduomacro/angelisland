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

// Engines/AngelIsland/ChestItemspawner.cs, last modified 2/28/05 by erlein.
// 02/28/05, erlein
//   Added logging of Count property change & deletion of ChestItemSpawner.
//   Now logs all changes to these in /logs/spawnerchange.log
// 4/23/04 Pulse
//	 Spawner no longer spawns a single random item each Spawn() attempt
//	   but instead spawns on of each item type listed in m_ItemsNames
//	 The limit of items in a chest is no longer m_Count items but is now
//     m_Count of each item type listed in m_ItemsNames
// 4/13/04 pixie
//   Removed a couple unnecessary checks that were throwing warnings.
// 4/11/04 pixie
//   Initial Revision.
// 4/06/04 Created by Pixie;

using System;
using System.IO;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Targeting;


namespace Server.Items
{
   public class ChestItemSpawner : Item
   {
	  private int m_Count;				//how much items to spawn
	  private TimeSpan m_MinDelay;		//min delay to respawn
	  private TimeSpan m_MaxDelay;		//max delay to respawn
	  private ArrayList m_ItemsName;	//list of item names
	  private ArrayList m_Items;		//list of items spawned
	  private DateTime m_End;			//time to next respawn
	  private InternalTimer m_Timer;	//internaltimer
      private bool m_Running;			//active ? 
	  private Container m_Container;	//container to spawn in

       
      public ArrayList ItemsName 
      { 
         get 
         { 
            return m_ItemsName; 
         } 
         set 
         { 
            m_ItemsName = value; 
            // If no itemname, no spawning 
            if ( m_ItemsName.Count < 1 ) 
               Stop(); 

            InvalidateProperties(); 
         } 
	  }

	  private Mobile m_LastProps;	// erl: added to hold who last opened props on it
	  public Mobile LastProps
	  {
		get
		{
			return m_LastProps;
		}
		set
		{
			if(value is PlayerMobile)
				m_LastProps = value;
		}
	  }


	  [CommandProperty( AccessLevel.GameMaster )]
	  public int Count
	  {
		 get
		 {
			return m_Count;
		 }
		 set
		 {
			if(m_Count!=value) {
				// erl: Log the change to Count
				LogChange("Count changed, " + m_Count + " to " + value);
			}
			m_Count = value;
			InvalidateProperties();
		 }
	  }


	  [CommandProperty( AccessLevel.GameMaster )]
	  public bool Running
      { 
         get 
         { 
            return m_Running; 
         } 
         set 
         { 
            if ( value ) 
               Start(); 
            else 
               Stop(); 

            InvalidateProperties(); 
         } 
      } 

       
      [CommandProperty( AccessLevel.GameMaster )] 
      public TimeSpan MinDelay 
      { 
         get 
         { 
            return m_MinDelay; 
         } 
         set 
         { 
            m_MinDelay = value; 
            InvalidateProperties(); 
         } 
      } 

       
      [CommandProperty( AccessLevel.GameMaster )] 
      public TimeSpan MaxDelay 
      { 
         get 
         { 
            return m_MaxDelay; 
         } 
         set 
         { 
            m_MaxDelay = value; 
            InvalidateProperties(); 
         } 
      } 

       
      [CommandProperty( AccessLevel.GameMaster )] 
      public TimeSpan NextSpawn 
      { 
         get 
         { 
            if ( m_Running ) 
               return m_End - DateTime.Now; 
            else 
               return TimeSpan.FromSeconds( 0 ); 
         } 
         set 
         { 
            Start(); 
            DoTimer( value ); 
         } 
      } 

	  [CommandProperty( AccessLevel.GameMaster )]
	  public Container SpawnContainer
	  {
		get
		{
			return m_Container;
		}
		set
		{
			m_Container = value;
		}
	  }



      [Constructable] 
      public ChestItemSpawner( int amount, int minDelay, int maxDelay, string itemName ) : base( 0x1f13 ) 
      { 
         ArrayList itemsName = new ArrayList(); 
         itemsName.Add( itemName.ToLower() ); 
         InitSpawn( amount, TimeSpan.FromMinutes( minDelay ), TimeSpan.FromMinutes( maxDelay ), itemsName ); 
      } 

       
      [Constructable] 
      public ChestItemSpawner( string itemName ) : base( 0x1f13 ) 
      { 
         ArrayList itemsName = new ArrayList(); 
         itemsName.Add( itemName.ToLower() ); 
         InitSpawn( 1, TimeSpan.FromMinutes( 20 ), TimeSpan.FromMinutes( 60 ), itemsName ); 
      } 

       
      [Constructable] 
      public ChestItemSpawner() : base( 0x1f13 ) 
      { 
         ArrayList itemsName = new ArrayList(); 
         InitSpawn( 1, TimeSpan.FromMinutes( 20 ), TimeSpan.FromMinutes( 60 ), itemsName ); 
      } 

       
      public ChestItemSpawner( int amount, TimeSpan minDelay, TimeSpan maxDelay, ArrayList itemsName ) : base( 0x1f13 ) 
      { 
         InitSpawn( amount, minDelay, maxDelay, itemsName ); 
      } 

       
      public void InitSpawn( int amount, TimeSpan minDelay, TimeSpan maxDelay, ArrayList itemsName ) 
      { 
         Visible = false; 
         Movable = true; 
         m_Running = true; 
         Name = "ChestItemSpawner"; 
         m_MinDelay = minDelay; 
         m_MaxDelay = maxDelay; 
         m_Count = amount; 
         m_ItemsName = itemsName; 
         m_Items = new ArrayList(); //create new list of creatures 
         DoTimer( TimeSpan.FromSeconds( 1 ) ); //spawn in 1 sec 
      } 
          
       
      public ChestItemSpawner( Serial serial ) : base( serial ) 
      { 
      } 

       
      public override void OnDoubleClick( Mobile from ) 
      { 
         ChestItemSpawnerGump g = new ChestItemSpawnerGump( this ); 
         from.SendGump( g ); 
      } 

       
      public override void GetProperties( ObjectPropertyList list ) 
      { 
         base.GetProperties( list ); 

         if ( m_Running ) 
         { 
            list.Add( 1060742 ); // active 

            list.Add( 1060656, m_Count.ToString() ); // amount to make: ~1_val~ 
            list.Add( 1060660, "speed\t{0} to {1}", m_MinDelay, m_MaxDelay ); // ~1_val~: ~2_val~ 
         } 
         else 
         { 
            list.Add( 1060743 ); // inactive 
         } 
      } 

       
      public override void OnSingleClick( Mobile from ) 
      { 
         base.OnSingleClick( from ); 

         if ( m_Running ) 
            LabelTo( from, "[Running]"); 
         else 
            LabelTo( from, "[Off]" ); 

		 
      } 

       
      public void Start() 
      { 
         if ( !m_Running ) 
         { 
            if ( m_ItemsName.Count > 0 ) 
            { 
               m_Running = true; 
               DoTimer(); 
            } 
         } 
      } 

       
      public void Stop() 
      { 
         if ( m_Running ) 
         { 
            m_Timer.Stop(); 
            m_Running = false; 
         } 
      } 

       
      public void Defrag() 
      { 
         bool removed = false; 

		 for ( int i = 0; i < m_Items.Count; ++i ) 
         { 
            object o = m_Items[i]; 

            if ( o is Item ) 
            { 
                
               Item item = (Item)o; 
                
               //if not in the original container or deleted -> delete from list 
               if(item.Deleted) 
               { 
                  m_Items.RemoveAt( i ); 
                  --i; 
                  removed = true; 
               } 
               else 
               { 
                   
                  if (item.Parent is Container) 
                  { 
                     Container par = (Container)item.Parent;
					  
                     if(this.m_Container != null) 
                     { 
                        Container cont = (Container)this.m_Container; 
                        if(((Item)cont).Serial != ((Item)par).Serial) 
                        { 
                           m_Items.RemoveAt( i ); 
                           --i; 
                           removed = true; 
                        }    
                     } 
                     else 
                     { 
                        m_Items.RemoveAt( i ); 
                        --i; 
                        removed = true; 
                      
                     } 
                  } 
                  else 
                  { 
                     m_Items.RemoveAt( i ); 
                     --i; 
                     removed = true; 
                      
                  } 

               } 
            } 
            else 
            { 
               //should not be something else 
               m_Items.RemoveAt( i ); 
               --i; 
               removed = true; 
            } 
         } 

         if ( removed ) 
            InvalidateProperties(); 
      } 

    
      public void OnTick() 
      { 
         DoTimer(); 
         Spawn(); 
      } 
       
      public void Respawn() 
      { 
		 try
		 {
	         RemoveItems(); 
	         for ( int i = 0; i < m_Count; i++ ) 
	            Spawn(); 
		 }
		 catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
      } 
       
    
      public void Spawn() 
      { 
		  // no longer spawns a single random item 
		  // but instead tries to spawn one of each item type
		  for (int i = 0; i < m_ItemsName.Count; i++)
		  {
			  Spawn( i );
		  }

	     // RANDOM ITEM SPAWNING(below) HAS BEEN REMOVED INDEFINATELY - REPLACED BY SPAWNING ONE
	     // ITEM OF EACH TYPE ON THE LIST AS CODED ABOVE
         //if there are item to spawn in list 
         //if ( m_ItemsName.Count > 0 ) 
         //  Spawn( Utility.Random( m_ItemsName.Count ) ); //spawn on of them index 
      } 
       
       
      public void Spawn( string itemName ) 
      { 
         for ( int i = 0; i < m_ItemsName.Count; i++ ) 
         { 
            if ( (string)m_ItemsName[i] == itemName ) 
            { 
               Spawn( i ); 
               break; 
            } 
         } 
      } 
       
       
      public void Spawn( int index ) 
      { 

         if ( m_ItemsName.Count == 0 || 
		      index >= m_ItemsName.Count  ||
			  m_Container == null ) 
            return; 

         Defrag(); 

         //limit already at for this type of item 
		 // (changed from m_Items.Count so that m_Count items of each type will spawn)
         if ( CountItems( (string)m_ItemsName[index] ) >= m_Count ) 
            return; 

         Type type = SpawnerType.GetType( (string)m_ItemsName[index] ); 

         if ( type != null ) 
         { 
            try 
            { 
               object o = Activator.CreateInstance( type ); 

               if ( o is Item ) 
               { 
				  if( m_Container != null )
                  { 
                     Item item = (Item)o;
                   
                     //add it to the list 
                     m_Items.Add( item ); 
                     InvalidateProperties(); 
                     //spawn it in the container 
                     Container cont = m_Container; 
                     cont.DropItem(item);
                  } 
               } 
            } 
            catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); } 
         } 
      } 

      public void DoTimer() 
      { 
         if ( !m_Running ) 
            return; 

         int minSeconds = (int)m_MinDelay.TotalSeconds; 
         int maxSeconds = (int)m_MaxDelay.TotalSeconds; 

         TimeSpan delay = TimeSpan.FromSeconds( Utility.RandomMinMax( minSeconds, maxSeconds ) ); 
         DoTimer( delay ); 
      } 

    
      public void DoTimer( TimeSpan delay ) 
      { 
         if ( !m_Running ) 
            return; 

         m_End = DateTime.Now + delay; 

         if ( m_Timer != null ) 
            m_Timer.Stop(); 

         m_Timer = new InternalTimer( this, delay ); 
         m_Timer.Start(); 
      } 

    
      private class InternalTimer : Timer 
      { 
         private ChestItemSpawner m_Spawner; 

         public InternalTimer( ChestItemSpawner spawner, TimeSpan delay ) : base( delay ) 
         { 
            Priority = TimerPriority.OneSecond; 
            m_Spawner = spawner; 
         } 

         protected override void OnTick() 
         { 
            if ( m_Spawner != null ) 
               if ( !m_Spawner.Deleted ) 
                  m_Spawner.OnTick(); 
         } 
      } 

       
      public int CountItems( string itemName ) 
      { 
          
         Defrag(); 

         int count = 0; 

         for ( int i = 0; i < m_Items.Count; ++i ) 
            if ( Insensitive.Equals( itemName, m_Items[i].GetType().Name ) ) 
               ++count; 

         return count; 
      } 

       
      public void RemoveItems( string itemName ) 
      { 
         //Console.WriteLine( "defrag from removeitems" ); 
         Defrag(); 

         itemName = itemName.ToLower(); 

         for ( int i = 0; i < m_Items.Count; ++i ) 
         { 
			object o = m_Items[i];

			if ( Insensitive.Equals( itemName, o.GetType().Name ) )
			{
			   if ( o is Item )
				  ((Item)o).Delete();

			}
		 }

		 InvalidateProperties();
	  }

	  public void RemoveItems()
	  {

		 Defrag();

		 for ( int i = 0; i < m_Items.Count; ++i )
		 {
			object o = m_Items[i];

			if ( o is Item )
			   ((Item)o).Delete();

		 }

		 InvalidateProperties();
	  }


	  // erl: for change logging!

	  public void LogChange(string changemade)
	  {
		if(changemade == "")
			return;

		StreamWriter LogFile = new StreamWriter( "logs/spawnerchange.log", true );

		string strAcc = "";

		if(m_LastProps is PlayerMobile)
			strAcc = m_LastProps.Account.ToString();
		else
			strAcc = "SYSTEM";

		LogFile.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", DateTime.Now, strAcc, this.Location.X, this.Location.Y, this.Location.Z, changemade);
		LogFile.Close();
	  }


	  public override void OnDelete()
	  {
		 // erl: Log the fact it's been deleted
		 LogChange("Spawner deleted");

		 base.OnDelete();
		 try
		 {
	         RemoveItems(); 
		 }
		 catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
         if ( m_Timer != null ) 
            m_Timer.Stop(); 
      } 
       
      public override void Serialize( GenericWriter writer ) 
      { 
         base.Serialize( writer ); 

         writer.Write( (int) 0 ); // version 

		 writer.Write( m_Container );
         writer.Write( m_MinDelay ); 
         writer.Write( m_MaxDelay ); 
         writer.Write( m_Count ); 
         writer.Write( m_Running ); 
          
         if ( m_Running ) 
            writer.Write( m_End - DateTime.Now ); 

         writer.Write( m_ItemsName.Count ); 

         for ( int i = 0; i < m_ItemsName.Count; ++i ) 
            writer.Write( (string)m_ItemsName[i] ); 

         writer.Write( m_Items.Count ); 

         for ( int i = 0; i < m_Items.Count; ++i ) 
         { 
            object o = m_Items[i]; 

            if ( o is Item ) 
               writer.Write( (Item)o ); 
            else 
               writer.Write( Serial.MinusOne ); 
         } 
      } 

      private static WarnTimer m_WarnTimer; 

      public override void Deserialize( GenericReader reader ) 
      { 
         base.Deserialize( reader ); 

         int version = reader.ReadInt(); 
          
		 m_Container = reader.ReadItem() as Container;
         m_MinDelay = reader.ReadTimeSpan(); 
         m_MaxDelay = reader.ReadTimeSpan(); 
         m_Count = reader.ReadInt(); 
         m_Running = reader.ReadBool(); 

         if ( m_Running ) 
         { 
            TimeSpan delay = reader.ReadTimeSpan(); 
            DoTimer( delay ); 
         } 
                
         int size = reader.ReadInt(); 
    
         m_ItemsName = new ArrayList( size ); 

         for ( int i = 0; i < size; ++i ) 
         { 
            string typeName = reader.ReadString(); 

            m_ItemsName.Add( typeName ); 

            if ( ChestItemSpawnerType.GetType( typeName ) == null ) 
            { 
               if ( m_WarnTimer == null ) 
                  m_WarnTimer = new WarnTimer(); 

               m_WarnTimer.Add( Location, Map, typeName ); 
            } 
         } 

         int count = reader.ReadInt(); 

         m_Items = new ArrayList( count ); 

         for ( int i = 0; i < count; ++i ) 
         { 
            IEntity e = World.FindEntity( reader.ReadInt() ); 

            if ( e != null ) 
               m_Items.Add( e ); 
         } 
      } 

      private class WarnTimer : Timer 
      { 
         private ArrayList m_List; 

         private class WarnEntry 
         { 
            public Point3D m_Point; 
            public Map m_Map; 
            public string m_Name; 

            public WarnEntry( Point3D p, Map map, string name ) 
            { 
               m_Point = p; 
               m_Map = map; 
               m_Name = name; 
            } 
         } 

         public WarnTimer() : base( TimeSpan.FromSeconds( 1.0 ) ) 
         { 
            m_List = new ArrayList(); 
            Start(); 
         } 

         public void Add( Point3D p, Map map, string name ) 
         { 
            m_List.Add( new WarnEntry( p, map, name ) ); 
         } 

         protected override void OnTick() 
         { 
            try 
            { 
               Console.WriteLine( "Warning(ChestItemspawner.cs:WarnTimer): {0} bad spawns detected, logged: 'badspawn.log'", m_List.Count ); 

               using ( StreamWriter op = new StreamWriter( "badspawn.log", true ) ) 
               { 
                  op.WriteLine( "# Bad spawns : {0}", DateTime.Now ); 
                  op.WriteLine( "# Format: X Y Z F Name" ); 
                  op.WriteLine(); 

                  foreach ( WarnEntry e in m_List ) 
                     op.WriteLine( "{0}\t{1}\t{2}\t{3}\t{4}", e.m_Point.X, e.m_Point.Y, e.m_Point.Z, e.m_Map, e.m_Name ); 

                  op.WriteLine(); 
                  op.WriteLine(); 
               } 
            } 
            catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); } 
         } 
      } 
   } 
} 
