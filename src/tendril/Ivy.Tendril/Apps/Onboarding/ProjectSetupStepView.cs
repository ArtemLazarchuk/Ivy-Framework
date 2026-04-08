using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Onboarding;

public class ProjectSetupStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var projectName = UseState("");
        var repoPaths = UseState(new List<string>());
        var newRepoPath = UseState<string?>(null);
        var projectContext = UseState("");
        var error = UseState<string?>(null);
        var verifications = UseState(new List<VerificationEntry>
        {
            new("CheckResult", "Verify the implementation matches the plan requirements.", true)
        });

        // Dialog state for editing verifications
        var editIndex = UseState<int?>(-1); // -1 = closed, null = new, >= 0 = editing index
        var editName = UseState("");
        var editPrompt = UseState("");
        var editRequired = UseState(false);

        var reposLayout = Layout.Vertical().Gap(2);
        var currentRepos = repoPaths.Value;
        for (var i = 0; i < currentRepos.Count; i++)
        {
            var ri = i;
            reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                           | Text.Block(currentRepos[ri]).Width(Size.Grow())
                           | new Button().Icon(Icons.Trash).Ghost().Small().OnClick(() =>
                           {
                               var list = new List<string>(repoPaths.Value);
                               list.RemoveAt(ri);
                               repoPaths.Set(list);
                           });
        }

        reposLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                       | newRepoPath.ToFolderInput("Select repository folder...", mode: FolderInputMode.FullPath)
                           .Width(Size.Grow())
                       | new Button("Add").Outline().Small().OnClick(() =>
                       {
                           if (!string.IsNullOrWhiteSpace(newRepoPath.Value))
                           {
                               var list = new List<string>(repoPaths.Value) { newRepoPath.Value };
                               repoPaths.Set(list);
                               newRepoPath.Set(null);
                           }
                       });

        // Verification list
        var verificationsLayout = Layout.Vertical().Gap(2);
        var currentVerifications = verifications.Value;
        for (var i = 0; i < currentVerifications.Count; i++)
        {
            var vi = i;
            var v = currentVerifications[vi];
            verificationsLayout |= Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                                   | Text.Block(v.Name).Width(Size.Grow())
                                   | (v.Required ? new Badge("Required") : null!)
                                   | new Button().Icon(Icons.Pencil).Ghost().Small().OnClick(() =>
                                   {
                                       editIndex.Set(vi);
                                       editName.Set(verifications.Value[vi].Name);
                                       editPrompt.Set(verifications.Value[vi].Prompt);
                                       editRequired.Set(verifications.Value[vi].Required);
                                   })
                                   | new Button().Icon(Icons.Trash).Ghost().Small().OnClick(() =>
                                   {
                                       var list = new List<VerificationEntry>(verifications.Value);
                                       list.RemoveAt(vi);
                                       verifications.Set(list);
                                   });
        }

        var content = Layout.Vertical().Gap(4)
                      | Text.H2("Project Setup")
                      | Text.Muted("Set up your first project. You can add more projects later in Settings.")
                      | (error.Value != null ? Text.Danger(error.Value) : null!)
                      | projectName.ToTextInput("Project name...").WithField().Label("Project Name")
                      | projectContext.ToTextareaInput("Project context or prompt for AI agents (optional)...")
                          .Rows(4)
                          .WithField()
                          .Label("Context / Prompt (Optional)")
                      | (Layout.Vertical().Gap(2)
                         | Text.Block("Repositories").Bold()
                         | Text.Muted("Add at least one repository path for this project.")
                         | reposLayout)
                      | (Layout.Vertical().Gap(2)
                         | Text.Block("Verifications").Bold()
                         | Text.Muted("Define verifications to run for this project.")
                         | verificationsLayout
                         | new Button("Add Verification").Icon(Icons.Plus).Outline().Small().OnClick(() =>
                         {
                             editIndex.Set(null);
                             editName.Set("");
                             editPrompt.Set("");
                             editRequired.Set(false);
                         }))
                      | (Layout.Horizontal().Gap(2)
                         | new Button("Skip for now").Outline().Large()
                             .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                         | new Button("Next").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                             .OnClick(() =>
                             {
                                 if (string.IsNullOrWhiteSpace(projectName.Value))
                                 {
                                     error.Set("Please enter a project name.");
                                     return;
                                 }

                                 if (repoPaths.Value.Count == 0)
                                 {
                                     error.Set("Please add at least one repository path.");
                                     return;
                                 }

                                 var validVerifications = verifications.Value
                                     .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                                     .ToList();

                                 var project = new ProjectConfig
                                 {
                                     Name = projectName.Value.Trim(),
                                     Color = "Green",
                                     Repos = repoPaths.Value.Select(p => new RepoRef { Path = p, PrRule = "default" })
                                         .ToList(),
                                     Context = projectContext.Value.Trim(),
                                     Verifications = validVerifications.Select(v => new ProjectVerificationRef
                                     {
                                         Name = v.Name,
                                         Required = v.Required
                                     }).ToList()
                                 };

                                 config.SetPendingProject(project);
                                 config.SetPendingVerificationDefinitions(validVerifications
                                     .Select(v => new VerificationConfig
                                     {
                                         Name = v.Name,
                                         Prompt = v.Prompt
                                     }).ToList());

                                 error.Set(null);
                                 stepperIndex.Set(stepperIndex.Value + 1);
                             })
                      );

        // Verification edit dialog
        if (editIndex.Value != -1)
        {
            var isNew = editIndex.Value == null;
            content |= new Dialog(
                _ => editIndex.Set(-1),
                new DialogHeader(isNew ? "Add Verification" : "Edit Verification"),
                new DialogBody(
                    Layout.Vertical().Gap(2)
                    | editName.ToTextInput("Verification name...").WithField().Label("Name")
                    | editPrompt.ToTextareaInput("Verification prompt...").Rows(6).WithField().Label("Prompt")
                    | editRequired.ToBoolInput("Required")
                ),
                new DialogFooter(
                    new Button("Cancel").Outline().OnClick(() => editIndex.Set(-1)),
                    new Button(isNew ? "Add" : "Save").Primary().OnClick(() =>
                    {
                        if (string.IsNullOrWhiteSpace(editName.Value)) return;
                        var list = new List<VerificationEntry>(verifications.Value);
                        if (isNew)
                            list.Add(new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value));
                        else
                            list[editIndex.Value!.Value] =
                                new VerificationEntry(editName.Value, editPrompt.Value, editRequired.Value);
                        verifications.Set(list);
                        editIndex.Set(-1);
                    })
                )
            ).Width(Size.Rem(35));
        }

        return content;
    }

    private record VerificationEntry(string Name, string Prompt, bool Required);
}
