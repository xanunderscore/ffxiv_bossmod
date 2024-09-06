namespace BossMod.Components;
public abstract class RoleplayModule(BossModule module) : BossComponent(module)
{
    private AIHints? _hints;
    private Actor? _player;
    private float _castTime;
    protected AIHints Hints => _hints!;
    protected Actor Player => _player!;

    protected uint MP;

    protected Roleplay.AID ComboAction => (Roleplay.AID)WorldState.Client.ComboState.Action;

    public abstract void Execute(Actor? primaryTarget);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints, float maxCastTime)
    {
        _hints = hints;
        _player = actor;
        _castTime = maxCastTime;

        MP = (uint)Math.Max(actor.HPMP.CurMP + Module.WorldState.PendingEffects.PendingMPDifference(actor.InstanceID), 0);

        Execute(WorldState.Actors.Find(actor.TargetID));
    }

    protected void UseAction(ActionID action, Actor? target, float additionalPriority = 0, Vector3 targetPos = default)
    {
        var def = ActionDefinitions.Instance[action];

        if (def == null)
            return;

        // not enough time to slidecast; skip
        if (def.CastTime > 0 && _castTime < def.CastTime - 0.5f)
            return;

        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + additionalPriority, targetPos: targetPos);
    }
    protected void UseAction(Roleplay.AID action, Actor? target, float additionalPriority = 0, Vector3 targetPos = default) => UseAction(ActionID.MakeSpell(action), target, additionalPriority, targetPos);

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - WorldState.CurrentTime).TotalSeconds, 0.0f);

    protected uint PredictedHP(Actor actor) => (uint)Math.Max(0, actor.HPMP.CurHP + WorldState.PendingEffects.PendingHPDifference(actor.InstanceID));

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = WorldState.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);
}
