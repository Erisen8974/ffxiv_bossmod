#pragma warning disable CA1707 // Identifiers should not contain underscores
using System.Numerics;
using System.Diagnostics;
using Dalamud.Logging;
using BossMod.QuestBattle.Shadowbringers.RoleQuests;
using BossMod.Endwalker.Alliance.A10RhalgrEmissary;

namespace BossMod.Endwalker.Variant.ASS.GeryonTheSteer;

public enum OID : uint
{
    Boss = 0x398B,
    Helper = 0x233C,
    HelperKeg = 0x39C9,
    HelperKeg2 = 0x398C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 6499, // Boss->player, no cast, single-target
    _Weaponskill_ColossalStrike = 29903, // Boss->player, 5.0s cast, single-target
    _Weaponskill_ExplodingCatapult = 29895, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_Explosion = 29909, // HelperKeg/HelperKeg2->self, 2.5s cast, range 3-17 donut
    _Weaponskill_Explosion1 = 29908, // HelperKeg2/HelperKeg->self, 2.5s cast, range 15 circle
    _Weaponskill_ColossalSlam = 29904, // Boss->self, 6.0s cast, range 60 60-degree cone
    _Weaponskill_SubterraneanShudder = 29906, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_RunawayRunoff = 29911, // Helper->self, 9.0s cast, range 60 circle
    _Weaponskill_ = 29894, // Boss->location, no cast, single-target
    _Weaponskill_ColossalLaunch = 29896, // Boss->self, 5.0s cast, range 40 width 40 rect
    _Weaponskill_1 = 31260, // HelperKeg->self, no cast, single-target
    _Weaponskill_2 = 29907, // HelperKeg2->self, no cast, single-target
    _Weaponskill_ColossalCharge = 29901, // Boss->location, 8.0s cast, width 14 rect charge
    _Weaponskill_Gigantomill = 29898, // Boss->self, 8.0s cast, range 72 width 10 cross
    _Weaponskill_Gigantomill1 = 29899, // Boss->self, no cast, range 72 width 10 cross
    _Weaponskill_ColossalCharge1 = 29900, // Boss->location, 8.0s cast, width 14 rect charge
    _Weaponskill_Gigantomill2 = 29897, // Boss->self, 8.0s cast, range 72 width 10 cross
    _Weaponskill_ColossalSwing = 29905, // Boss->self, 5.0s cast, range 60 180-degree cone

}

