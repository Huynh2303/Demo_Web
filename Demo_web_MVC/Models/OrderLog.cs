using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class OrderLog
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
