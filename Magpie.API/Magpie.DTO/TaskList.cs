using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.DTO
{
    public class TaskList
    {
        public TaskListMetadata Metadata { get; set; }
        public IEnumerable<DTO.Task> Tasks { get; set; }
    }
}
