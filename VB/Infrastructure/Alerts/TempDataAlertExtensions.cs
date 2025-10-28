using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Alerts;

public static class TempDataAlertExtensions
{
    private const string Key = "SWAL";

    public static void Swal(this Controller controller, SwalMessage message)
        => controller.TempData[Key] = message.ToJson();

    public static void SwalSuccess(this Controller controller, string text, string title = "Done")
        => controller.Swal(new SwalMessage(title, text, "success"));

    public static void SwalError(this Controller controller, string text, string title = "Error")
        => controller.Swal(new SwalMessage(title, text, "error", Toast: false, Position: "center", ShowConfirmButton: true, Timer: 0));

    public static void SwalInfo(this Controller controller, string text, string title = "Info")
        => controller.Swal(new SwalMessage(title, text, "info"));

    public static void SwalWarning(this Controller controller, string text, string title = "Warning")
        => controller.Swal(new SwalMessage(title, text, "warning"));
}
