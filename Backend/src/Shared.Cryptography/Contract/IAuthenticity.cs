using System.Security.Cryptography;

namespace Shared.Cryptography.Application.Cryptography.Contract;

/// <summary>
/// Interface for data authentication process.
/// </summary>
public interface IAuthenticity
{
	/// <summary>
	/// Hash text.
	/// </summary>
	/// <param name="text">The text to hash.</param>
	/// <param name="algorithm">The hashing algorithm to use.</param>
	/// <returns>Hashed text.</returns>
	string Hash(string text, HashAlgorithm algorithm);
}
