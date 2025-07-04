#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Foray.FATE.Observer;

public enum OID : uint
{
    Boss = 0x47DC,
    Helper2 = 0x47DE,
    Eye = 0x47DD,
    Eye2 = 0x4818,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 43367, // Boss->player, no cast, single-target
    _Ability_Search = 43038, // Eye/Eye2->self, no cast, single-target
    _Weaponskill_MarkOfDeath = 43039, // Eye/Eye2->self, no cast, range 6 ?-degree cone
    _Ability_ = 42839, // Eye/Eye2->self, no cast, single-target
    _Weaponskill_Stare = 43268, // Boss->self, 5.0s cast, range 60 width 8 rect
    _Weaponskill_JumpScare = 43041, // Boss->player, no cast, single-target
    _Weaponskill_Oogle = 43043, // Boss->self, 4.0s cast, range 40 circle
    _Spell_VoidThunderII = 43045, // Helper2->location, 3.0s cast, range 6 circle
    _Weaponskill_Stare1 = 43044, // Boss->self, 5.0s cast, range 60 width 8 rect
}

class StareBeam(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_Stare, AID._Weaponskill_Stare1], new AOEShapeRect(60, 4));
class VoidThunderII(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidThunderII, 6);
class OogleGaze(BossModule module) : Components.CastGaze(module, AID._Weaponskill_Oogle, false, 40);

// Eye/Eye2 persistent void zone (Treat the wandering gaze as a voidzone and just steer clear of it)
class PersistentVoidZone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Eye).Concat(m.Enemies(OID.Eye2)).Where(e => !e.IsDeadOrDestroyed), 4, 1);

class ObserverStates : StateMachineBuilder
{
    public ObserverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StareBeam>()
            .ActivateOnEnter<VoidThunderII>()
            .ActivateOnEnter<OogleGaze>()
            .ActivateOnEnter<PersistentVoidZone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13853)]
public class Observer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-70, 560), new ArenaBoundsCircle(30));