using Microsoft.JSInterop;

namespace FitFuel.Client.Services
{
    public class DialogService : IDialogService
    {
        private readonly IJSRuntime _jsRuntime;

        public DialogService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool?> ShowMessageBox(string title, string message, string? yesText = "OK", string? cancelText = null)
        {
            // If there's no cancel option, show a simple alert
            if (string.IsNullOrEmpty(cancelText))
            {
                await _jsRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
                return true;
            }
            
            // Otherwise, show a confirmation dialog
            var confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", $"{title}\n\n{message}");
            return confirmed;
        }
    }
}