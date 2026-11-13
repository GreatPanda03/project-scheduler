
using ProjectScheduler.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectScheduler.Services;

public class PreWorkChecklistService
{
    private const string FileName = "prework-checklist.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Load the checklist from local storage.
    public async Task<List<PreWorkItem>> LoadAsync()
    {
        var file = await ApplicationData.Current.LocalFolder
            .TryGetItemAsync(FileName) as StorageFile;

        // If no saved checklist exists, return default items.
        if (file == null)
        {
            return GetDefaultItems();
        }

        string json = await FileIO.ReadTextAsync(file);

        return JsonSerializer.Deserialize<List<PreWorkItem>>(
            json,
            JsonOptions
        ) ?? new List<PreWorkItem>();
    }

    // Save the checklist to local storage.
    public async Task SaveAsync(List<PreWorkItem> items)
    {
        var file = await ApplicationData.Current.LocalFolder
            .CreateFileAsync(
                FileName,
                CreationCollisionOption.ReplaceExisting
            );

        string json = JsonSerializer.Serialize(items, JsonOptions);

        await FileIO.WriteTextAsync(file, json);
    }

    // Items to use when the application is first opened.
    private static List<PreWorkItem> GetDefaultItems()
    {
        return new List<PreWorkItem>
        {
            new() { Title = "Drink a glass of water" },
            new() { Title = "Take a couple of stretches" },
            new() { Title = "Enable your plugins" },
            new() { Title = "Prepare your workspace" }
        };
    }
}
