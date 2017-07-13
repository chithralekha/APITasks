using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Model
{
    public class WorkingSetTemplate
    {
        public int WorkingSetTemplateId { get; set; }
        public Guid WorkingSetTemplateGuid { get; set; }
        public string Name { get; set; }
        public User CreatedByUser { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<ControlSet> ControlSets { get; set; }
    }
}
