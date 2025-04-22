using BossMod.Autorotation.MiscAI;
using BossMod.Dawntrail.Savage.RM03SBruteBomber;

namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

public enum OID : uint
{
    Boss = 0x4115,
    Helper = 0x233C,
    Valigarmanda = 0x417A, // R0.000, x1, Part type
    Valigarmanda1 = 0x4179, // R0.000, x1, Part type
    ArcaneSphere = 0x4493, // R1.000, x0 (spawn during fight)
    FeatherOfRuin = 0x4116, // R2.680, x0 (spawn during fight)
    ArcaneSphere1 = 0x4181, // R1.000, x0 (spawn during fight)
    FlameKissedBeacon = 0x438B, // R4.800, x0 (spawn during fight)
    GlacialBeacon = 0x438C, // R4.800, x0 (spawn during fight)
    ThunderousBeacon = 0x438A, // R4.800, x0 (spawn during fight)

}

public enum AID : uint
{
    _Weaponskill_Attack = 36899, // Boss->player, no cast, single-target
    StranglingCoil = 36160, // Helper->self, 7.3s cast, range 8-30 donut
    SlitheringStrike = 36158, // Helper->self, 7.3s cast, range 24 180-degree cone
    SusurrantBreath = 36156, // Helper->self, 7.3s cast, range 50 ?-degree cone
    IceTalon = 36184, // Boss->self, 4.0+1.0s cast, single-target
    IceTalonDamage = 36185, // 417A/4179->player, no cast, range 6 circle
    _Weaponskill_Skyruin = 36161, // Boss->self, 6.0+5.3s cast, single-target
    _Weaponskill_Skyruin1 = 36162, // Helper->self, 4.5s cast, range 80 circle
    _Weaponskill_NorthernCross = 36168, // Helper->self, 3.0s cast, range 60 width 25 rect
    FreezingDust = 36177, // Boss->self, 5.0+0.8s cast, range 80 circle
    _Weaponskill_NorthernCross1 = 36169, // Helper->self, 3.0s cast, range 60 width 25 rect
    _Weaponskill_ChillingCataclysm = 39264, // 4493->self, 1.0s cast, single-target
    ChillingCataclysm = 39265, // Helper->self, 1.5s cast, range 40 width 5 cross
    _Weaponskill_DisasterZone = 36164, // Boss->self, 3.0+0.8s cast, ???
    _Weaponskill_DisasterZone1 = 36165, // Helper->self, 3.8s cast, range 80 circle
    _Weaponskill_Skyruin2 = 38338, // Boss->self, 6.0+5.3s cast, single-target
    _Weaponskill_Skyruin3 = 36163, // Helper->self, 4.5s cast, range 80 circle
    _Weaponskill_ThunderousBreath = 36175, // Boss->self, 7.0+0.9s cast, single-target
    _Weaponskill_ThunderousBreath1 = 36176, // Helper->self, 7.9s cast, range 50 135-degree cone
    _Weaponskill_HailOfFeathers = 36170, // Boss->self, 4.0+2.0s cast, single-target
    _Weaponskill_HailOfFeathers1 = 36171, // 4116->self, no cast, single-target
    _Weaponskill_HailOfFeathers2 = 36361, // Helper->self, 6.0s cast, range 80 circle
    _Weaponskill_BlightedBolt = 36172, // Boss->self, 7.0+0.8s cast, single-target
    _Weaponskill_BlightedBolt1 = 36174, // Helper->4116, 7.8s cast, range 7 circle
    _Weaponskill_BlightedBolt2 = 36173, // Helper->player, no cast, range 3 circle
    _Weaponskill_ArcaneLightning = 39001, // 4181->self, 1.0s cast, range 50 width 5 rect
    _Weaponskill_DisasterZone2 = 36166, // Boss->self, 3.0+0.8s cast, ???
    _Weaponskill_DisasterZone3 = 36167, // Helper->self, 3.8s cast, range 80 circle
    _Weaponskill_Ruinfall = 36186, // Boss->self, 4.0+1.6s cast, single-target
    Ruinfall1 = 36187, // Helper->self, 5.6s cast, range 6 circle
    Ruinfall2 = 36189, // Helper->self, 8.0s cast, range 40 width 40 rect
    Ruinfall3 = 39129, // Helper->location, 9.7s cast, range 6 circle
    _Weaponskill_RuinForetold = 38545, // Boss->self, 5.0s cast, range 80 circle
    _Ability_ = 34722, // Helper->player, no cast, single-target
    _Weaponskill_CalamitousCry = 36192, // Boss->self, 5.1+0.9s cast, single-target
    _Weaponskill_CalamitousEcho = 36195, // Helper->self, 5.0s cast, range 40 20-degree cone
    _Weaponskill_CalamitousCry1 = 36194, // Helper->self, no cast, range 80 width 6 rect
    _Ability_1 = 26708, // Helper->player, no cast, single-target
    _Weaponskill_CalamitousCry2 = 36193, // Boss->self, no cast, single-target
    _Weaponskill_ = 38245, // 438B->Boss, no cast, single-target
    _Weaponskill_1 = 38247, // 438C->Boss, no cast, single-target
    _Weaponskill_2 = 38246, // 438A->Boss, no cast, single-target
    _Weaponskill_3 = 38323, // Boss->self, no cast, single-target
    _Weaponskill_Tulidisaster = 36197, // Boss->self, 7.0+3.0s cast, single-target
    _Weaponskill_Tulidisaster1 = 36199, // Helper->self, no cast, range 80 circle
    _Weaponskill_Tulidisaster2 = 36200, // Helper->self, no cast, range 80 circle
    _Weaponskill_Tulidisaster3 = 36198, // Helper->self, no cast, range 80 circle
    _Weaponskill_Eruption = 36190, // Boss->self, 3.0s cast, single-target
    Eruption = 36191, // Helper->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_Trauma = 3796, // Helper->player, extra=0x1
    _Gen_SustainedDamage = 2935, // Helper->player, extra=0x0
    _Gen_Concussion = 997, // Helper->player, extra=0xF43
    _Gen_DamageDown = 628, // Helper->player, extra=0x1
    _Gen_Frostbite = 2083, // 417A/4179/Helper->player, extra=0x0
    FreezingUp = 3523, // Boss->player, extra=0x0
    _Gen_Electrocution = 2086, // Helper->player, extra=0x0
    _Gen_Levitate = 3974, // none->player, extra=0xD7
    _Gen_ = 2552, // none->player/4116, extra=0x21/0x2AD/0x2BA
    _Gen_DamageUp = 3975, // Boss->Boss, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    _Gen_PerpetualConflagration = 4122, // none->player, extra=0x0

}

