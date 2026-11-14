using System;
using System.Text.Json;
using Windows.Storage;
using ProjectScheduler.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            return JsonSerializer.Deserialize<List<ResourceItem>>(
                json,
                JsonOptions
            ) ?? new List<ResourceItem>();
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
