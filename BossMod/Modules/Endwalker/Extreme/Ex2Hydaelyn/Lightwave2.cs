﻿namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for second lightwave (3 waves, 5 crystals) + hero's glory mechanics
class Lightwave2(BossModule module) : LightwaveCommon(module)
{
    private WPos _safeCrystal;
    private Vector4? _safeCrystalOrigin;

    private static readonly WPos _crystalCenter = new(100, 101);
    private static readonly WPos _crystalTL = new(90, 92);
    private static readonly WPos _crystalTR = new(110, 92);
    private static readonly WPos _crystalBL = new(90, 110);
    private static readonly WPos _crystalBR = new(110, 110);
    private static readonly AOEShapeCone _gloryAOE = new(40, 90.Degrees());

    public override void Update()
    {
        if (NumCasts == 4 && (Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && Module.PrimaryActor.PosRot != _safeCrystalOrigin)
        {
            _safeCrystalOrigin = Module.PrimaryActor.PosRot;
            _safeCrystal = new[] { _crystalTL, _crystalTR, _crystalBL, _crystalBR }.FirstOrDefault(c => !_gloryAOE.Check(c, Module.PrimaryActor));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && _gloryAOE.Check(actor.Position, Module.PrimaryActor))
            hints.Add("GTFO from glory aoe!");

        (bool inWave, bool inSafeCone) = NumCasts < 4
            ? (WaveAOE.Check(actor.Position, Wave1Pos(), 0.Degrees()) || WaveAOE.Check(actor.Position, Wave2Pos(), 0.Degrees()), InSafeCone(NextSideCrystal(), _crystalCenter, actor.Position))
            : (WaveAOE.Check(actor.Position, Wave3Pos(), 0.Degrees()), _safeCrystal == default || InSafeCone(_crystalCenter, _safeCrystal, actor.Position));

        if (inWave)
            hints.Add("GTFO from wave!");
        if (!inSafeCone)
            hints.Add("Hide behind crystal!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false)
            _gloryAOE.Draw(Arena, Module.PrimaryActor);

        if (NumCasts < 4)
        {
            WaveAOE.Draw(Arena, Wave1Pos(), 0.Degrees());
            WaveAOE.Draw(Arena, Wave2Pos(), 0.Degrees());
            DrawSafeCone(NextSideCrystal(), _crystalCenter);
        }
        else
        {
            WaveAOE.Draw(Arena, Wave3Pos(), 0.Degrees());
            if (_safeCrystal != new WPos())
            {
                DrawSafeCone(_crystalCenter, _safeCrystal);
            }
        }
    }

    private WPos Wave1Pos() => Waves.Count > 0 ? Waves[0].Position : new(86, 70);
    private WPos Wave2Pos() => Waves.Count switch
    {
        0 => new(114, 70),
        1 => new(Waves[0].Position.X < 100 ? 114 : 86, 70),
        _ => Waves[1].Position
    };
    private WPos Wave3Pos() => Waves.Count > 2 ? Waves[2].Position : new(100, 70);

    private WPos NextSideCrystal()
    {
        bool w1Next = (NumCasts & 1) == 0;
        bool w1Left = Wave1Pos().X < 100;
        float nextX = w1Next == w1Left ? _crystalTL.X : _crystalBR.X;
        float nextZ = (NumCasts & 2) == 0 ? _crystalTL.Z : _crystalBR.Z;
        return new(nextX, nextZ);
    }
}
