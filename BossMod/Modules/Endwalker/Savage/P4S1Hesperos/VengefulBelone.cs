﻿namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to vengeful belone mechanic
class VengefulBelone(BossModule module) : BossComponent(module)
{
    private readonly Dictionary<ulong, Role> _orbTargets = [];
    private int _orbsExploded;
    private readonly int[] _playerRuinCount = new int[8];
    private readonly Role[] _playerActingRole = new Role[8];

    private const float _burstRadius = 8;

    private Role OrbTarget(ulong instanceID) => _orbTargets.GetValueOrDefault(instanceID, Role.None);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
            return; // inactive

        int ruinCount = _playerRuinCount[slot];
        if (ruinCount > 2 || (ruinCount == 2 && _playerActingRole[slot] != Role.None))
        {
            hints.Add("Failed orbs...");
        }

        if (Module.Enemies(OID.Orb).Where(orb => IsOrbLethal(slot, actor, OrbTarget(orb.InstanceID))).InRadius(actor.Position, _burstRadius).Any())
        {
            hints.Add("GTFO from wrong orb!");
        }

        if (ruinCount < 2)
        {
            // TODO: stack check...
            hints.Add($"Pop next orb {ruinCount + 1}/2!", false);
        }
        else if (ruinCount == 2 && _playerActingRole[slot] == Role.None)
        {
            hints.Add($"Avoid orbs", false);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
            return;

        var orbs = Module.Enemies(OID.Orb);
        foreach (var orb in orbs)
        {
            var orbRole = OrbTarget(orb.InstanceID);
            if (orbRole == Role.None)
                continue; // this orb has already exploded

            bool lethal = IsOrbLethal(pcSlot, pc, orbRole);
            Arena.Actor(orb, lethal ? ArenaColor.Enemy : ArenaColor.Danger, true);

            var target = WorldState.Actors.Find(orb.Tether.Target);
            if (target != null)
            {
                Arena.AddLine(orb.Position, target.Position, ArenaColor.Danger);
            }

            int goodInRange = 0, badInRange = 0;
            foreach ((var i, var player) in Raid.WithSlot().InRadius(orb.Position, _burstRadius))
            {
                if (IsOrbLethal(i, player, orbRole))
                    ++badInRange;
                else
                    ++goodInRange;
            }

            bool goodToExplode = goodInRange == 2 && badInRange == 0;
            Arena.AddCircle(orb.Position, _burstRadius, goodToExplode ? ArenaColor.Safe : ArenaColor.Danger);
        }

        foreach ((int i, var player) in Raid.WithSlot())
        {
            bool nearLethalOrb = orbs.Where(orb => IsOrbLethal(i, player, OrbTarget(orb.InstanceID))).InRadius(player.Position, _burstRadius).Any();
            Arena.Actor(player, nearLethalOrb ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.OrbRole:
                _orbTargets[actor.InstanceID] = OrbRoleFromStatusParam(status.Extra);
                break;
            case SID.ThriceComeRuin:
                ModifyRuinStacks(actor, status.Extra);
                break;
            case SID.ActingDPS:
                ModifyActingRole(actor, Role.Melee);
                break;
            case SID.ActingHealer:
                ModifyActingRole(actor, Role.Healer);
                break;
            case SID.ActingTank:
                ModifyActingRole(actor, Role.Tank);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ThriceComeRuin:
                ModifyRuinStacks(actor, 0);
                break;
            case SID.ActingDPS:
            case SID.ActingHealer:
            case SID.ActingTank:
                ModifyActingRole(actor, Role.None);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BeloneBurstsAOETank or AID.BeloneBurstsAOEHealer or AID.BeloneBurstsAOEDPS)
        {
            _orbTargets[caster.InstanceID] = Role.None;
            ++_orbsExploded;
        }
    }

    private Role OrbRoleFromStatusParam(uint param)
    {
        return param switch
        {
            0x13A => Role.Tank,
            0x13B => Role.Melee,
            0x13C => Role.Healer,
            _ => Role.None
        };
    }

    private bool IsOrbLethal(int slot, Actor player, Role orbRole)
    {
        int ruinCount = _playerRuinCount[slot];
        if (ruinCount >= 2)
            return true; // any orb is now lethal

        var actingRole = _playerActingRole[slot];
        if (ruinCount == 1 && actingRole != Role.None)
            return orbRole != actingRole; // player must clear acting debuff, or he will die

        var playerRole = player.Role == Role.Ranged ? Role.Melee : player.Role;
        return orbRole == playerRole;
    }

    private void ModifyRuinStacks(Actor actor, ushort count)
    {
        int slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerRuinCount[slot] = count;
    }

    private void ModifyActingRole(Actor actor, Role role)
    {
        int slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerActingRole[slot] = role;
    }
}
