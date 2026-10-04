using System.Security.Cryptography;
using System.Text;

namespace Shared.Cryptography.Application.Cryptography.Contract;
/// <summary>
/// Represents the Authenticity component.
/// </summary>
public class Authenticity: IAuthenticity
{
		public string Hash(string text, HashAlgorithm algorithm)
	{
		using (algorithm)
		{
			byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
			return string.Join("", bytes.Select(x => x.ToString("x2")));
		}
	}
}
