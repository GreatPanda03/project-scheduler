using ProjectScheduler.Interfaces;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectScheduler.ViewModels
{
    internal sealed class ResourceAssignViewModel
    {
        public IHasResources Target { get; set; }
        private ObservableCollection<ResourceItem> _availableResources = new();

        public ResourceAssignViewModel(IHasResources target)
        {
            Target = target;
        }

        // Add Resource
        public bool AddResource(Guid resourceId)
        {
            if (Target.ResourceIds.Contains(resourceId))
            {
                return false;
            }

            Target.ResourceIds.Add(resourceId);

            return true;
        }

        // Remove Resource
        public bool RemoveResource(Guid resourceId)
        {
            return Target.ResourceIds.Remove(resourceId);
        }

        // Open Resource
        public void OpenResource(Guid resourceId)
        {
            ResourceItem? resource = _availableResources
                .FirstOrDefault(res => res.Id == resourceId);

            if (resource != null)
            {
                new ResourceOpener().OpenResource(resource);
            }
        }
    }
}
