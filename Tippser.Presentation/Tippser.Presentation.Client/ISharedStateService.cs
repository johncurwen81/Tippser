namespace Tippser.Presentation.Client
{
    public interface ISharedStateService
    {
        event Action? OnChange;

        void Set(string data);
    }
}
