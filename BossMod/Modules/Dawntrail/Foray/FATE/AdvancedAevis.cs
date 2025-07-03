#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Dawntrail.Foray.FATE.AdvancedAevis;

public enum OID : uint
{
    Boss = 0x4737,
    Helper = 0x4738,
}

public enum AID : uint
{
    //_AutoAttack_ = 42005, // Boss->player, no cast, single-target
    //_Ability_ = 41995, // Boss->location, no cast, single-target
    ZombieScales = 42000, // Helper->location, 8.0s cast, range 40 ?-degree cone
    //_Weaponskill_ZombieScales1 = 41997, // Boss->self, 8.0s cast, single-target
    ZombieScales2 = 42001, // Helper->location, 8.0s cast, range 40 ?-degree cone
    AeroII = 42018, // Helper->location, 3.0s cast, range 4 circle
    //_Weaponskill_AeroII1 = 42017, // Boss->self, 3.0s cast, single-target
}

class ZombieScales(BossModule module) : Components.GroupedAOEs(module, [AID.ZombieScales, AID.ZombieScales2], new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 6);
class AeroII(BossModule module) : Components.StandardAOEs(module, AID.AeroII, new AOEShapeCircle(4));

class AdvancedAevisStates : StateMachineBuilder
{
    public AdvancedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ZombieScales>()
            .ActivateOnEnter<AeroII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13704)]
public class AdvancedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-48.1f, -320), new ArenaBoundsCircle(20));