public enum IconID : uint
{
    IceTalon = 344, // player->self
}

public class ShapedLocationTargetedAOEs(BossModule module, Enum aid, WPos position, AOEShape shape, Angle rotation = default, string warningText = "GTFO from puddle!", int maxCasts = int.MaxValue) : Components.GenericAOEs(module, aid, warningText)
{
    public AOEShape Shape = shape;
    public WPos Position = position;
    public Angle Rotation = rotation;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public readonly List<Actor> Casters = [];

    public IEnumerable<Actor> ActiveCasters => Casters.Take(MaxCasts);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, Position, Rotation, Module.CastFinishAt(c.CastInfo), Color, Risky));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

class StranglingCoil(BossModule module) : ShapedLocationTargetedAOEs(module, AID.StranglingCoil, new(100, 100), new AOEShapeDonut(8, 30));

class IceTalon(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.IceTalon, AID.IceTalonDamage, 6, 5);

class SlitheringStrike(BossModule module) : Components.SelfTargetedAOEs(module, AID.SlitheringStrike, new AOEShapeCone(24, 90.Degrees()));

class SusurrantBreath(BossModule module) : ShapedLocationTargetedAOEs(module, AID.SusurrantBreath, new(100, 77), new AOEShapeCone(50, 45.Degrees()));

class Eruption(BossModule module) : Components.LocationTargetedAOEs(module, AID.Eruption, 6);
class Ruinfall1(BossModule module) : Components.CastTowers(module, AID.Ruinfall1, 6, 2, 2);

//aihints go towards the front and not in line with a puddle, should be removed for actual KB ai hints
class Ruinfall2(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Ruinfall2, 20, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sources(slot, actor).Any())
            hints.AddForbiddenZone(new AOEShapeRect(100, 100), new(100, 87));
    }
}
class Ruinfall3(BossModule module) : Components.LocationTargetedAOEs(module, AID.Ruinfall3, 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            if (c.Risky)
                hints.AddForbiddenZone(new AOEShapeRect(100, 6), new(c.Origin.X, 80), activation: c.Activation);
    }
}
class T01ValigarmandaStates : StateMachineBuilder
{
    public T01ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SlitheringStrike>()
            .ActivateOnEnter<SusurrantBreath>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<Ruinfall1>()
            .ActivateOnEnter<Ruinfall2>()
            .ActivateOnEnter<Ruinfall3>()
            .ActivateOnEnter<IceTalon>()
            .ActivateOnEnter<StranglingCoil>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Erisen", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 832, NameID = 12854)]
public class T01Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15));
