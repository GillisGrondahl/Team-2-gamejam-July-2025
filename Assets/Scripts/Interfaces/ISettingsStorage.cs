public interface ISettingsStorage
{
    bool TryLoad(out GameSettings settings);
    void Save(GameSettings settings);
}
