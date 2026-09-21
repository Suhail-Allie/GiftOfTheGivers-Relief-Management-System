using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class ReliefProjectUpdate
{
    public int ProjectUpdateId { get; set; }

    public int ReliefProjectId { get; set; }

    public int EmployeeId { get; set; }

    public string UpdateTitle { get; set; } = null!;

    public string UpdateContent { get; set; } = null!;

    public DateTime PostedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ReliefProject ReliefProject { get; set; } = null!;
}
