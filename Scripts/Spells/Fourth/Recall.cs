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

/* Scripts\Spells\Fourth\Recall.cs
 * ChangeLog:
 *  3/28/07, Adam
 *      make calls to IsSpecial(loc) a normal check instead of 'send to jail'
 *  03/28/07, plasma,
 *      Prevent recall to boats
 *	2/28/06, Adam
 *		If you steal at all, you are now bound by the 2 minute timer
 *		i.e., Thou'rt a criminal and cannot escape so easily.
 *	2/12/06, Adam
 *		because we delay before the actual teleport, we should recheck to make sure we havn't done 
 *		anything funky like looted someone: call m_Spell.CheckCast() in OnTick()
 *	7/23/05, Adam
 *		Remove all Necromancy, and Chivalry nonsense
 *	3/7/05, Adam
 *		Look for the recall exploit and send them to jail
 *		If we are in the teleport phase of recall and inmate == true, they
 *		are exploiting.
 *		Also: hook this up to the global InmateRecallExploitCheck flag so we can 
 *		turn it on off.
 *	3/6/05: Pix
 *		Added special checking.
 *	1/19/04, Pix
 *		Made the caster drop anything he's holding just before he teleports.
 *	8/27/04, mith
 *		Added the InternalTimer, which gives a 3/4 second pause between when a rune is targetted and when the player is teleported.
	6/5/04, Pix
		Merged in 1.0RC0 code.
*/

using System;
using Server.Items;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Regions;

namespace Server.Spells.Fourth
{
    public class RecallSpell : Spell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Recall", "Kal Ort Por",
                SpellCircle.Fourth,
                239,
                9031,
                Reagent.BlackPearl,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        private RunebookEntry m_Entry;
        private Runebook m_Book;

        public RecallSpell(Mobile caster, Item scroll)
            : this(caster, scroll, null, null)
        {
        }

        public RecallSpell(Mobile caster, Item scroll, RunebookEntry entry, Runebook book)
            : base(caster, scroll, m_Info)
        {
            m_Entry = entry;
            m_Book = book;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            //if ( TransformationSpell.UnderTransformation( Caster, typeof( WraithFormSpell ) ) )
            //min = max = 0;
            //else
            base.GetCastSkills(out min, out max);
        }

        public override void OnCast()
        {
            if (m_Entry == null)
                Caster.Target = new InternalTarget(this);
            else
                Effect(m_Entry.Location, m_Entry.Map, true);
        }

        public override bool CheckCast()
        {
            if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                return false;
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                return false;
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }
            else if (Caster is PlayerMobile && (DateTime.Now - ((PlayerMobile)Caster).LastStoleAt < TimeSpan.FromMinutes(2)))
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom);
        }

        public void Effect(Point3D loc, Map map, bool checkMulti)
        {
            if (map == null || (!Core.AOS && Caster.Map != map))
            {
                Caster.SendLocalizedMessage(1005569); // You can not recall to another facet.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.RecallTo))
            {
            }
            else if (Caster.Kills >= 5 && map != Map.Felucca)
            {
                Caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
            }
            else if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if ((checkMulti && SpellHelper.CheckMulti(loc, map)))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (m_Book != null && m_Book.CurCharges <= 0)
            {
                Caster.SendLocalizedMessage(502412); // There are no charges left on that item.
            }
            else if (BaseBoat.FindBoatAt(loc, map, 16) != null)
            {
                //plasma: If this is a boat, disallow
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (IsSpecial(loc) || IsSpecial(Caster.Location))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (CheckSequence())
            {
                InternalTimer t = new InternalTimer(this, Caster, loc, m_Book);
                t.Start();
            }

            FinishSequence();
        }

        private bool IsSpecial(Point3D location)
        {
            Region region = Server.Region.Find(location, Map.Felucca);
            if (region != null)
            {
                if (Jail.IsInSpecial(region.Name))
                {
                    return true;
                }
            }
            return false;
        }

        private class InternalTarget : Target
        {
            private RecallSpell m_Owner;

            public InternalTarget(RecallSpell owner)
                : base(12, false, TargetFlags.None)
            {
                m_Owner = owner;

                owner.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is RecallRune)
                {
                    RecallRune rune = (RecallRune)o;

                    if (rune.Marked)
                        m_Owner.Effect(rune.Target, rune.TargetMap, true);
                    else
                        from.SendLocalizedMessage(501805); // That rune is not yet marked.
                }
                else if (o is Runebook)
                {
                    RunebookEntry e = ((Runebook)o).Default;

                    if (e != null)
                        m_Owner.Effect(e.Location, e.Map, true);
                    else
                        from.SendLocalizedMessage(502354); // Target is not marked.
                }
                else if (o is Key && ((Key)o).KeyValue != 0 && ((Key)o).Link is BaseBoat)
                {
                    BaseBoat boat = ((Key)o).Link as BaseBoat;

                    if (!boat.Deleted && boat.CheckKey(((Key)o).KeyValue))
                        m_Owner.Effect(boat.GetMarkedLocation(), boat.Map, false);
                    else
                        from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 502357, from.Name, "")); // I can not recall from that object.
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 502357, from.Name, "")); // I can not recall from that object.
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }

        private class InternalTimer : Timer
        {
            private Spell m_Spell;
            private Mobile m_Caster;
            private Point3D m_Location;
            private Runebook m_Book;

            public InternalTimer(Spell spell, Mobile caster, Point3D location, Runebook book)
                : base(TimeSpan.FromSeconds(0.75))
            {
                m_Spell = spell;
                m_Caster = caster;
                m_Location = location;
                m_Book = book;

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                // Adam: because we delay before the actual teleport, we should recheck
                //	to make sure we havn't done anything funky like looted someone.
                if (m_Spell.CheckCast())
                {
                    // Since all we have is Felucca, we can assume Map.Felucca here
                    BaseCreature.TeleportPets(m_Caster, m_Location, Map.Felucca, true);

                    if (m_Book != null)
                        --m_Book.CurCharges;

                    //Pix: Make sure the caster hasn't picked up anything since he or she
                    // targetted the recall object.
                    m_Caster.DropHolding();

                    // is this exploit check enabled?
                    if ((CoreAI.DynamicFeatures & (int)CoreAI.FeatureBits.InmateRecallExploitCheck) > 0)
                    {
                        // Adam: Look for the recall exploit and send them to jail
                        PlayerMobile pm = m_Caster as PlayerMobile;
                        if (pm != null && pm.Inmate == true)
                        {
                            Server.Point3D jail = new Point3D(5295, 1174, 0);
                            pm.MoveToWorld(jail, Map.Felucca);
                            return;
                        }
                    }

                    m_Caster.PlaySound(0x1FC);
                    m_Caster.MoveToWorld(m_Location, Map.Felucca);
                    m_Caster.PlaySound(0x1FC);
                }
            }
        }
    }
}