using System.Text;

namespace RomanNumbers;

/// <summary>
/// Converts numbers between 1-50 to Roman
/// NOTE: this code is out of scope.
/// </summary>
internal static class RomanNumberConverter
{
	private static readonly string[] Tens = { "", "X", "XX", "XXX", "XL", "L" };
	private static readonly string[] Ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
	private static readonly Random Random = new();

	public static string Convert(int number)
	{
		StringBuilder sb = new();
		sb.Append(Tens[number / 10]);
		sb.Append(Ones[(number % 10)]);
		return sb.ToString();
	}

	public static int[] GenerateTestArray(int arrayLength) =>
		Enumerable
			.Range(1, arrayLength)
			.Select(num => Random.Next(1, 51))
			.ToArray();
}
