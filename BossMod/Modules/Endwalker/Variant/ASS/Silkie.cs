#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Endwalker.Variant.ASS.Silkie;

public enum OID : uint
{
    Boss = 0x39EF,
    OtherSilkPuff = 0x233C,
    SilkPuff = 0x39F0,
    Puddles = 0x1E9230,
}

public enum AID : uint
{
    _AutoAttack_Attack = 6497, // Boss->player, no cast, single-target
    _Weaponskill_TotalWash = 30508, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_SqueakyLeft = 30510, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_SqueakyLeft1 = 30514, // OtherSilkPuff->self, 6.0s cast, range 60 90-degree cone
    _Weaponskill_SqueakyLeft2 = 30515, // OtherSilkPuff->self, 7.7s cast, range 60 90-degree cone
    _Weaponskill_SqueakyLeft3 = 30516, // OtherSilkPuff->self, 9.2s cast, range 60 225-degree cone
    _Weaponskill_SqueakyRight = 30509, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_SqueakyRight1 = 30511, // OtherSilkPuff->self, 6.0s cast, range 60 90-degree cone
    _Weaponskill_SqueakyRight2 = 30512, // OtherSilkPuff->self, 7.7s cast, range 60 90-degree cone
    _Weaponskill_SqueakyRight3 = 30513, // OtherSilkPuff->self, 9.2s cast, range 60 225-degree cone
    _Weaponskill_CarpetBeater = 30507, // Boss->player, 5.0s cast, single-target
    _Weaponskill_ChillingSuds = 30518, // Boss->self, 5.0s cast, single-target
    _Weaponskill_SoapsUp = 30519, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_ChillingDuster = 30520, // OtherSilkPuff->self, 5.0s cast, range 60 width 10 cross
    _Weaponskill_BracingSuds = 30517, // Boss->self, 5.0s cast, single-target
    _Weaponskill_BracingDuster = 30521, // OtherSilkPuff->self, 5.0s cast, range 5-60 donut
    _Weaponskill_SlipperySoap = 30522, // Boss->location, 5.0s cast, width 10 rect charge
    _Weaponskill_ChillingDuster1 = 30523, // OtherSilkPuff->self, 8.5s cast, range 60 width 10 cross
    _Weaponskill_SpotRemover = 30531, // OtherSilkPuff->location, 3.5s cast, range 5 circle
    _Weaponskill_SpotRemover1 = 30530, // Boss->self, 3.5s cast, single-target
    _Weaponskill_DustBluster = 30532, // Boss->location, 5.0s cast, range 60 circle
    _Weaponskill_FreshPuff = 30525, // Boss->self, 4.0s cast, single-target
    _Weaponskill_PuffAndTumble = 30538, // SilkPuff->location, 3.0s cast, single-target
    _Weaponskill_PuffAndTumble1 = 30540, // OtherSilkPuff->location, 4.6s cast, range 4 circle
    _Weaponskill_PuffAndTumble2 = 30539, // SilkPuff->location, no cast, single-target
    _Weaponskill_PuffAndTumble3 = 30656, // OtherSilkPuff->location, 1.6s cast, range 4 circle
    _Weaponskill_BracingDuster1 = 30524, // OtherSilkPuff->self, 8.5s cast, range 5-60 donut
    _Weaponskill_SoapingSpree = 30529, // SilkPuff->self, 6.0+1.0s cast, single-target
    _Weaponskill_SoapingSpree1 = 30526, // Boss->self, 6.0+1.0s cast, single-target
    _Weaponskill_ChillingDuster2 = 30527, // OtherSilkPuff->self, 7.0s cast, range 60 width 10 cross
}

class TotalWashRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_TotalWash);
class DustBlusterKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_DustBluster, 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = Module.Components.OfType<Components.GenericAOEs>();
        var center = Module.Arena.Center;
        var bounds = Module.Arena.Bounds;
        var a = actor;
        foreach (var src in Sources(slot, actor))
        {
            var source = src;
            hints.AddForbiddenZone(p =>
            {
                var dir = (p - source.Origin).Normalized();
                var dest = p + source.Distance * dir;
                if (!bounds.Contains(dest - center))
                    return true;
                foreach (var comp in aoes)
                    foreach (var aoe in comp.ActiveAOEs(slot, a))
                        if (aoe.Check(dest))
                            return true;
                return false;
            }, src.Activation);
        }
    }
}
class SqueakyLeftCone(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyLeft1, new AOEShapeCone(60, 45.Degrees()));
class SqueakyLeftCone2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyLeft2, new AOEShapeCone(60, 45.Degrees()));
class SqueakyLeftCone3(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyLeft3, new AOEShapeCone(60, 112.5f.Degrees()));
class SqueakyRightCone(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyRight1, new AOEShapeCone(60, 45.Degrees()));
class SqueakyRightCone2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyRight2, new AOEShapeCone(60, 45.Degrees()));
class SqueakyRightCone3(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SqueakyRight3, new AOEShapeCone(60, 112.5f.Degrees()));
class ChillingDusterCross(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChillingDuster, new AOEShapeCross(60, 5));
class ChillingDuster1Cross(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChillingDuster1, new AOEShapeCross(60, 5));
class ChillingDuster2Cross(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChillingDuster2, new AOEShapeCross(60, 5));
class BracingDusterDonut(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BracingDuster, new AOEShapeDonut(5, 60));
class BracingDuster1Donut(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BracingDuster1, new AOEShapeDonut(5, 60));
class PuffAndTumbleCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PuffAndTumble1, new AOEShapeCircle(4));
class PuffAndTumble3Circle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PuffAndTumble3, new AOEShapeCircle(4));

class SpotRemoverVoids(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID._Weaponskill_SpotRemover, m => m.Enemies(OID.Puddles), 0) { }

class SilkieStates : StateMachineBuilder
{
    public SilkieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TotalWashRaidwide>()
            .ActivateOnEnter<DustBlusterKnockback>()
            .ActivateOnEnter<SqueakyLeftCone>()
            .ActivateOnEnter<SqueakyLeftCone2>()
            .ActivateOnEnter<SqueakyLeftCone3>()
            .ActivateOnEnter<SqueakyRightCone>()
            .ActivateOnEnter<SqueakyRightCone2>()
            .ActivateOnEnter<SqueakyRightCone3>()
            .ActivateOnEnter<ChillingDusterCross>()
            .ActivateOnEnter<ChillingDuster1Cross>()
            .ActivateOnEnter<ChillingDuster2Cross>()
            .ActivateOnEnter<BracingDusterDonut>()
            .ActivateOnEnter<BracingDuster1Donut>()
            .ActivateOnEnter<SlipperySoapCharge>()
            .ActivateOnEnter<SpotRemoverVoids>()
            .ActivateOnEnter<PuffAndTumbleCircle>()
            .ActivateOnEnter<PuffAndTumble3Circle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11369)]
public class Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-335, -155), new ArenaBoundsSquare(20));

class SlipperySoapCharge(BossModule module) : Components.ChargeAOEs(module, AID._Weaponskill_SlipperySoap, 5) { }
