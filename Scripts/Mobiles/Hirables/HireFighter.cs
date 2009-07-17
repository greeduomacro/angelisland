/* *	This program is the CONFIDENTIAL and PROPRIETARY property 
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
 * 7/22/04 - Old Salty:  Fighter now computes his "price" on being born, so you can't hire them for 1 gp.
 * 6/12/04 - Old Salty:  Changed "ActiveSpeed" so that these guys aren't as fast
 * 
 */

using System; 
using System.Collections; 
using Server.Items; 
using Server.ContextMenus; 
using Server.Misc; 
using Server.Network; 

namespace Server.Mobiles 
{ 
    public class HireFighter : BaseHire 
    { 
        [Constructable] 
        public HireFighter()
        { 
            SpeechHue = Utility.RandomDyedHue(); 
            Hue = Utility.RandomSkinHue(); 
        	this.Payday( this );
        		

            if ( this.Female = Utility.RandomBool() ) 
            { 
                Body = 0x191; 
                Name = NameList.RandomName( "female" ); 
            } 
            else 
            { 
                Body = 0x190; 
                Name = NameList.RandomName( "male" ); 
            } 
            ActiveSpeed = 0.30;
            
		Title = "the fighter";
            Item hair = new Item( Utility.RandomList( 0x203B, 0x2049, 0x2048, 0x204A ) ); 
            hair.Hue = Utility.RandomNeutralHue(); 
            hair.Layer = Layer.Hair; 
            hair.Movable = false; 
            AddItem( hair ); 

            if( Utility.RandomBool() && !this.Female )
            {
                Item beard = new Item( Utility.RandomList( 0x203E, 0x203F, 0x2040, 0x2041, 0x204B, 0x204C, 0x204D ) );

                beard.Hue = hair.Hue;
                beard.Layer = Layer.FacialHair;
                beard.Movable = false;

                AddItem( beard );
            }

            SetStr( 91, 91 ); 
            SetDex( 91, 91 ); 
            SetInt( 50, 50 ); 

            SetDamage( 7, 14 ); 

			SetSkill( SkillName.Tactics, 50, 60 );			
			SetSkill( SkillName.Anatomy, 50, 60 );
			SetSkill( SkillName.Parry, 50, 60 );
						
			
			switch( Utility.Random( 4 ) ) //pick what type of fighter they will be
			{
				case 0: //sword fighter
					MakeSwordsman();
					break;
				case 1: //mace fighter
					MakeMacer();
					break;
				case 2: //fencer
					MakeFencer();
					break;
				case 3: //wrestler
				    SetSkill( SkillName.Wrestling, 40, 80 );
					break;
			}
			
			if ( HasFreeHand() && Utility.RandomBool() )
				AddShield();

            Fame = 100; 
            Karma = 100; 

            AddItem( new Shoes( Utility.RandomNeutralHue() ) ); 
            AddItem( new Shirt());          

            // Pick some armor
            switch( Utility.Random( 5 ) )
            {
                case 0: // Leather
                    AddItem( new LeatherChest() );
                    AddItem( new LeatherArms() );
                    AddItem( new LeatherGloves() );
                    AddItem( new LeatherGorget() );
                    AddItem( new LeatherLegs() );
					AddHelm();
                    break;

                case 1: // Studded Leather
                    AddItem( new StuddedChest() );
                    AddItem( new StuddedArms() );
                    AddItem( new StuddedGloves() );
                    AddItem( new StuddedGorget() );
                    AddItem( new StuddedLegs() );
            		AddHelm();
                    break;

                case 2: // Ringmail
                    AddItem( new RingmailChest() );
                    AddItem( new RingmailArms() );
                    AddItem( new RingmailGloves() );
                    AddItem( new RingmailLegs() );
            		AddHelm();
                    break;

                case 3: // Chain
                    AddItem( new ChainChest() );
                    AddItem( new ChainCoif() );
                    AddItem( new ChainLegs() );
                    break;
            	
            	case 4: // Plate
            		AddItem( new PlateChest() );
            		AddItem( new PlateArms() );
            		AddItem( new PlateGloves() );
            		AddItem( new PlateGorget() );
            		AddItem( new PlateLegs() );
            		AddHelm();
            		break;
            }
        } 
	public override bool ClickTitle{ get{ return false; } }
        public HireFighter( Serial serial ) : base( serial ) 
        { 
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
        
        public void AddHelm()
    	{
    		switch( Utility.Random( 6 ) )
				{
					case 0: break;
					case 1: AddItem( new Bascinet() ); break;
					case 2: AddItem( new CloseHelm() ); break;
					case 3: AddItem( new NorseHelm() ); break;
					case 4: AddItem( new Helmet() ); break;
					case 5: AddItem( new PlateHelm() ); break;
				}
    	}
    	
    	public void MakeSwordsman()
    	{
    		SetSkill( SkillName.Swords, 40, 80 );
    		
            switch ( Utility.Random( 4 )) // Pick a random sword
            { 
                case 0: AddItem( new Longsword() ); break;
                case 2: AddItem( new VikingSword() ); break; 
            	case 3: AddItem( new BattleAxe() ); break;
    			case 4: AddItem( new TwoHandedAxe() ); break;
            } 
    	}
    	
    	public void MakeMacer()
    	{
    		SetSkill( SkillName.Macing, 40, 80 );
    		
    		switch ( Utility.Random( 4 )) //Pick a random mace 
    		{
    			case 0: AddItem( new Club() ); break;
    			case 1: AddItem( new WarAxe() ); break;
    			case 2: AddItem( new WarHammer() ); break;
    			case 3: AddItem( new QuarterStaff() ); break;
    		}
    	}
    	public void MakeFencer()
    	{
    		SetSkill( SkillName.Fencing, 40, 80 );
    		
    		switch ( Utility.Random( 4 ))  //Pick a random fencing wep
    		{
    			case 0: AddItem( new WarFork() ); break;
    			case 1: AddItem( new Kryss() ); break;
    			case 2: AddItem( new Spear() ); break;
    			case 3: AddItem( new ShortSpear() ); break;
    		}
    	}
    	
    	public void AddShield() 
    	{	
            switch ( Utility.Random( 6 )) // Pick a random shield
            { 
                case 0: AddItem( new BronzeShield() ); break; 
                case 1: AddItem( new HeaterShield() ); break; 
                case 2: AddItem( new MetalKiteShield() ); break; 
                case 3: AddItem( new MetalShield() ); break; 
                case 4: AddItem( new WoodenKiteShield() ); break; 
                case 5: AddItem( new WoodenShield() ); break; 
            } 
    	}
    	
    } 
} 
