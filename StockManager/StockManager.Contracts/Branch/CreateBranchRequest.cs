using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Branch
{
   public record CreateBranchRequest(
       string Name,
            string Town,
            string Phone,
            string Address);
  
}
