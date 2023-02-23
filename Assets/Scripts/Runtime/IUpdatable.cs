
public interface IUpdatable
{
    bool isActiveAndEnabled { get; }

    bool isDestroyed { get; }

    void OnUpdate(string type);
}