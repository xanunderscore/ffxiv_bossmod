namespace BossMod;

// information relevant for AI decision making process for a specific player
public sealed class AIHints
{
    public class Enemy(Actor actor, bool shouldBeTanked)
    {
        // TODO: split 'pointless to attack' (eg invulnerable, but fine to hit by aoes) vs 'actually bad to hit' (eg can lead to wipe)
        public const int PriorityForbidAI = -1; // ai is forbidden from attacking this enemy, but player explicitly targeting it is not (e.g. out of combat enemies that we might not want to pull)
        public const int PriorityForbidFully = -2; // attacking this enemy is forbidden both by ai or player (e.g. invulnerable, or attacking/killing might lead to a wipe)

        public Actor Actor = actor;
        public int Priority = actor.InCombat ? 0 : PriorityForbidAI; // <0 means damaging is actually forbidden, 0 is default (TODO: revise default...)
        //public float TimeToKill;
        public float AttackStrength = 0.05f; // target's predicted HP percent is decreased by this amount (0.05 by default)
        public WPos DesiredPosition = actor.Position; // tank AI will try to move enemy to this position
        public Angle DesiredRotation = actor.Rotation; // tank AI will try to rotate enemy to this angle
        public float TankDistance = 2; // enemy will start moving if distance between hitboxes is bigger than this
        public bool ShouldBeTanked = shouldBeTanked; // tank AI will try to tank this enemy
        public bool PreferProvoking; // tank AI will provoke enemy if not targeted
        public bool ForbidDOTs; // if true, dots on target are forbidden
        public bool ShouldBeInterrupted; // if set and enemy is casting interruptible spell, some ranged/tank will try to interrupt
        public bool ShouldBeStunned; // if set, AI will stun if possible
        public bool StayAtLongRange; // if set, players with ranged attacks don't bother coming closer than max range (TODO: reconsider)
    }

    public class AllyState(List<Actor> allies)
    {
        public List<Actor> Allies = allies;
        public void Clear() => Allies.Clear();

        public void Add(Actor ally) => Allies.Add(ally);

        public IEnumerable<(int, Actor)> WithSlot() => Allies.Select((a, i) => (i, a));
        public IEnumerable<Actor> WithoutSlot() => Allies;
        public int Count => Allies.Count;
        public Actor this[int Index] => Allies[Index];

        public int FindSlot(ulong instanceID) => instanceID != 0 ? Allies.FindIndex(a => a.InstanceID == instanceID) : -1;
    }

    public enum SpecialMode
    {
        Normal,
        Pyretic, // pyretic/acceleration bomb type of effects - no movement, no actions, no casting allowed at activation time
        Freezing, // should be moving at activation time
        // TODO: misdirection, etc
    }

    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(30);

    public WPos Center;
    public ArenaBounds Bounds = DefaultBounds;

    // list of potential targets
    public List<Enemy> PotentialTargets = [];
    public int HighestPotentialTargetPriority;

    public AllyState Allies = new([]);

    // forced target
    // this should be set only if either explicitly planned by user or by ai, otherwise it will be annoying to user
    public Actor? ForcedTarget;

    // low-level forced movement - if set, character will move in specified direction (ignoring casts, uptime, forbidden zones, etc), or stay in place if set to default
    public Vector3? ForcedMovement;

    // indicates to AI mode that it should try to interact with some object
    public Actor? InteractWithTarget;

    // positioning: list of shapes that are either forbidden to stand in now or will be in near future
    // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
    public List<(Func<WPos, float> shapeDistance, DateTime activation)> ForbiddenZones = [];

    // positioning: rough target & radius of the movement; if not set, uses either target's position or module center instead
    // used to somewhat prioritize movement direction and optimize pathfinding
    public WPos? PathfindingHintDestination;
    public float? PathfindingHintRadius;

    // positioning: next positional hint (TODO: reconsider, maybe it should be a list prioritized by in-gcds, and imminent should be in-gcds instead? or maybe it should be property of an enemy? do we need correct?)
    public (Actor? Target, Positional Pos, bool Imminent, bool Correct) RecommendedPositional;

