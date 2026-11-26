using ProjectScheduler.Models;
using ProjectScheduler.Services;
using ProjectScheduler.ViewModels.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectScheduler.ViewModels
{
    internal sealed class ProjectViewModel
        : ListViewModel<ProjectItem>
    {
        private readonly ProjectService _projectService;

        public ProjectViewModel()
            : this(new ProjectService())
        {

        }

        internal ProjectViewModel(
            ProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task LoadAsync()
        {
            List<ProjectItem> savedProjects =
                await _projectService.LoadAsync();

            ReplaceItems(savedProjects);
        }

        public void AddProject(ProjectItem project)
        {
            AddItem(project);
            SelectedItem = project;
        }

        public void DeleteProject(ProjectItem project)
        {
            RemoveItem(project);

            if (SelectedItem?.Id == project.Id)
            {
                SelectedItem = null;
            }
        }

        public async Task SaveAsync()
        {
            await _projectService.SaveAsync(Items);
        }
    }
}
