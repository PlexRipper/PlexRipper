namespace PlexRipper.BaseTests.DTO;

public class TestCredentialsDTO
{
    public List<TestAccountDTO> Credentials { get; set; }
}

public class TestAccountDTO
{
    public string Username { get; set; }

    public string Password { get; set; }
}
