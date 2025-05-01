using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Interfaces.Authentification;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentification
{
  internal class JwtTokenGenerator : IJwtTokenGenerator
  {
    private readonly JwtSettings _jwtSettings;
    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
      _jwtSettings = jwtOptions.Value;
    }
    public string GenerateToken(int id, string name)
    {
      var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)), SecurityAlgorithms.HmacSha256);
      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
        new Claim(JwtRegisteredClaimNames.GivenName, name.ToString()),

      };
      var securityToken = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
        claims: claims,
        signingCredentials: signingCredentials);

      return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
  }
}