    // positioning: recommended range to target (TODO: reconsider?)
    public float RecommendedRangeToTarget;

    // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
    // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
    public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = [];

    // closest special movement/targeting/action mode, if any
    public (SpecialMode mode, DateTime activation) ImminentSpecialMode;

    // predicted incoming damage (raidwides, tankbusters, etc.)
    // AI will attempt to shield & mitigate
    public List<(BitMask players, DateTime activation)> PredictedDamage = [];

    // actions that we want to be executed, gathered from various sources (manual input, autorotation, planner, ai, modules, etc.)
    public ActionQueue ActionsToExecute = new();

    // buffs to be canceled asap
    public List<(uint statusId, ulong sourceId)> StatusesToCancel = [];

    // dismount
    public bool Dismount;

    // clear all stored data
    public void Clear()
    {
        Center = default;
        Dismount = false;
        Bounds = DefaultBounds;
        PotentialTargets.Clear();
        Allies.Clear();
        ForcedTarget = null;
        ForcedMovement = null;
        InteractWithTarget = null;
        ForbiddenZones.Clear();
        PathfindingHintDestination = null;
        PathfindingHintRadius = null;
        RecommendedPositional = default;
        RecommendedRangeToTarget = 0;
        ForbiddenDirections.Clear();
        ImminentSpecialMode = default;
        PredictedDamage.Clear();
        ActionsToExecute.Clear();
        StatusesToCancel.Clear();
    }

    // fill list of potential targets from world state
    public void FillPotentialTargets(WorldState ws, bool playerIsDefaultTank)
    {
        bool playerInFate = ws.Client.ActiveFate.ID != 0 && ws.Party.Player()?.Level <= Service.LuminaRow<Lumina.Excel.GeneratedSheets.Fate>(ws.Client.ActiveFate.ID)?.ClassJobLevelMax;
        var allowedFateID = playerInFate ? ws.Client.ActiveFate.ID : 0;
        foreach (var actor in ws.Actors.Where(a => a.IsTargetable && !a.IsAlly && !a.IsDead))
        {
            // fate mob in fate we are NOT a part of, skip entirely. it's okay to "attack" these (i.e., they won't be added as forbidden targets) because we can't even hit them
            // (though aggro'd mobs will continue attacking us after we unsync, but who really cares)
            if (actor.FateID > 0 && actor.FateID != allowedFateID)
                continue;

            // target is dying; skip it so that AI retargets, but ensure that it's not marked as a forbidden target
            // skip this check on striking dummies (name ID 541) as they die constantly
            var predictedHP = ws.PendingEffects.PendingHPDifference(actor.InstanceID);
            if (actor.HPMP.CurHP + predictedHP <= 0 && actor.NameID != 541)
                continue;

            var allowedAttack = actor.InCombat && ws.Party.FindSlot(actor.TargetID) >= 0;
            // enemies in our enmity list can also be attacked, regardless of who they are targeting (since they are keeping us in combat)
            allowedAttack |= actor.AggroPlayer;
            // all fate mobs can be attacked if we are level synced (non synced mobs are skipped above)
            allowedAttack |= actor.FateID > 0;

            PotentialTargets.Add(new(actor, playerIsDefaultTank)
            {
                Priority = allowedAttack ? 0 : Enemy.PriorityForbidAI
            });
        }
    }

    public void FillAllies(WorldState ws) => Allies.Allies.AddRange(ws.Party.WithoutSlot(partyOnly: true));

    public void PrioritizeTargetsByOID(uint oid, int priority = 0) => PrioritizeTargetsByOID([oid], priority);
    public void PrioritizeTargetsByOID<OID>(OID oid, int priority = 0) where OID : Enum => PrioritizeTargetsByOID((uint)(object)oid, priority);

    public void PrioritizeTargetsByOID(uint[] oids, int priority = 0)
    {
        foreach (var h in PotentialTargets)
            if (oids.Contains(h.Actor.OID))
                h.Priority = Math.Max(priority, h.Priority);
    }
    public void InteractWithOID(WorldState ws, uint oid)
    {
        InteractWithTarget = ws.Actors.FirstOrDefault(a => a.OID == oid && a.IsTargetable);
    }
    public void InteractWithOID<OID>(WorldState ws, OID oid) where OID : Enum => InteractWithOID(ws, (uint)(object)oid);

