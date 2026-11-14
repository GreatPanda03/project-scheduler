using ProjectScheduler.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectScheduler.Services
{
    internal class SubslotService
    {
        private const string FileName = "subslots.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public async Task<List<SubslotItem>> LoadAsync()
        {
            var file = await ApplicationData.Current.LocalFolder
                .TryGetItemAsync(FileName) as StorageFile;

            if (file == null)
            {
                return new List<SubslotItem>();
            }

            string json = await FileIO.ReadTextAsync(file);

            return JsonSerializer.Deserialize<List<SubslotItem>>(
                json,
                JsonOptions
            ) ?? new List<SubslotItem>();
        }

        public async Task SaveAsync(
            IEnumerable<SubslotItem> subslots)
        {
            var file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(
                    FileName,
                    CreationCollisionOption.ReplaceExisting
                );

            string json = JsonSerializer.Serialize(
                subslots,
                JsonOptions
            );

            await FileIO.WriteTextAsync(file, json);
        }
    }
}
