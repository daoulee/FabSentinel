using GreenVision.Models;

namespace GreenVision.Services;

public interface IVisionInspectionService
{
    IReadOnlyList<InspectionResult> History { get; }
    bool IsCameraConnected { get; }
    event EventHandler<InspectionResult>? InspectionCompleted;
    Task<InspectionResult> RunInspectionAsync(CancellationToken ct = default);
    Task<bool> ConnectCameraAsync(int cameraIndex = 0);
}
