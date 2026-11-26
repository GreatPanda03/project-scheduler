using ProjectScheduler.Interfaces;
using ProjectScheduler.Models;
using ProjectScheduler.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProjectScheduler.ViewModels
{
    internal sealed class ResourceAssignViewModel
    {
        public IHasResources? Target { get; set; } = null;
        private readonly ResourceService _resourceService = new();
        private List<ResourceItem> _allResources = new();

        public ResourceAssignViewModel()
        {
            LoadResources();
        }

        public async void LoadResources()
        {
            _allResources = await _resourceService.LoadAsync() ?? new();
        }

        public List<ResourceItem> GetAllResources()
        {
            return _allResources;
        }


        // Add Resource
        public bool AddResource(Guid resourceId)
        {
            if (Target is null) return false;

            if (Target.ResourceIds.Contains(resourceId))
                return false;

            Target.ResourceIds.Add(resourceId);

            return true;
        }

        // Remove Resource
        public bool RemoveResource(Guid resourceId)
        {
            if (Target is null) return false;
            return Target.ResourceIds.Remove(resourceId);
        }

        // Open Resource
        public void OpenResource(Guid resourceId)
        {
            ResourceItem? resource = _allResources
                .FirstOrDefault(res => res.Id == resourceId);

            if (resource != null)
            {
                new ResourceOpener().OpenResource(resource);
            }
        }

        // Get Target's Resources
        public List<ResourceItem> GetTargetResources()
        {
            List<ResourceItem> resources = new();

            if (Target is not null)
            {
                foreach (ResourceItem resource in _allResources)
                {
                    if (Target.ResourceIds.Contains(resource.Id))
                        resources.Add(resource);
                }
            }

            return resources;
        }
    }
}
