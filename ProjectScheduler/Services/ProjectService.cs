using ProjectScheduler.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectScheduler.Services
{
    internal class ProjectService
    {
        private const string FileName = "projects.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<ProjectItem>> LoadAsync()
        {
            var file = await ApplicationData.Current.LocalFolder
                .TryGetItemAsync(FileName) as StorageFile;

            if (file == null)
            {
                return new List<ProjectItem>();
            }

            string json = await FileIO.ReadTextAsync(file);

            return JsonSerializer.Deserialize<List<ProjectItem>>(
                json,
                JsonOptions
            ) ?? new List<ProjectItem>();
        }

        public async Task SaveAsync(IEnumerable<ProjectItem> projects)
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(
                    FileName,
                    CreationCollisionOption.ReplaceExisting);

            string json = JsonSerializer.Serialize(projects, JsonOptions);

            await FileIO.WriteTextAsync(file, json);
        }
    }
}
