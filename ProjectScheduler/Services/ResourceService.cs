using System;
using System.Text.Json;
using Windows.Storage;
using ProjectScheduler.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace ProjectScheduler.Services
{
    internal class ResourceService
    {
        private const string FileName = "resources.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public async Task<List<ResourceItem>> LoadAsync()
        {
            var file = await ApplicationData.Current.LocalFolder
                .TryGetItemAsync(FileName) as StorageFile;

            if (file == null)
            {
                return new List<ResourceItem>();
            }

            string json = await FileIO.ReadTextAsync(file);

            JsonNode? jsonNode = JsonNode.Parse(json);

            // Convert the legacy Directory property to Location (v0.6.0)
            if (jsonNode is JsonArray resourcesArray)
            {
                foreach (JsonNode? resourceNode in resourcesArray)
                {
                    if (resourceNode is not JsonObject resourceObject)
                    {
                        continue;
                    }

                    // Only use Directory if Location doesn't already exist
                    if (!resourceObject.ContainsKey(nameof(ResourceItem.Location)) &&
                        resourceObject.TryGetPropertyValue(
                            "Directory",
                            out JsonNode? legacyLocation))
                    {
                        resourceObject[nameof(ResourceItem.Location)] = 
                            legacyLocation?.DeepClone();
                    }

                    // Remove the old property regardless
                    resourceObject.Remove("Directory");
                }
            }

            return jsonNode?.Deserialize<List<ResourceItem>>(JsonOptions)
                ?? new List<ResourceItem>();
        }

        public async Task SaveAsync(
            IEnumerable<ResourceItem> resources)
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(
                    FileName,
                    CreationCollisionOption.ReplaceExisting
                );

            string json = JsonSerializer.Serialize(
                resources,
                JsonOptions
            );

            await FileIO.WriteTextAsync(file, json);
        }

    }
}
