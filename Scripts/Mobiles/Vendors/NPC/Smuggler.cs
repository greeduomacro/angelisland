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

/* /Scripts/Mobiles/Vendors/NPC/Smuggler.cs
 * ChangeLog
 *  05/02/05, Kit
 *	Fixed problem with smugglers only haveing a title
 *  10/18/04, Froste
 *      Modified Restock to use OnRestock() because it's fixed now
 *  10/11/04, Froste
 *      Created this modified version of Importer.cs
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	4/29/04, mith
 *		Modified Restock to use OnRestockReagents() to restock 100 of each item instead of only 20.
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
	public class Smuggler : BaseVendor
	{
        public ArrayList m_SBInfos = new ArrayList();
        protected override ArrayList SBInfos { get { return m_SBInfos; } }
        
        //public override NpcGuild NpcGuild{ get{ return NpcGuild.MagesGuild; } }

		[Constructable]
		public Smuggler() : base("the smuggler")
		{
/*			SetSkill( SkillName.EvalInt, 65.0, 88.0 );
 *			SetSkill( SkillName.Inscribe, 60.0, 83.0 );
 *			SetSkill( SkillName.Magery, 64.0, 100.0 );
 *			SetSkill( SkillName.Meditation, 60.0, 83.0 );
 *			SetSkill( SkillName.MagicResist, 65.0, 88.0 );
 *			SetSkill( SkillName.Wrestling, 36.0, 68.0 );
 */		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add( new SBSmuggler() );
		}

/*		public override VendorShoeType ShoeType
 *		{
 *			get{ return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals; }
 *		}
 */
        public override void InitBody()
        {

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
            }
            else
            {
                Body = 0x190;
            }

            InitStats(100, 100, 25);

            SpeechHue = Utility.RandomDyedHue();
            

            if (IsInvulnerable && !Core.AOS)
                NameHue = 0x35;

	    if (this.Female)
	    {
		// "Lizzie" "the smuggler"
		Name = NameList.RandomName("pirate_female");
	    }
	    else
	    {
		Name = NameList.RandomName("pirate_male");
	    }
			
           
            Hue = 0x4001;



        }

        public override void InitOutfit()
        {
            switch (Utility.Random(2))
            {
                case 0: AddItem(new SkullCap(Utility.RandomRedHue())); break;
                case 1: AddItem(new TricorneHat(Utility.RandomRedHue())); break;
            }


            if (Utility.RandomBool())
            {
                Item shirt = new Shirt(Utility.RandomRedHue());
                AddItem(shirt);
            }

            Item sash = new BodySash(0x85);
            Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2049, 0x204A));
            Item pants = new LongPants(Utility.RandomRedHue());
            Item boots = new Boots(Utility.RandomRedHue());
            hair.Hue = Utility.RandomHairHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;

            AddItem(hair);
            AddItem(sash);
            AddItem(pants);
            AddItem(boots);

            if (!this.Female)
            {
                Item beard = new Item(Utility.RandomList(0x203E, 0x203F, 0x2040, 0x2041, 0x204B, 0x204C, 0x204D));
                beard.Hue = hair.Hue;
                beard.Layer = Layer.FacialHair;
                beard.Movable = false;
                AddItem(beard);
            }

        }        
        
        public override void Restock()
		{
			base.LastRestock = DateTime.Now;

			IBuyItemInfo[] buyInfo = this.GetBuyInfo();

			foreach ( IBuyItemInfo bii in buyInfo )
                bii.OnRestock(); // change bii.OnRestockReagents() to OnRestock()
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.InRange(this.Location, 2))
                return true;

            return base.HandlesOnSpeech(from);
        }

  /*      public override void OnSpeech(SpeechEventArgs e)
   *    {
   *        base.OnSpeech( e );
   *         this.Say("Leave these halls before it is too late!");
   *    }
   */

        public override void GetContextMenuEntries(Mobile from, ArrayList list)
        {
            base.GetContextMenuEntries(from, list);

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] is ContextMenus.VendorBuyEntry)
                    list.RemoveAt(i--);
                else if (list[i] is ContextMenus.VendorSellEntry)
                    list.RemoveAt(i--);
            }
        }

        public Smuggler( Serial serial ) : base( serial )
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

    }
}