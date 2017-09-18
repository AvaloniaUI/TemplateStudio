// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Fakes;
using Microsoft.Templates.UI;
using Microsoft.VisualStudio.Threading;
using Microsoft.TemplateEngine.Abstractions;

using Xunit;

namespace Microsoft.Templates.Test
{
    public class BaseGenAndBuildTests : BaseTestContextProvider
    {
        protected GenerationFixture _fixture;

        private async Task SetUpFixtureForTestingAsync(string language)
        {
            await _fixture.InitializeFixtureAsync(language, this);
        }

        protected async Task<string> AssertGenerateProjectAsync(Func<ITemplateInfo, bool> projectTemplateSelector, string projectName, string projectType, string framework, string language, Func<ITemplateInfo, string> getName = null, bool cleanGeneration = true)
        {
            await SetUpFixtureForTestingAsync(language);

            var targetProjectTemplate = GenerationFixture.Templates.FirstOrDefault(projectTemplateSelector);

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = await GenerationFixture.SetupProjectAsync(projectType, framework, language, getName);

            if (getName != null)
            {
                GenerationFixture.AddItems(userSelection, GenerationFixture.GetTemplates(framework), getName);
            }

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            var resultPath = Path.Combine(_fixture.TestProjectsPath, projectName);

            // Assert
            Assert.True(Directory.Exists(resultPath));
            Assert.True(Directory.GetFiles(resultPath, "*.*", SearchOption.AllDirectories).Count() > 2);

            // Clean
            if (cleanGeneration)
            {
                Fs.SafeDeleteDirectory(resultPath);
            }

            return resultPath;
        }

        protected void AssertBuildProjectAsync(string projectPath, string projectName)
        {
            // Build solution
            var result = GenerationFixture.BuildSolution(projectName, projectPath);

            // Assert
            Assert.True(result.exitCode.Equals(0), $"Solution {projectName} was not built successfully. {Environment.NewLine}Errors found: {GenerationFixture.GetErrorLines(result.outputFile)}.{Environment.NewLine}Please see {Path.GetFullPath(result.outputFile)} for more details.");

            // Clean
            Fs.SafeDeleteDirectory(projectPath);
        }

        protected async Task<string> AssertGenerateRightClickAsync(string projectName, string projectType, string framework, string language, bool emptyProject, bool cleanGeneration = true)
        {
            await SetUpFixtureForTestingAsync(language);

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestNewItemPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = await GenerationFixture.SetupProjectAsync(projectType, framework, language);

            if (!emptyProject)
            {
                GenerationFixture.AddItems(userSelection, GenerationFixture.GetTemplates(framework), GenerationFixture.GetDefaultName);
            }

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            var project = Path.Combine(_fixture.TestNewItemPath, projectName);

            // Assert on project
            Assert.True(Directory.Exists(project));

            int emptyProjecFileCount = Directory.GetFiles(project, "*.*", SearchOption.AllDirectories).Count();
            Assert.True(emptyProjecFileCount > 2);

            // Add new items
            var rightClickTemplates = GenerationFixture.Templates.Where(
                                            t => (t.GetTemplateType() == TemplateType.Feature || t.GetTemplateType() == TemplateType.Page)
                                              && t.GetFrameworkList().Contains(framework)
                                              && !t.GetIsHidden()
                                              && t.GetRightClickEnabled());

            foreach (var item in rightClickTemplates)
            {
                OutputPath = GenContext.GetTempGenerationPath(projectName);

                var newUserSelection = new UserSelection
                {
                    ProjectType = projectType,
                    Framework = framework,
                    HomeName = "",
                    Language = language,
                    ItemGenerationType = ItemGenerationType.GenerateAndMerge
                };

                GenerationFixture.AddItem(newUserSelection, item, GenerationFixture.GetDefaultName);

                await NewItemGenController.Instance.UnsafeGenerateNewItemAsync(item.GetTemplateType(), newUserSelection);

                NewItemGenController.Instance.UnsafeFinishGeneration(newUserSelection);
            }

            var finalProjectPath = Path.Combine(_fixture.TestNewItemPath, projectName);
            int finalProjectFileCount = Directory.GetFiles(finalProjectPath, "*.*", SearchOption.AllDirectories).Count();

            if (emptyProject)
            {
                Assert.True(finalProjectFileCount > emptyProjecFileCount);
            }
            else
            {
                Assert.True(finalProjectFileCount == emptyProjecFileCount);
            }
            // Clean
            if (cleanGeneration)
            {
                Fs.SafeDeleteDirectory(finalProjectPath);
            }

            return finalProjectPath;
        }
        protected async Task<(string ProjectPath, string ProjecName)> AssertGenerationOneByOneAsync(string itemName, string projectType, string framework, string itemId, string language, bool cleanGeneration = true)
        {
            await SetUpFixtureForTestingAsync(language);

            var projectTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.GetTemplateType() == TemplateType.Project && t.GetProjectTypeList().Contains(projectType) && t.GetFrameworkList().Contains(framework));
            var itemTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.Identity == itemId);
            var finalName = itemTemplate.GetDefaultName();
            var validators = new List<Validator>
            {
                new ReservedNamesValidator(),
            };
            if (itemTemplate.GetItemNameEditable())
            {
                validators.Add(new DefaultNamesValidator());
            }

