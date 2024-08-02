namespace Application.Contracts;

public class AuthPinLocation
{
    public string Code { get; set; }

    public bool EuropeanUnionMember { get; set; }

    public string ContinentCode { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public string TimeZone { get; set; }

    public string PostalCode { get; set; }

    public string Subdivisions { get; set; }

    public string Coordinates { get; set; }
}
