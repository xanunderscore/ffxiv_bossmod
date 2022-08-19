﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Trial.T03GarudaN
{
    public enum OID : uint
    {
        Boss = 0xEF, // x1
        Monolith = 0xED, // x4
        EyeOfTheStormHelper = 0x622, // x1
        RazorPlumeP1 = 0xEE, // spawn during fight
        RazorPlumeP2 = 0x2B0, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        Friction = 656, // Boss->players, no cast, range 5 circle at random target
        Downburst = 657, // Boss->self, no cast, range 10+1.7 ?-degree cone cleave
        WickedWheel = 658, // Boss->self, no cast, range 7+1.7 circle cleave
        Slipstream = 659, // Boss->self, 2.5s cast, range 10+1.7 ?-degree cone interruptible aoe
        MistralSongP1 = 667, // Boss->self, 4.0s cast, range 30+1.7 LOSable raidwide
        AerialBlast = 662, // Boss->self, 4.0s cast, raidwide
        EyeOfTheStorm = 664, // EyeOfTheStormHelper->self, 3.0s cast, range 12-25 donut
        MistralSongP2 = 660, // Boss->self, 4.0s cast, range 30+1.7 ?-degree cone aoe
        MistralShriek = 661, // Boss->self, 4.0s cast, range 23+1.7 circle aoe
        Featherlance = 665, // RazorPlumeP1/RazorPlumeP2->self, no cast, range 8 circle, suicide attack if not killed in ~25s
    };

    // disallow clipping monoliths
    class Friction : BossComponent
    {
        private AOEShapeCircle _shape = new(5);

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            if (module.PrimaryActor.CastInfo == null) // don't forbid standing near monoliths while boss is casting to allow avoiding aoes
                foreach (var m in module.Enemies(OID.Monolith))
                    zone.ForbidZone(_shape, m.Position, new(), module.WorldState.CurrentTime, 10000);
        }
    }

    class Downburst : Components.Cleave
    {
        public Downburst() : base(ActionID.MakeSpell(AID.Downburst), new AOEShapeCone(11.7f, 60.Degrees())) { }
    }

    class Slipstream : Components.SelfTargetedAOEs
    {
        public Slipstream() : base(ActionID.MakeSpell(AID.Slipstream), new AOEShapeCone(11.7f, 45.Degrees()), false) { }
    }

    class MistralSongP1 : Components.CastLineOfSightAOE
    {
        public MistralSongP1() : base(ActionID.MakeSpell(AID.MistralSongP1), (uint)OID.Monolith, 31.7f) { }
    }

    // actual casts happen every ~6s after aerial blast cast
    class EyeOfTheStorm : Components.GenericSelfTargetedAOEs
    {
        public EyeOfTheStorm() : base(ActionID.MakeSpell(AID.AerialBlast), new AOEShapeDonut(12, 25)) { }

        public override IEnumerable<(WPos, Angle, DateTime)> ImminentCasts(BossModule module)
        {
            if (NumCasts > 0)
                foreach (var c in module.Enemies(OID.EyeOfTheStormHelper))
                    yield return (c.Position, new Angle(), module.WorldState.CurrentTime);
        }
    }

    class MistralSongP2 : Components.SelfTargetedAOEs
    {
        public MistralSongP2() : base(ActionID.MakeSpell(AID.MistralSongP2), new AOEShapeCone(31.7f, 60.Degrees()), false) { }
    }

    class MistralShriek : Components.SelfTargetedAOEs
    {
        public MistralShriek() : base(ActionID.MakeSpell(AID.MistralShriek), new AOEShapeCircle(24.7f), false) { }
    }

    class T03GarudaNStates : StateMachineBuilder
    {
        public T03GarudaNStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Friction>()
                .ActivateOnEnter<Downburst>()
                .ActivateOnEnter<Slipstream>()
                .ActivateOnEnter<MistralSongP1>()
                .ActivateOnEnter<EyeOfTheStorm>()
                .ActivateOnEnter<MistralSongP2>()
                .ActivateOnEnter<MistralShriek>();
        }
    }

    public class T03GarudaN : BossModule
    {
        public T03GarudaN(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-0, 0), 21)) { }

        public override bool FillTargets(BossTargets targets, int pcSlot)
        {
            if (!targets.AddIfValid(Enemies(OID.RazorPlumeP1)) && !targets.AddIfValid(Enemies(OID.RazorPlumeP2)))
                targets.AddIfValid(PrimaryActor);
            return true;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var m in Enemies(OID.Monolith))
                Arena.Actor(m, ArenaColor.Danger);
        }
    }
}