    public void AddForbiddenZone(Func<WPos, float> shapeDistance, DateTime activation = new()) => ForbiddenZones.Add((shapeDistance, activation));
    public void AddForbiddenZone(AOEShape shape, WPos origin, Angle rot = new(), DateTime activation = new()) => ForbiddenZones.Add((shape.Distance(origin, rot), activation));

    public void AddSpecialMode(SpecialMode mode, DateTime activation)
    {
        if (ImminentSpecialMode == default || ImminentSpecialMode.activation > activation)
            ImminentSpecialMode = (mode, activation);
    }

    // normalize all entries after gathering data: sort by priority / activation timestamp
    // TODO: note that the name is misleading - it actually happens mid frame, before all actions are gathered (eg before autorotation runs), but further steps (eg ai) might consume previously gathered data
    public void Normalize()
    {
        PotentialTargets.SortByReverse(x => x.Priority);
        HighestPotentialTargetPriority = Math.Max(0, PotentialTargets.FirstOrDefault()?.Priority ?? 0);
        ForbiddenZones.SortBy(e => e.activation);
        ForbiddenDirections.SortBy(e => e.activation);
        PredictedDamage.SortBy(e => e.activation);
    }

    // query utilities
    public IEnumerable<Enemy> PotentialTargetsEnumerable => PotentialTargets;
    public IEnumerable<Enemy> PriorityTargets => PotentialTargets.TakeWhile(e => e.Priority == HighestPotentialTargetPriority);
    public IEnumerable<Enemy> ForbiddenTargets => PotentialTargetsEnumerable.Reverse().TakeWhile(e => e.Priority < 0);

    // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
    public int NumPriorityTargetsInAOE(Func<Enemy, bool> pred) => ForbiddenTargets.Any(pred) ? 0 : PriorityTargets.Count(pred);
    public int NumPriorityTargetsInAOECircle(WPos origin, float radius) => NumPriorityTargetsInAOE(a => TargetInAOECircle(a.Actor, origin, radius));
    public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle) => NumPriorityTargetsInAOE(a => TargetInAOECone(a.Actor, origin, radius, direction, halfAngle));
    public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => NumPriorityTargetsInAOE(a => TargetInAOERect(a.Actor, origin, direction, lenFront, halfWidth, lenBack));
    public bool TargetInAOECircle(Actor target, WPos origin, float radius) => target.Position.InCircle(origin, radius + target.HitboxRadius);
    public bool TargetInAOECone(Actor target, WPos origin, float radius, WDir direction, Angle halfAngle) => target.Position.InCircleCone(origin, radius + target.HitboxRadius, direction, halfAngle);
    public bool TargetInAOERect(Actor target, WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => target.Position.InRect(origin, direction, lenFront + target.HitboxRadius, lenBack, halfWidth);

    public WPos ClampToBounds(WPos position) => Center + Bounds.ClampToBounds(position - Center);

    public record struct PartyMemberHealthStatus(List<PartyMemberState> Members) : IEnumerable<PartyMemberState>
    {
        public readonly IEnumerator<PartyMemberState> GetEnumerator() => Members.GetEnumerator();
        public readonly PartyMemberState this[int Index] => Members[Index];
        public readonly PartyHealthState GetPartyHealth() => GetPartyHealth(_ => true);
        public readonly PartyHealthState GetPartyHealth(Func<Actor, bool> actorFilter)
        {
            int count = 0;
            float mean = 0;
            float m2 = 0;
            float min = float.MaxValue;
            int minSlot = -1;

            for (var i = 0; i < Members.Count; i++)
            {
                var p = Members[i];
                if (!actorFilter(p.Actor))
                    continue;

                if (p.NoHealStatusRemaining > 1.5f && p.DoomRemaining == 0)
                    continue;

                var pred = p.DoomRemaining > 0 ? 0 : p.PredictedHPRatio;
                if (pred < min)
                {
                    min = pred;
                    minSlot = i;
                }
                count++;
                var delta = pred - mean;
                mean += delta / count;
                var delta2 = pred - mean;
                m2 += delta * delta2;
            }
            var variance = m2 / count;
            return new PartyHealthState()
            {
                LowestHPSlot = minSlot,
                Avg = mean,
                StdDev = MathF.Sqrt(variance),
                Count = count
            };
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Members).GetEnumerator();
    }

    public record PartyMemberState(Actor Actor)
    {
        public Actor Actor = Actor;
        public int PredictedHP;
        public int PredictedHPMissing;
        public float AttackerStrength;
        // predicted ratio including pending HP loss and current attacker strength
        public float PredictedHPRatio;
        // *actual* ratio including pending HP loss, used mainly just for essential dignity
        public float PendingHPRatio;
        // remaining time on cleansable status, to avoid casting it on a target that will lose the status by the time we finish
        public float EsunableStatusRemaining;
        // tank invulns go here, but also statuses like Excog that give burst heal below a certain HP threshold
        // no point in spam healing a tank in an area with high mob density (like Sirensong Sea pull after second boss) until their excog falls off
        public float NoHealStatusRemaining;
        // Doom (1769 and possibly other statuses) is only removed once a player reaches full HP, must be healed asap
        public float DoomRemaining;
    }

    public record PartyHealthState
    {
        public int LowestHPSlot;
        public int Count;
        public float Avg;
        public float StdDev;
    }

    private static readonly uint[] NoHealStatuses = [
        82, // Hallowed Ground
        409, // Holmgang
        810, // Living Dead
        811, // Walking Dead
        1220, // Excogitation
        1836, // Superbolide
        2685, // Catharsis of Corundum
        (uint)WAR.SID.BloodwhettingDefenseLong
    ];

    public PartyMemberHealthStatus CalcPartyMemberHealth(WorldState ws)
    {
        BitMask esunas = new();
        foreach (var caster in Allies.WithoutSlot().Where(a => a.CastInfo?.IsSpell(WHM.AID.Esuna) ?? false))
            esunas.Set(Allies.FindSlot(caster.TargetID));

        float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - ws.CurrentTime).TotalSeconds, 0.0f);

        List<PartyMemberState> states = [];

        for (var i = 0; i < Allies.Count; i++)
        {
            var actor = Allies[i];
            var state = new PartyMemberState(actor)
            {
                EsunableStatusRemaining = 0,
                NoHealStatusRemaining = 0
            };
            if (actor.IsDead || actor.HPMP.MaxHP == 0)
            {
                state.PredictedHP = state.PredictedHPMissing = 0;
                state.PredictedHPRatio = state.PendingHPRatio = 1;
            }
            else
            {
                state.PredictedHP = (int)actor.HPMP.CurHP + ws.PendingEffects.PendingHPDifference(actor.InstanceID);
                state.PredictedHPMissing = (int)actor.HPMP.MaxHP - state.PredictedHP;
                state.PredictedHPRatio = state.PendingHPRatio = (float)state.PredictedHP / actor.HPMP.MaxHP;
                var canEsuna = actor.IsTargetable && !esunas[i];
                foreach (var s in actor.Statuses)
                {
                    if (canEsuna && Utils.StatusIsRemovable(s.ID))
                        state.EsunableStatusRemaining = Math.Max(StatusDuration(s.ExpireAt), state.EsunableStatusRemaining);

                    if (NoHealStatuses.Contains(s.ID))
                        state.NoHealStatusRemaining = StatusDuration(s.ExpireAt);

                    if (s.ID == 1769)
                        state.DoomRemaining = StatusDuration(s.ExpireAt);
                }
            }
            states.Add(state);
        }

        foreach (var enemy in PotentialTargets)
        {
            var targetSlot = Allies.FindSlot(enemy.Actor.TargetID);
            if (targetSlot >= 0)
            {
                var state = states[targetSlot];

                state.AttackerStrength += enemy.AttackStrength;
                if (state.PredictedHPRatio < 0.99f)
                    state.PredictedHPRatio -= enemy.AttackStrength;
            }
        }

        return new(states);
    }
}
