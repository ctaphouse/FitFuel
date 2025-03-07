namespace FitFuel.Client.Services
{
    public interface IDialogService
    {
        Task<bool?> ShowMessageBox(string title, string message, string? yesText = "OK", string? cancelText = null);
    }
}