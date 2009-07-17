/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property 
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution 
and
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
 */

/* /Scripts/Items/Misc/GrapplingHook.cs
 * ChangeLog :
 *  07/02/07, plasma
 *      Remove now redundant IsOnDeck call which we don't want as grapple can be used on any part of a victim's boat
 *  4/3/07, Adam
 *      Ignore staff when tele'ing to another boat
 *  4/2/07, Adam
 *      Add a check in targeting to check for 'Wet' tiles to prevent targeting water that may appear included
 *      in the Multis rect.
 *	03/12/07, plasma
 *		Initial creation.
 */

using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
    public class GrapplingHook : Item
    {
        
        [Constructable]
        public GrapplingHook()
            : base(0x14F8)
        {       
            Name = "a grappling hook";            
        }

        public GrapplingHook(Serial serial)
            : base(serial)
        {
        }


        public override void OnDoubleClick(Mobile from)        
        {
            //wooo sanity!
            if (Deleted)
                return;

            //Has to be in backpack and on a boat!
            if (!this.IsChildOf(from.Backpack))
            {
                from.SendMessage("That must be in your backpack to use it.");
                return;
            }
            BaseBoat bt = BaseBoat.FindBoatAt(from);
            if (bt == null || !bt.IsOnDeck(from) )       
            {               
               from.SendMessage("You must be on a boat to use this.");
               return;               
            }
            //Begin tarzan! 
            from.SendMessage("Target the boat you wish to grapple.");
            from.Target = new InternalTarget( this );
        }
        
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        
        }
        
        private class InternalTarget : Target
        {
            private GrapplingHook m_Hook;

            public InternalTarget( GrapplingHook g )
                : base(10, false, TargetFlags.None)
            {
                m_Hook = g;                
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                BaseBoat bt = null;
                Point3D loc = new Point3D();

                //Check we have a hook
                if (m_Hook == null)
                    return;

                //first do all our validation *again* to thwart sneaky players!
                //Has to be in backpack and on a boat
                if (!m_Hook.IsChildOf(from.Backpack))
                {
                    from.SendMessage("That must be in your backpack to use it.");
                    return;
                }
                bt = BaseBoat.FindBoatAt(from);
                if (bt == null || !bt.IsOnDeck(from))       
                {
                    from.SendMessage("You must be on a boat to use this.");
                    return;
                }
                bt = null;

                //Disallow the targetting of movable Items
                if (targeted is Item)
                {
                    Item i  = (Item)targeted;
                    if (i.Movable == true)
                    {
                        from.SendMessage("You must grapple onto something more sturdy!");
                        return;
                    }
                    //this item is immovable, see if there's a boat at its location
                    bt = BaseBoat.FindBoatAt(i);
                    loc = i.Location; 
                } // A static target is a good candidate
                else if (targeted is StaticTarget )
                {
                    //see if there's a boat at its location
                    StaticTarget st = (StaticTarget)targeted;

                    // Boats seem to have both TileFlag.Surface and TileFlag.Impassable flags on different parts of the deck
                    if ((st.Flags & TileFlag.Wet) == 0)
                    {
                        bt = BaseBoat.FindBoatAt(st.Location, from.Map, 16);
                        loc = st.Location;
                    }
                }

                //If bt is still null, something was targeted not on a ship.
                if (bt != null)
                {
                    //Prevent targeting their own boat
                    if (bt.IsOnDeck(from))
                    {
                        from.SendMessage("You can't grapple the boat you are on!");
                        return;
                    } 
    
                        if (bt.IsMoving)
                        {
                            //Play a "rope being thrown"  type sound effect (bellows :P)
                            Effects.PlaySound(from.Location, from.Map, 0x2B2);
                            from.SendMessage("You fail to get a good hook into the boat.");
                            return;
                        }

                        //If there any players who are alive then the hook gets pusehd back in ye olde water..
                        foreach (Mobile m in bt.GetMobilesOnDeck())
                            if (m is PlayerMobile && m.Alive && m.AccessLevel <= AccessLevel.Player)
                            {
                                from.SendMessage("You grapple the boat, but the hook is tossed back into the water.");
                                //Play a random water splashy sound effect!
                                Effects.PlaySound(from.Location, from.Map, 36 + Utility.Random(3)+1);
                                return;
                            }

                        //Success!
                        from.SendMessage("You grapple the boat!");
                        //This is the bellows and double arrow hit played together :D
                        Effects.PlaySound(from.Location, from.Map, 0x2B2);
                        Effects.PlaySound(from.Location, from.Map, 0x523);                                                
                        //Consume the grappling hook
                        m_Hook.Consume();
                        //Start the timer (approx the same time as the sound effects finish)
                        InternalTimer t = new InternalTimer(from, bt);
                        t.Start();
                }
                else
                    from.SendMessage("That's not a boat!");
            }
            /// <summary>
            /// Teleport timer!
            /// </summary>
            private class InternalTimer : Timer
            {
                private Mobile m_Mobile;        //Mobly
                private BaseBoat m_Boat;        //Boat
           
                public InternalTimer(Mobile m, BaseBoat boat)
                    : base(TimeSpan.FromSeconds(2.0))
                { //assign initial vals..
                    m_Mobile = m;
                    m_Boat = boat;             
                }

                protected override void OnTick()
                {
                    if (m_Mobile == null || m_Boat == null || m_Boat.Map == Map.Internal)
                        return;
                    //Get spawn location on the deck
                    Point3D loc = m_Boat.FindSpawnLocationOnDeck();
                    //Check there is a valid spawn location.
                    //NOTE: 99.99% of the time there will always be at least one spawn location free (next to the tillerman)
                    //The only expception is if every location is blocked with an imapssable object, which is highly unlikley as you
                    //can't grapple a boat that has living players on it, and you have to use the overload bug to get an item next to the tillerman.
                    //I'll put this check in away though, as stranger things have happened! :)
                    if (loc == new Point3D())
                    {
                        m_Mobile.SendMessage("You swing across to the boat, but are unable to get a good footing upon the deck!");
                        return;
                    }
                    //Finally move the player
                    m_Mobile.MoveToWorld(loc, m_Boat.Map);
                }
            }
        }
    }
}