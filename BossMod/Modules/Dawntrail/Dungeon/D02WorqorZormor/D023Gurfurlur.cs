﻿namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    BitingWind = 0x4160, // R1.000, x0 (spawn during fight)
    AuraSphere = 0x4162, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    HeavingHaymaker = 36269, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    HeavingHaymakerAOE = 36375, // Helper->self, 5.3s cast, range 60 circle, raidwide
    Stonework = 36301, // Boss->self, 3.0s cast, single-target, visual (elemental square)
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect (elemental square spawn)
    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect
    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle spread
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect, knock-forward 25
    Sledgehammer = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect
    SledgehammerRest = 36314, // Boss->self, no cast, range 60 width 8 rect
    SledgehammerTargetSelect = 36315, // Helper->player, no cast, single-target, visual (target select)
    SledgehammerLast = 39260, // Boss->self, no cast, range 60 width 8 rect
    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target, visual (spawn buffing spheres)
    ShroudOfEons = 36321, // AuraSphere->player, no cast, single-target, damage up from touching spheres
    ShroudOfEonsBoss = 36322, // AuraSphere->Boss, no cast, single-target, damage up if sphere reaches boss
    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle, raidwide after spheres
    WindswrathShort = 36310, // Helper->self, 7.0s cast, range 40 circle, knockback 15
    WindswrathLong = 39074, // Helper->self, 15.0s cast, range 40 circle, knockback 15
    Whirlwind = 36311, // Helper->self, no cast, range 5 circle, tornado
}

public enum IconID : uint
{
    VolcanicDrop = 139, // player
}

class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, AID.HeavingHaymakerAOE);
class LithicImpact(BossModule module) : Components.StandardAOEs(module, AID.LithicImpact, new AOEShapeRect(4, 2));

class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = _aoes.FirstOrDefault().Activation.AddSeconds(0.5f);
        return _aoes.TakeWhile(aoe => aoe.Activation < deadline);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Allfire1 or AID.Allfire2 or AID.Allfire3)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Allfire1 or AID.Allfire2 or AID.Allfire3)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
    }
}

class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, AID.VolcanicDrop, 6);

class GreatFlood(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GreatFlood, 25, kind: Kind.DirForward)
{
    private readonly List<Actor> _allfireCasters = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;

            if (_allfireCasters.Count == 0)
            {
                // no aoes => everything closer than knockback distance (25) to the wall is unsafe; add an extra margin for safety
                hints.AddForbiddenZone(ShapeContains.Rect(Arena.Center, s.Direction, 20, 7, 20), s.Activation);
            }
            else
            {
                // safe zone is one a 10x10 rect (4 first allfires), offset by knockback distance
                var center = _allfireCasters.PositionCentroid();
                hints.AddForbiddenZone(ShapeContains.InvertedRect(center, s.Direction, -15, 35, 10), s.Activation);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.Allfire1)
            _allfireCasters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Allfire1)
            _allfireCasters.Remove(caster);
    }
}

class Sledgehammer(BossModule module) : Components.GenericWildCharge(module, 4, AID.Sledgehammer, 60)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Sledgehammer)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SledgehammerLast)
            Source = null;
    }
}

class AuraSpheres : Components.PersistentInvertibleVoidzone
{
    public AuraSpheres(BossModule module) : base(module, 2, m => m.Enemies(OID.AuraSphere).Where(x => !x.IsDead))
    {
        InvertResolveAt = WorldState.CurrentTime;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Sources(Module).Any(x => !Shape.Check(actor.Position, x)))
            hints.Add("Touch the balls!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = Sources(Module).Select(s => ShapeContains.InvertedCircle(s.Position + Shape.Radius * s.Rotation.ToDirection(), Shape.Radius)).ToList();
        if (shapes.Count > 0)
            hints.AddForbiddenZone(ShapeContains.Intersection(shapes));
    }
}

class EnduringGlory(BossModule module) : Components.RaidwideCast(module, AID.EnduringGlory);

class BitingWind(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.BitingWind))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // we want to dodge out->in
        foreach (var t in Sources(Module))
        {
            var dir = t.Rotation.ToDirection();
            var distToCenter = Math.Abs(dir.OrthoL().Dot(t.Position - Module.Center));
            if (distToCenter < 10)
            {
                // normal voidzones for central ones
                hints.AddForbiddenZone(ShapeContains.Circle(t.Position, 5));
                hints.AddForbiddenZone(ShapeContains.Capsule(t.Position, dir, 20, 5), WorldState.FutureTime(2));
            }
            else
            {
                // just forbid outer ones
                hints.AddForbiddenZone(ShapeContains.Rect(t.Position, dir, 40, 40, 5));
            }
        }
    }
}

class WindswrathShort(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WindswrathShort, 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (!IsImmune(slot, s.Activation))
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(s.Origin, 3), s.Activation);
        }
    }
}

class WindswrathLong(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WindswrathLong, 15)
{
    private readonly IReadOnlyList<Actor> _tornadoes = module.Enemies(OID.BitingWind);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation) || (s.Activation - Module.WorldState.CurrentTime).TotalSeconds > 3)
                continue;

            // ok knockback is imminent, calculate precise safe zone
            List<Func<WPos, bool>> funcs = [
                ShapeContains.InvertedRect(Module.Center, new WDir(0, 1), 21, 21, 21),
                .. _tornadoes.Select(t => ShapeContains.Capsule(t.Position, t.Rotation, 20, 6))
            ];
            bool combined(WPos p)
            {
                var offset = p - s.Origin;
                offset += offset.Normalized() * s.Distance;
                var adj = s.Origin + offset;
                return funcs.Any(f => f(adj));
            }
            hints.AddForbiddenZone(combined, s.Activation);
        }
    }
}

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<Sledgehammer>()
            .ActivateOnEnter<AuraSpheres>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<WindswrathShort>()
            .ActivateOnEnter<WindswrathLong>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-54, -195), new ArenaBoundsSquare(20));
