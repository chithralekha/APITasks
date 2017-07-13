using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.DTO
{
    public class WorkingSet
    {
        public int Id { get; set; }
        public Guid WorkingSetGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Deployed { get; set; }
        public User DeployedByUser { get; set; }
        public int? WorkingSetCompliance { get; set; }
        public WorkingSetTemplate WorkingSetTemplate { get; set; }
        public WorkingSetDataPoint WorkingSetDataPoint { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}
