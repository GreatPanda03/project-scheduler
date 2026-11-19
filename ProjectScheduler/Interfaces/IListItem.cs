using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectScheduler.Interfaces
{
    public interface IListItem
    {
        Guid Id { get; }
        string Name { get; }
    }
}
