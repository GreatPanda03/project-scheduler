using System;

namespace ProjectScheduler.Interfaces
{
    public interface IListItem
    {
        Guid Id { get; }
        string Name { get; }
    }
}
