using System;
using System.Collections.Generic;

namespace Emart_DotNet.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? HouseNumber { get; set; }

    public string? Landmark { get; set; }

    public string? Pincode { get; set; }

    public string? State { get; set; }

    public string? Town { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Customer? User { get; set; }
}
