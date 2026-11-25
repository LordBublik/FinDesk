using System;
using System.Collections.Generic;

namespace FinDesk.Models;

public partial class Category
{
    public Category()
    {
        Transactions = new HashSet<Transaction>();
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }
}