namespace Application.Contracts;

public class SubscriptionDTO
{
    public bool Active { get; set; }

    public object Status { get; set; }

    public object Plan { get; set; }

    public object Features { get; set; }
}