using ProjectScheduler.Models;
using ProjectScheduler.Services;
using ProjectScheduler.ViewModels.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectScheduler.ViewModels
{
    internal sealed class ResourceViewModel
        : ListViewModel<ResourceItem>
    {
        private readonly ResourceService _resourceService;

        public ResourceViewModel()
            : this(new ResourceService())
        {

        }

        internal ResourceViewModel(
            ResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        public async Task LoadAsync()
        {
            List<ResourceItem> savedResources =
                await _resourceService.LoadAsync();

            ReplaceItems(savedResources);
        }

        public void AddResource(ResourceItem resource)
        {
            AddItem(resource);
            SelectedItem = resource;
        }

        public void DeleteResource(ResourceItem resource)
        {
            RemoveItem(resource);

            if (SelectedItem?.Id == resource.Id)
            {
                SelectedItem = null;
            }
        }

        public async Task SaveAsync()
        {
            await _resourceService.SaveAsync(Items);
        }
    }
}
