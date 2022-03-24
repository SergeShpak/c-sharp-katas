namespace SameHashCode;

using System;
using System.Collections.Generic;

public struct StringsWithSameHash
{
    public StringsWithSameHash(string first, string second, string third)
    {
        First = first;
        Second = second;
        Third = third;
    }
    public string First;
    public string Second;
    public string Third;

    public override string ToString()
    {
        return $"(\"{First}\", \"{Second}\", \"{Third})\"";
    }
}

public static class Searcher
{
    public class HashCollisionNotFoundException : Exception
    {
        public HashCollisionNotFoundException() : base() { }
        public HashCollisionNotFoundException(string message) : base(message) { }
        public HashCollisionNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public static StringsWithSameHash FindStringsWithSameHash()
    {
        return FindStringsWithSameHash(10);
    }

    public static StringsWithSameHash FindStringsWithSameHash(int lengthLimit)
    {
        Random randGen = new Random();
        var alphabet = generateAsciiSet();
        var (first, second) = performBirthdayParadoxAttack(alphabet, lengthLimit: lengthLimit);
        Console.WriteLine("Found a collision");
        var third = findCollision(alphabet, new string[] { first, second });
        Console.WriteLine("Found another collision");
        var result = new StringsWithSameHash(
            first,
            second,
            third
        );
        Console.WriteLine(result);
        return result;
    }
    private static Func<string, bool> generateParadoxAttackCheck(Dictionary<int, string> foundHashes)
    {
        return delegate (string sToCheck)
        {
            var hashToCheck = sToCheck.GetHashCode();
            if (foundHashes.ContainsKey(hashToCheck))
            {
                return true;
            }
            foundHashes.Add(hashToCheck, sToCheck);
            return false;
        };
    }

    private static (string, string) performBirthdayParadoxAttack(char[] alphabet, int startLength = 5, int lengthLimit = 10)
    {
        var foundHashes = new Dictionary<int, string>();
        var foundCollision = generateAllStrings(generateParadoxAttackCheck(foundHashes), alphabet, startLength, lengthLimit);
        string? first = "";
        if (!foundHashes.TryGetValue(foundCollision.GetHashCode(), out first))
        {
            throw new Exception("value that provoke a collision not found");
        }
        return ((string)first, foundCollision);
    }

    private static string findCollision(char[] alphabet, string[] foundStrings, int startLength = 5, int lengthLimit = 10)
    {
        var foundCollision = generateAllStrings(generateFindCollisionCheck(foundStrings), alphabet, startLength, lengthLimit);
        return foundCollision;
    }

    private static Func<string, bool> generateFindCollisionCheck(string[] foundStrings)
    {
        return delegate (string sToCheck)
        {
            var hashToCheck = sToCheck.GetHashCode();
            if (hashToCheck != foundStrings[0].GetHashCode())
            {
                return false;
            }
            for (var i = 0; i < foundStrings.Length; i++)
            {
                if (String.Equals(foundStrings[i], sToCheck))
                {
                    return false;
                }
            }
            return true;
        };
    }

    private static string generateAllStrings(Func<string, bool> check, char[] alphabet, int startLength, int lengthLimit)
    {
        if (lengthLimit < 1)
        {
            throw new ArgumentException($"generateAllStrings failed: expected the length limit parameter to be greater than 0, got {lengthLimit}");
        }
        int currLen = startLength;
        while (currLen <= lengthLimit)
        {
            var acc = new Char[currLen];
            var result = generateAllStringsAux(check, alphabet, acc, currLen);
            if (result is not null)
            {
                return result;
            }
            currLen++;
        }
        throw new HashCollisionNotFoundException();
    }

    private static string? generateAllStringsAux(Func<string, bool> check, char[] alphabet, char[] acc, int remaining)
    {
        if (remaining == 0)
        {
            var result = new String(acc);
            if (check(result))
            {
                return result;
            }
            return null;
        }
        foreach (char c in alphabet)
        {
            acc[acc.Length - remaining] = c;
            var intermidiateResult = generateAllStringsAux(check, alphabet, acc, remaining - 1);
            if (intermidiateResult is not null)
            {
                return intermidiateResult;
            }
        }
        return null;
    }

    private static string getRandomString(Char[] alphabet, int length, Random randGen)
    {
        if (length < 0)
        {
            throw new ArgumentException($"expected a positive length argument, got {length}");
        }
        if (alphabet.Length == 0 || length == 0)
        {
            return String.Empty;
        }
        if (alphabet.Length == 1)
        {
            return new String(alphabet[0], length);
        }
        return new String(
            System.Linq.Enumerable.
                Range(0, length - 1).
                Select(_ => alphabet[randGen.Next(alphabet.Length)]).
                ToArray()
        );
    }

    private static Char[] generateAsciiSet()
    {
        int asciiLower = 32, asciiUpper = 126;
        return generateCharSet(asciiLower, asciiUpper);
    }

    private static Char[] generateCharSet(int lowerCode, int upperCode)
    {
        const int lowerLimit = 32, upperLimit = 126;
        if ((lowerCode < lowerLimit) || (lowerCode > upperLimit))
        {
            throw new ArgumentException($"expected lowerCode to represent a printable ASCII value ({lowerLimit} <= lowerCode <= {upperLimit}), got {lowerCode}");
        }
        if ((upperCode < lowerLimit) || (lowerCode > upperLimit))
        {
            throw new ArgumentException($"expected lowerCode to represent a printable ASCII value ({lowerLimit} <= lowerCode <= {upperLimit}), got {lowerCode}");
        }
        if (lowerCode > upperCode)
        {
            throw new ArgumentException($"passed lowerCode value ({lowerCode}) is greater than the upperCode value ({upperCode})");
        }
        var diff = upperCode - lowerCode + 1;
        Char[] alphabet = new Char[diff];
        for (var i = 0; i < diff; i++)
        {
            alphabet[i] = (Char)(lowerCode + i);
        }
        return alphabet;
    }
}
