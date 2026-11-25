using System;
using System.Collections.Generic;

namespace FinDesk.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }
}
