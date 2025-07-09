#pragma warning disable CA1707 // Identifiers should not contain underscores
using System.Numerics;

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

}

class ColossalSlam(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ColossalSlam, new AOEShapeCone(60, 30.Degrees()));
class ColossalCharge(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ColossalCharge, AID._Weaponskill_ColossalCharge1], new AOEShapeRect(60, 7));
class GigantomillRotating(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Gigantomill)
{
    private readonly List<(WPos pos, Angle rot, DateTime activation)> _aoes = [];
    private const float Length = 72;
    private const float HalfWidth = 5;
    private const int Steps = 4;
    private const float StepAngle = -90f / (Steps + 1);
    private const float StepDelay = 2.0f; // Adjust if timing is different
    private readonly AOEShapeCross Shape = new(Length, HalfWidth);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _aoes.Select(a => new AOEInstance(Shape, a.pos, a.rot, a.activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Gigantomill)
        {
            var baseTime = Module.CastFinishAt(spell);
            var pos = caster.Position;
            var rot = caster.Rotation;
            for (int i = 0; i <= Steps; ++i)
            {
                _aoes.Add((pos, rot + (i * StepAngle).Degrees(), baseTime.AddSeconds(i * StepDelay)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Gigantomill1 or AID._Weaponskill_Gigantomill && _aoes.Count > 0)
        {
            // Remove the earliest unresolved AOE
            _aoes.RemoveAt(0);
        }
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
                var dest = p + 18 * dir;
                foreach (var comp in Module.Components.OfType<Components.GenericAOEs>())
                    foreach (var aoe in comp.ActiveAOEs(slot, actor))
                        if (aoe.Check(dest))
                            return true;
            }
            return false;
        }, activation.AddSeconds(2));
    }
}
class ExplosionPredict(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, DateTime activation, ulong instanceID, bool isKeg2)> _predicted = [];
    private const float DonutInner = 3, DonutOuter = 17, CircleRadius = 15;
    private readonly AOEShapeDonut Donut = new(DonutInner, DonutOuter);
    private readonly AOEShapeCircle Circle = new(CircleRadius);
    private const float KegKnockDistance = 9; // how far to move keg left/right
    private static bool IsKeg2(Actor actor) => actor.OID == (uint)OID.HelperKeg2;
    private static bool IsKeg(Actor actor) => actor.OID == (uint)OID.HelperKeg;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _predicted.Select(a => new AOEInstance(a.isKeg2 ? Circle : Donut, a.pos, default, a.activation));

    public override void OnActorCreated(Actor actor)
    {
        if (IsKeg(actor) || IsKeg2(actor))
        {
            _predicted.Add((actor.Position, WorldState.FutureTime(20), actor.InstanceID, IsKeg2(actor)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion1))
        {
            var idx = _predicted.FindIndex(a => a.instanceID == caster.InstanceID);
            if (idx >= 0)
                _predicted[idx] = (caster.Position, Module.CastFinishAt(spell), caster.InstanceID, _predicted[idx].isKeg2);
        }
        else if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalLaunch))
        {
            // Swap only for currently tracked kegs
            for (int i = 0; i < _predicted.Count; ++i)
                _predicted[i] = (_predicted[i].pos, _predicted[i].activation, _predicted[i].instanceID, !_predicted[i].isKeg2);
        }
        else if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge1))
        {
            // Use the same AOEShapeRect as ColossalCharge
            var chargeShape = new AOEShapeRect(60, 7);
            var chargeDir = spell.Rotation;
            var isLeft = spell.Action == ActionID.MakeSpell(AID._Weaponskill_ColossalCharge);
            var offsetDir = (isLeft ? 1 : -1) * 90.Degrees();
            var moveDir = (chargeDir + offsetDir).ToDirection();
            for (int i = 0; i < _predicted.Count; ++i)
            {
                // Check if keg is hit by the charge AoE
                if (chargeShape.Check(_predicted[i].pos, caster.Position, chargeDir))
                {
                    // Move keg left or right by KegKnockDistance
                    var newPos = _predicted[i].pos + KegKnockDistance * moveDir;
                    _predicted[i] = (newPos, _predicted[i].activation, _predicted[i].instanceID, _predicted[i].isKeg2);
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion) || spell.Action == ActionID.MakeSpell(AID._Weaponskill_Explosion1))
        {
            _predicted.RemoveAll(a => a.instanceID == caster.InstanceID);
        }
    }
}

class ColossalLaunchRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ColossalLaunch);

class GeryonTheSteerStates : StateMachineBuilder
{
    public GeryonTheSteerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExplosionPredict>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<SubterraneanShudderRaidwide>()
            .ActivateOnEnter<RunawayRunoffKnockback>()
            .ActivateOnEnter<ColossalLaunchRaidwide>()
            .ActivateOnEnter<ColossalCharge>()
            .ActivateOnEnter<GigantomillRotating>()
            .ActivateOnEnter<ExplodingCatapultRaidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442)]
public class GeryonTheSteer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-213, 100), new ArenaBoundsSquare(18));
