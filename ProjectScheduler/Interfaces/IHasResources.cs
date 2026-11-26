using System;
using System.Collections.Generic;

namespace ProjectScheduler.Interfaces
{
    public interface IHasResources
    {
        List<Guid> ResourceIds { get; }
    }
}
