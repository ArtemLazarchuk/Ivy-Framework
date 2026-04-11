namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog(
    List<string> projectNames,
    Action<string, string[]> onCreatePlan,
    Action onClose,
    string[]? defaultProjects = null) : ViewBase
{
    private readonly string[] _defaultProjects = defaultProjects ?? ["[Auto]"];
    private readonly Action _onClose = onClose;
    private readonly Action<string, string[]> _onCreatePlan = onCreatePlan;
    private readonly List<string> _projectNames = projectNames;

    public override object Build()
    {
        var createPlanText = UseState("");
        var selectedProjects = UseState(_defaultProjects);

        var options = new List<string> { "[Auto]" };
        options.AddRange(_projectNames);

        return new Dialog(
            _ => _onClose(),
            new DialogHeader("Create New Plan"),
            new DialogBody(
                Layout.Vertical()
                | selectedProjects.ToSelectInput(options).Variant(SelectInputVariant.Toggle).WithLabel("Select project(s)")
                | createPlanText.ToTextareaInput("Enter task description...").Rows(6).AutoFocus().WithField()
                    .Label("Describe the task for the new plan")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _onClose()),
                new Button("Create").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(createPlanText.Value))
                    {
                        var projects = selectedProjects.Value
                            .Where(p => p != "[Auto]")
                            .ToArray();
                        if (projects.Length == 0) projects = ["[Auto]"];
                        _onCreatePlan(createPlanText.Value, projects);
                        _onClose();
                    }
                })
            )
        ).Width(Size.Rem(30));
    }
}