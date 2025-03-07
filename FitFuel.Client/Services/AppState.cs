// Create this file in the Services folder: Services/AppState.cs
namespace FitFuel.Client.Services
{
    public class AppState
    {
        public bool IsNavMenuExpanded { get; private set; } = false;
        
        public event Action? OnChange;
        
        public void ToggleNavMenu()
        {
            IsNavMenuExpanded = !IsNavMenuExpanded;
            NotifyStateChanged();
        }
        
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}