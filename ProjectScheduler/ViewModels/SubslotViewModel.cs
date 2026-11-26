using ProjectScheduler.Models;
using ProjectScheduler.Services;
using ProjectScheduler.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace ProjectScheduler.ViewModels
{
    internal sealed class SubslotViewModel
        : ListViewModel<SubslotItem>
    {
        private readonly SubslotService _subslotService;

        public SubslotViewModel()
            : this(new SubslotService())
        {

        }

        internal SubslotViewModel(
            SubslotService subslotService)
        {
            _subslotService = subslotService;
        }

        public async Task LoadAsync()
        {
            List<SubslotItem> savedSubslots =
                await _subslotService.LoadAsync();

            ReplaceItems(savedSubslots);
        }

        public void AddSubslot(SubslotItem subslot)
        {
            AddItem(subslot);
            SelectedItem = subslot;
        }

        public void DeleteSubslot(SubslotItem subslot)
        {
            RemoveItem(subslot);

            if (SelectedItem?.Id == subslot.Id)
            {
                SelectedItem = null;
            }
        }

        public async Task SaveAsync()
        {
            await _subslotService.SaveAsync(Items);
        }
    }
}