            finalName = Naming.Infer(finalName, validators);

            var projectName = $"{projectType}{framework}{finalName}";

            ProjectName = projectName;
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, projectName, projectName);
            OutputPath = ProjectPath;

            var userSelection = await GenerationFixture.SetupProjectAsync(projectType, framework, language);

            GenerationFixture.AddItem(userSelection, itemTemplate, GenerationFixture.GetDefaultName);

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            var resultPath = Path.Combine(_fixture.TestProjectsPath, projectName);

            // Assert
            Assert.True(Directory.Exists(resultPath));
            Assert.True(Directory.GetFiles(resultPath, "*.*", SearchOption.AllDirectories).Count() > 2);

            // Clean
            if (cleanGeneration)
            {
                Fs.SafeDeleteDirectory(resultPath);
            }

            return (resultPath, projectName);
        }

        protected async Task<(string ProjectPath, string ProjecName)> SetUpComparisonProjectAsync(string language, string projectType, string framework, IEnumerable<string> genIdentities)
        {
            await SetUpFixtureForTestingAsync(language);

            var projectTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.GetTemplateType() == TemplateType.Project && t.GetProjectTypeList().Contains(projectType) && t.GetFrameworkList().Contains(framework));

            ProjectName = $"{projectType}{framework}Compare{language.Replace("#", "S")}";
            ProjectPath = Path.Combine(_fixture.TestProjectsPath, ProjectName, ProjectName);
            OutputPath = ProjectPath;

            var userSelection = await GenerationFixture.SetupProjectAsync(projectType, framework, language);

            foreach (var identity in genIdentities)
            {
                var itemTemplate = GenerationFixture.Templates.FirstOrDefault(t => t.Identity.Contains(identity)
                                                                                && t.GetFrameworkList().Contains(framework));
                GenerationFixture.AddItem(userSelection, itemTemplate, GenerationFixture.GetDefaultName);

                // Add multiple pages if supported to check these are handled the same
                if (itemTemplate.GetMultipleInstance())
                {
                    GenerationFixture.AddItem(userSelection, itemTemplate, GenerationFixture.GetDefaultName);
                }
            }

            await NewProjectGenController.Instance.UnsafeGenerateProjectAsync(userSelection);

            var resultPath = Path.Combine(_fixture.TestProjectsPath, ProjectName);

            return (resultPath, ProjectName);
        }

        public static IEnumerable<object[]> GetProjectTemplatesAsync()
        {
            JoinableTaskContext context = new JoinableTaskContext();
            JoinableTaskCollection tasks = context.CreateCollection();
            context.CreateFactory(tasks);
            var result = context.Factory.Run(() => GenerationFixture.GetProjectTemplatesAsync());

            return result;
        }

        public static IEnumerable<object[]> GetPageAndFeatureTemplatesAsync(string framework)
        {
            JoinableTaskContext context = new JoinableTaskContext();
            JoinableTaskCollection tasks = context.CreateCollection();
            context.CreateFactory(tasks);
            var result = context.Factory.Run(() => GenerationFixture.GetPageAndFeatureTemplatesAsync(framework));
            return result;
        }


        private const string NavigationPanel = "SplitView";
        private const string Blank = "Blank";
        private const string TabsAndPivot = "TabbedPivot";
        private const string MvvmBasic = "MVVMBasic";
        private const string MvvmLight = "MVVMLight";
        private const string CodeBehind = "CodeBehind";

        // This returns a list of project types and frameworks supported by BOTH C# and VB
        public static IEnumerable<object[]> GetMultiLanguageProjectsAndFrameworks()
        {
            yield return new object[] { NavigationPanel, CodeBehind };
            yield return new object[] { NavigationPanel, MvvmBasic };
            yield return new object[] { NavigationPanel, MvvmLight };
            yield return new object[] { Blank, CodeBehind };
            yield return new object[] { Blank, MvvmBasic };
            yield return new object[] { Blank, MvvmLight };
            yield return new object[] { TabsAndPivot, CodeBehind };
            yield return new object[] { TabsAndPivot, MvvmBasic };
            yield return new object[] { TabsAndPivot, MvvmLight };
        }

        /// <summary>
        /// Gets a list of partial identities for page and feature templates supported by C# and VB
        /// </summary>
        protected static IEnumerable<string> GetPagesAndFeaturesForMultiLanguageProjectsAndFrameworks(string projectType, string framework)
        {
            // Hardcoding the response lists is necessary while there are different pages & features available for different combinations of projecttype and framework
            if (projectType == NavigationPanel && framework == CodeBehind)
            {
                // These are the items being built out for first public trial
                return new[] { "wts.Page.Blank.CodeBehind", "wts.Feat.SettingsStorage", "wts.Feat.SuspendAndResume" };
            }
            else if (framework == CodeBehind)
            {
                // Every combination supports these
                return new[] { "wts.Page.Blank.CodeBehind", "wts.Feat.SettingsStorage", "wts.Feat.SuspendAndResume" };
            }
            else
            {
                // Every combination supports these
                return new[] { "wts.Page.Blank", "wts.Feat.SettingsStorage", "wts.Feat.SuspendAndResume" };
            }
        }
    }
}
