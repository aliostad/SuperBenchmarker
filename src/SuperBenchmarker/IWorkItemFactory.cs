using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public interface IWorkItemFactory
    {
        Task<WorkResult> GetWorkItem();
    }
}
