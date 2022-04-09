using System.Collections.Generic;

namespace PierresTreats.Models
{
  public class Flavor
  {

    public Flavor()
    {
      this.JoinPierresTreats = new HashSet<TreatFlavor>();
    }

    public int FlavorId { get; set; }
    public string Name { get; set; }

    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<TreatFlavor> JoinPierresTreats { get; set; }
  }
}