namespace Application.Interfaces.Authentification
{
  public interface IJwtTokenGenerator
  {
    public string GenerateToken(int id, string name);
  }
}
