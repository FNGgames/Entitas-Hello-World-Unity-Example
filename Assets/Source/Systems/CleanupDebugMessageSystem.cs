using Entitas;

public class CleanupDebugMessageSystem : ICleanupSystem
{
    readonly GameContext _context;
    readonly IGroup<GameEntity> _debugMessages;

    public CleanupDebugMessageSystem(Contexts contexts)
    {
        _context = contexts.game;
        _debugMessages = _context.GetGroup(GameMatcher.DebugMessage);
    }

    public void Cleanup()
    {
        foreach (var e in _debugMessages.GetEntities())
        {
            _context.DestroyEntity(e);
        }
    }
}
