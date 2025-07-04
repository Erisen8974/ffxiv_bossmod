#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Foray.FATE.AdvancedAevis;

public enum OID : uint
{
    Boss = 0x4737,
    Helper2 = 0x4738,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 42005, // Boss->player, no cast, single-target
    _Ability_ = 41995, // Boss->location, no cast, single-target

    Cosmetic_ZombieScales4 = 41999, // Boss->self, 8.0s cast, single-target
    Cosmetic_ZombieScales3 = 41998, // Boss->self, 8.0s cast, single-target
    Cosmetic_ZombieScales2 = 41997, // Boss->self, 8.0s cast, single-target
    Cosmetic_ZombieScales1 = 41996, // Boss->self, 8.0s cast, single-target
    Cosmetic_AeroII = 42017, // Boss->self, 3.0s cast, single-target
    Cosmetic_BreathWing1 = 42006, // Boss->self, 5.0s cast, single-target
    Cosmetic_TripleFlight = 42010, // Boss->self, no cast, single-target
    Cosmetic_QuarryLake = 42002, // Boss->self, 5.0s cast, single-target

    ZombieScales = 42000, // Helper2->location, 8.0s cast, range 40 ?-degree cone
    ZombieScales2 = 42001, // Helper2->location, 8.0s cast, range 40 ?-degree cone
    AeroII = 42018, // Helper2->location, 3.0s cast, range 4 circle
    ZombieBreath = 42004, // Boss->self, 5.0s cast, range 40 180-degree cone

    BreathWing_Boss = 42008, // Boss->self, 4.0s cast, single-target
    BreathWing_PB = 42009, // Helper2->location, 4.0s cast, range 10 circle
    BreathWing_Donut = 42013, // Boss->self, no cast, range ?-20 donut
    TripleFlight_Boss = 42012, // Boss->self, 4.0s cast, range ?-20 donut
    TripleFlight_PB = 42011, // Helper2->location, no cast, range 10 circle
    FlashFoehn_Followup = 42014, // Boss->self, no cast, range 80 width 10 rect

    BreathWing_RaidWide = 42007, // Helper2->location, 5.0s cast, range 30 circle
    QuarryLake = 42003, // Helper2->location, 5.0s cast, range 40 circle

}

class ZombieScales(BossModule module) : Components.GroupedAOEs(module, [AID.ZombieScales, AID.ZombieScales2], new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 6);
class AeroII(BossModule module) : Components.StandardAOEs(module, AID.AeroII, new AOEShapeCircle(4));
class ZombieBreath(BossModule module) : Components.StandardAOEs(module, AID.ZombieBreath, new AOEShapeCone(40, 90.Degrees()));
class BreathWing(BossModule module) : Components.RaidwideCast(module, AID.BreathWing_RaidWide);
class QuarryLake(BossModule module) : Components.CastGaze(module, AID.QuarryLake, false, 40);

public class Triple(BossModule module) : Components.GenericAOEs(module)
{
    private enum Shape
    {
        Circle,
        Donut,
        Line,
    }
    private readonly List<(Shape s, DateTime activation)> _predicted = [];

    public uint Color;
    public uint ColorImminent = ArenaColor.Danger;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime nextActivation = default;
        foreach (var c in _predicted)
        {
            var color = Color;
            var thisActivation = c.activation;
            if (nextActivation == default)
                nextActivation = thisActivation.AddSeconds(0.5f);
            if (thisActivation < nextActivation)
                color = ColorImminent;
            yield return GetShape(c) with { Color = color };
        }
    }
    private AOEInstance circle = new(new AOEShapeCircle(10), default);
    private AOEInstance donut = new(new AOEShapeDonut(10, 20), default);
    private AOEInstance line = new(new AOEShapeRect(40, 5, 40), default);
    private WPos origin;
    private Angle rotation;

    private AOEInstance BaseAOE(Shape s) => s switch
    {
        Shape.Circle => circle,
        Shape.Donut => donut,
        Shape.Line => line,
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
    };

    private AOEInstance GetShape((Shape s, DateTime activation) info) => BaseAOE(info.s) with { Activation = info.activation, Origin = origin, Rotation = rotation };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.BreathWing_Boss:
                origin = caster.Position;
                rotation = caster.Rotation;
                _predicted.Add((Shape.Circle, activation));
                _predicted.Add((Shape.Donut, activation.AddSeconds(2f)));
                _predicted.Add((Shape.Line, activation.AddSeconds(4f)));
                _predicted.SortBy(p => p.activation);
                break;
            case AID.TripleFlight_Boss:
                origin = caster.Position;
                rotation = caster.Rotation;
                _predicted.Add((Shape.Donut, activation));
                _predicted.Add((Shape.Circle, activation.AddSeconds(2f)));
                _predicted.Add((Shape.Line, activation.AddSeconds(4f)));
                _predicted.SortBy(p => p.activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BreathWing_PB or AID.TripleFlight_PB:
                if (_predicted.Count > 0)
                {
                    if (_predicted[0].s != Shape.Circle)
                        Service.Log($"[TripleCast] Unexpected cast order: expected {circle}, got {_predicted[0]}");
                    _predicted.RemoveAll(p => p.s == Shape.Circle);
                }
                else
                {
                    Service.Log($"[TripleCast] Unexpected cast order: no predicted cast for {circle}");
                }
                NumCasts++;
                break;
            case AID.BreathWing_Donut or AID.TripleFlight_Boss:
                if (_predicted.Count > 0)
                {
                    if (_predicted[0].s != Shape.Donut)
                        Service.Log($"[TripleCast] Unexpected cast order: expected {donut}, got {_predicted[0]}");
                    _predicted.RemoveAll(p => p.s == Shape.Donut);
                }
                else
                {
                    Service.Log($"[TripleCast] Unexpected cast order: no predicted cast for {donut}");
                }
                NumCasts++;
                break;
            case AID.FlashFoehn_Followup:
                if (_predicted.Count > 0)
                {
                    if (_predicted[0].s != Shape.Line)
                        Service.Log($"[TripleCast] Unexpected cast order: expected {line}, got {_predicted[0]}");
                    _predicted.RemoveAll(p => p.s == Shape.Line);
                }
                else
                {
                    Service.Log($"[TripleCast] Unexpected cast order: no predicted cast for {line}");
                }
                NumCasts++;
                break;
        }
    }
}

class AdvancedAevisStates : StateMachineBuilder
{
    public AdvancedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ZombieScales>()
            .ActivateOnEnter<ZombieBreath>()
            .ActivateOnEnter<Triple>()
            .ActivateOnEnter<AeroII>()
            .ActivateOnEnter<QuarryLake>()
            .ActivateOnEnter<BreathWing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13704)]
public class AdvancedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-48.1f, -320), new ArenaBoundsCircle(30));
