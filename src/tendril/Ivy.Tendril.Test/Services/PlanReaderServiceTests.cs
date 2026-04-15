using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ivy.Tendril.Test.Services;

public class PlanReaderServiceTests
{
    [Fact]
    public void TransitionState_NotifiesPlanWatcher()
    {
        // Arrange
        var mockConfig = new Mock<IConfigService>();
        mockConfig.Setup(c => c.PlanFolder).Returns(Path.GetTempPath());

        var mockLogger = new Mock<ILogger<PlanReaderService>>();
        var mockWatcher = new Mock<IPlanWatcherService>();

        var service = new PlanReaderService(
            mockConfig.Object,
            mockLogger.Object,
            planWatcherService: mockWatcher.Object);

        var folderName = "01234-TestPlan";

        // Create a temporary plan folder and plan.yaml file
        var planFolder = Path.Combine(Path.GetTempPath(), folderName);
        Directory.CreateDirectory(planFolder);
        var planYamlPath = Path.Combine(planFolder, "plan.yaml");
        File.WriteAllText(planYamlPath, "state: Draft\nproject: TestProject\n");

        try
        {
            // Act
            service.TransitionState(folderName, PlanStatus.ReadyForReview);

            // Assert
            mockWatcher.Verify(w => w.NotifyChanged(folderName), Times.Once);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(planFolder))
                Directory.Delete(planFolder, true);
        }
    }

    [Fact]
    public void SaveRevision_NotifiesPlanWatcher()
    {
        // Arrange
        var mockConfig = new Mock<IConfigService>();
        mockConfig.Setup(c => c.PlanFolder).Returns(Path.GetTempPath());

        var mockLogger = new Mock<ILogger<PlanReaderService>>();
        var mockWatcher = new Mock<IPlanWatcherService>();

        var service = new PlanReaderService(
            mockConfig.Object,
            mockLogger.Object,
            planWatcherService: mockWatcher.Object);

        var folderName = "01234-TestPlan";
        var content = "# Test Revision\n\nTest content";

        // Create a temporary plan folder
        var planFolder = Path.Combine(Path.GetTempPath(), folderName);
        Directory.CreateDirectory(planFolder);

        try
        {
            // Act
            service.SaveRevision(folderName, content);

            // Give background write a moment to complete
            Thread.Sleep(100);

            // Assert
            mockWatcher.Verify(w => w.NotifyChanged(folderName), Times.Once);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(planFolder))
                Directory.Delete(planFolder, true);
        }
    }
}
