using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RomanNumbers;

int[] testSource = RomanNumberConverter.GenerateTestArray(200_000);
int[] availableNumbers = Enumerable.Range(1, 50).ToArray();

/*
	Given a collection of integers – let’s say 200 000 integers,
	ALL ranging between 1 and 50 – what is the simplest / fastest way to return a corresponding
	list of Roman Numeral equivalent strings assuming we *already* have a ConvertIntToRoman(int i) function that returns a string.
	EG: 
	Input collection: 1,3,4,6,1,3,10...N
	Output collection: I,III,IV,VI,I,III,X...N
*/

// Let's add cache for Roman numbers to avoid creating/managing multiple string references
// Alternative would be to cache required numbers locally. check ConvertToRomanUsingLocalCache
Dictionary<int, string> GenerateCacheForNumbers(int[] numbers)
{
	Dictionary<int, string> result = new();
	foreach (int number in numbers)
	{
		result.Add(number, RomanNumberConverter.Convert(number));
	}
	return result;
}

Dictionary<int, string> cache = GenerateCacheForNumbers(availableNumbers);

string[] resultUsingLinq = ConvertToRomanUsingLinq(testSource);
string[] resultUsingSpanLoop = ConvertToRomanUsingSpanLoop(testSource);
string[] resultUsingLocalCache = ConvertToRomanUsingLocalCache(testSource);
string[] resultUsingReference = ConvertToRomanUsingReference(testSource);

string[] ConvertToRomanUsingLinq(int[] source) =>
	source
		.AsParallel()
		.AsOrdered()
		.Select(num => cache[num])
		.ToArray();

string[] ConvertToRomanUsingSpanLoop(int[] source)
{
	string[] result = new string[source.Length];
	Span<int> sourceAsSpan = source;
	for (int i = 0; i < sourceAsSpan.Length; i++)
	{
		int sourceValue = sourceAsSpan[i];
		result[i] = cache[sourceValue];
	}
	return result;
}

string[] ConvertToRomanUsingLocalCache(int[] source)
{
	string[] result = new string[source.Length];
	Dictionary<int, string> localCache = new();
	Span<int> sourceAsSpan = source;
	for (int i = 0; i < sourceAsSpan.Length; i++)
	{
		int sourceValue = sourceAsSpan[i];
		if (localCache.ContainsKey(sourceValue))
		{
			result[i] = localCache[sourceValue];
		}
		else
		{
			string romanNumber = RomanNumberConverter.Convert(sourceValue);
			localCache.Add(sourceValue, romanNumber);
			result[i] = romanNumber;
		}
	}
	return result;
}

// currently the fastest way to loop over arrays (used a lot in microsoft libraries)
string[] ConvertToRomanUsingReference(int[] source)
{
	string[] result = new string[source.Length];
	ref int address = ref MemoryMarshal.GetArrayDataReference(source);
	for (int i = 0; i < source.Length; i++)
	{
		int sourceValue = Unsafe.Add(ref address, i);
		result[i] = cache[sourceValue];
	}
	return result;
}

/*
	Assume while solving problem #1 we find out that it is actually computationally expensive (relatively speaking)
	to convert all integers to Roman, and after speaking to users our requirement gets refined to only providing the top 5 roman values
	by count of occurrence in the source list. Please can you show how you would revise your code from #1 to meet this new requirement.
	EG: Input collection as per #1
	Output collection: I,III,X,V,VII
*/

// let's start with finding top 5 frequent numbers in source array
// no need for .AsOrdered() since we are getting frequent numbers
int[] GetTopFiveIntegersUsingLinq(int[] source) =>
	source
		.AsParallel()
		.GroupBy(num => num)
		.OrderByDescending(g => g.Count())
		.Take(5)
		.Select(g => g.Key)
		.ToArray();

int[] GetTopFiveIntegersWithDictionary(int[] source)
{
	Dictionary<int, int> allCounts = new();
	foreach (var item in source)
	{
		if (allCounts.ContainsKey(item))
		{
			allCounts[item]++;
		}
		else
		{
			allCounts[item] = 1;
		}
	}

	// sort by descending on allCounts values
	var numbersWithCounts = allCounts.ToList();
	numbersWithCounts.Sort((x, y) => y.Value.CompareTo(x.Value));

	int[] result = new int[5];
	for (int i = 0; i < 5; i++)
	{
		result[i] = numbersWithCounts[i].Key;
	}

	return result;
}

int[] revisedSource = GetTopFiveIntegersUsingLinq(testSource);
int[] revisedSourceWithDictionary = GetTopFiveIntegersWithDictionary(testSource);
// let's populate cache with this five numbers since we don't want 50 roman numbers in there
cache = GenerateCacheForNumbers(revisedSource);
// i would use refernce method here because it's fastest. AsParallel wouldn't give any benefit
string[] result = ConvertToRomanUsingReference(revisedSource);


/*
	Bonus question – how would you provide a full summary of the collection from step #1 showing int value, roman equivalent & count.
	EG: Input collection as per #1
	Output collection: 
	1 - I   - 21120
	2 - II  - 8245
	3 - III - 21201
	4 - IV  - 11994
	5 - V   - 20900
	6 - VI  - 15012
	...
	50 - L -  10015
*/

cache = GenerateCacheForNumbers(availableNumbers);

// alternatively we could use SortedDictionary or Basic Dictionary used in GetTopFiveIntegersWithoutLinq to get all counts then sort it by Key instead.
(int, string, int)[] GetSummary(int[] source) =>
	source
		.AsParallel()
		.GroupBy(num => num)
		.OrderBy(g => g.Key)
		.Select(g => (g.Key, cache[g.Key], g.Count()))
		.ToArray();

(int, string, int)[] GetSummaryWithoutLinq(int[] source)
{
	int[] copy = (int[]) source.Clone();
	Array.Sort(copy);
	List<(int, string, int)> result = new();
	int lastNumber = 0;
	int lastIndex = 0;
	for (int i = 0; i < copy.Length; i++)
	{
		int value = copy[i];
		if (value > lastNumber && lastNumber != 0)
		{
			result.Add((lastNumber, cache[lastNumber], i - lastIndex));
			lastIndex = i;
		}
		if (i == copy.Length - 1)
		{
			result.Add((value, cache[value], i + 1 - lastIndex));
		}
		lastNumber = value;
	}
	return result.ToArray();
}

(int, string, int)[] summaryResult = GetSummary(testSource);
(int, string, int)[] summaryResultWithoutLinq = GetSummaryWithoutLinq(testSource);

Console.ReadKey();