class ColossalSlam(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ColossalSlam, new AOEShapeCone(60, 30.Degrees()));
class ColossalChargeLeft(BossModule module) : Components.ChargeAOEs(module, AID._Weaponskill_ColossalCharge, 7) { }
class ColossalChargeRight(BossModule module) : Components.ChargeAOEs(module, AID._Weaponskill_ColossalCharge1, 7) { }
class GigantomillRotating(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, Angle rot, DateTime activation)> _aoes = [];
    private const float Length = 72;
    private const float HalfWidth = 5;
    private const int Steps = 4;
    private const float StepDelay = 2.0f; // Adjust if timing is different
    private readonly AOEShapeCross Shape = new(Length, HalfWidth);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _aoes.Select(a => new AOEInstance(Shape, a.pos, a.rot, a.activation)).Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Gigantomill or AID._Weaponskill_Gigantomill2)
        {
            var baseTime = Module.CastFinishAt(spell);
            var pos = caster.Position;
            // Always start the first cross at a cardinal (0, 90, 180, 270)
            var baseRot = 0.Degrees();
            var isCCW = (AID)spell.Action.ID == AID._Weaponskill_Gigantomill2;
            var stepAngle = (isCCW ? 1 : -1) * 90f / Steps;
            for (var i = 0; i <= Steps; ++i)
            {
                var aoeRot = baseRot + (i * stepAngle).Degrees();
                _aoes.Add((pos, aoeRot, baseTime.AddSeconds(i * StepDelay)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Gigantomill1 or AID._Weaponskill_Gigantomill or AID._Weaponskill_Gigantomill2 && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}
class ExplodingCatapultRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ExplodingCatapult);
class SubterraneanShudderRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_SubterraneanShudder);
class RunawayRunoffKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_RunawayRunoff, 18)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Use the actual cast finish time if available, otherwise default to 20 seconds in the future
        var activation = Casters.FirstOrDefault()?.CastInfo != null ? Module.CastFinishAt(Casters[0].CastInfo) : WorldState.FutureTime(20);
        // Forbidden zone for knockbacks that land outside the arena (at activation time)
        hints.AddForbiddenZone(p =>
        {
            foreach (var k in Casters)
            {
                var dir = (p - k.Position).Normalized();
                var dest = p + 18 * dir;
                if (!Module.Arena.Bounds.Contains(dest - Module.Arena.Center))
                    return true;
            }
            return false;
        }, activation);
        // Forbidden zone for knockbacks that land in AoEs
        hints.AddForbiddenZone(p =>
        {
            foreach (var k in Casters)
            {
                var dir = (p - k.Position).Normalized();
                for (var i = 10; i < 18; ++i)
                {
                    var dest = p + i * dir;
                    var isHit = false;
                    foreach (var comp in Module.Components.OfType<Components.GenericAOEs>())
                        foreach (var aoe in comp.ActiveAOEs(slot, actor))
                            if (aoe.Check(dest))
                                isHit = true;
                    if (!isHit)
                        return false;
                }
            }
            return true;
        }, activation.AddSeconds(2));
    }
}
class ExplosionPredict(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong instanceID, WPos pos, DateTime activation, bool isKeg2)> _predicted = [];
    private const float DonutInner = 3, DonutOuter = 17, CircleRadius = 15;
    private readonly AOEShapeDonut Donut = new(DonutInner, DonutOuter);
    private readonly AOEShapeCircle Circle = new(CircleRadius);
    private const float KegKnockDistance = 9;
    private static bool IsKeg2(Actor actor) => actor.OID == (uint)OID.HelperKeg2;
    private static bool IsKeg(Actor actor) => actor.OID == (uint)OID.HelperKeg;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var a in _predicted)
            yield return new AOEInstance(a.isKeg2 ? Circle : Donut, a.pos, default, a.activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (IsKeg(actor) || IsKeg2(actor))
        {
            _predicted.RemoveAll(a => a.instanceID == actor.InstanceID);
            _predicted.Add((actor.InstanceID, actor.Position, WorldState.FutureTime(20), IsKeg2(actor)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion1))
        {
            _predicted.RemoveAll(a => a.instanceID == caster.InstanceID);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion1))
        {
            // Use the caster's position directly for the keg at the time the explosion cast starts
            var idx = _predicted.FindIndex(a => a.instanceID == caster.InstanceID);
            if (idx >= 0)
            {
                bool isKeg2 = spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion1);
                _predicted[idx] = (caster.InstanceID, caster.Position, Module.CastFinishAt(spell), isKeg2);
            }
        }
        else if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalLaunch))
        {
            for (var i = 0; i < _predicted.Count; ++i)
                _predicted[i] = (_predicted[i].instanceID, _predicted[i].pos, _predicted[i].activation, !_predicted[i].isKeg2);
        }
        else if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge1))
        {
            var chargeLengthFront = 60f;
            var chargeLengthBack = 0f;
            var chargeHalfWidth = 7f;
            var chargeDir = caster.Rotation;
            var isLeft = spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge);
            var offsetDir = (isLeft ? 1 : -1) * 90.Degrees();
            var moveDir = (caster.Rotation + offsetDir).ToDirection();
            var moveVec = moveDir * KegKnockDistance;
            for (int i = 0; i < _predicted.Count; ++i)
            {
                var pos = _predicted[i].pos;
                if (pos.InRect(caster.Position, chargeDir, chargeLengthFront, chargeLengthBack, chargeHalfWidth))
                    _predicted[i] = (_predicted[i].instanceID, pos + moveVec, _predicted[i].activation, _predicted[i].isKeg2);
            }
        }
    }
}

class ColossalLaunchRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ColossalLaunch);
class ColossalSwing(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ColossalSwing, new AOEShapeCone(60, 90.Degrees()));

class GeryonTheSteerStates : StateMachineBuilder
{
    public GeryonTheSteerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExplosionPredict>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<ColossalSwing>()
            .ActivateOnEnter<SubterraneanShudderRaidwide>()
            .ActivateOnEnter<RunawayRunoffKnockback>()
            .ActivateOnEnter<ColossalLaunchRaidwide>()
            .ActivateOnEnter<ColossalChargeLeft>()
            .ActivateOnEnter<ColossalChargeRight>()
            .ActivateOnEnter<GigantomillRotating>()
            .ActivateOnEnter<ExplodingCatapultRaidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442)]
public class GeryonTheSteer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-213, 100), new ArenaBoundsSquare(18));
