namespace SameHashCode;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

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
        return $"(\"{First}\", \"{Second}\", \"{Third}\")";
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

    public static StringsWithSameHash FindStringsWithSameHash(bool isParallel = true, int lengthLimit = 10)
    {
        return FindStringsWithSameHash(lengthLimit, isParallel);
    }

    public static StringsWithSameHash FindStringsWithSameHash(int lengthLimit, bool isParallel)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        var result = findStringsWithSameHash(lengthLimit, isParallel);
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}",
            ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10
        );

        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"RunTime (is parallel: {isParallel}) {elapsedTime}");
        return result;
    }

    private static StringsWithSameHash findStringsWithSameHash(int lengthLimit, bool isParallel)
    {
        Random randGen = new Random();
        var alphabet = generateAsciiSet();
        var (first, second) = performBirthdayParadoxAttack(alphabet, lengthLimit: lengthLimit);
        Console.WriteLine("Found a collision");
        var third = findCollision(alphabet, new string[] { first, second }, isParallel: isParallel);
        Console.WriteLine("Found another collision");
        var result = new StringsWithSameHash(
            first,
            second,
            third
        );
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

    private static string findCollision(char[] alphabet, string[] foundStrings, int startLength = 5, int lengthLimit = 10, bool isParallel = true)
    {
        string? foundCollision;
        if (isParallel)
        {
            foundCollision = generateAllStringsInParallel(generateFindCollisionCheck(foundStrings), alphabet, startLength, lengthLimit);
        }
        else
        {
            foundCollision = generateAllStrings(generateFindCollisionCheck(foundStrings), alphabet, startLength, lengthLimit);
        }
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

    private static string generateAllStringsInParallel(Func<string, bool> check, char[] alphabet, int startLength, int lengthLimit)
    {
        var paramsGen = new TasksParamsGenerator(alphabet, startLength, lengthLimit);
        CancellationTokenSource cts = new CancellationTokenSource();
        ParallelOptions po = new ParallelOptions();
        po.CancellationToken = cts.Token;
        po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
        String? result = null;
        var localLockObject = new object();
        try
        {
            Parallel.ForEach(paramsGen.GetTaskParams(), po, (tp, state) =>
                    {
                        var acc = new Char[tp.length];
                        acc[0] = tp.startChar;
                        var taskResult = generateAllStringsAux(check, alphabet, acc, tp.length - 1);
                        if (taskResult != null)
                        {
                            lock (localLockObject)
                            {
                                result = taskResult;
                            }
                            cts.Cancel();
                        }
                    });
        }
        catch (OperationCanceledException e) { }
        finally
        {
            cts.Dispose();
        }
        if (result is null)
        {
            throw new HashCollisionNotFoundException();
        }
        return result;
    }

    private struct TaskParams
    {
        public Char startChar;
        public int length;

        public TaskParams(Char startChar, int length)
        {
            this.startChar = startChar;
            this.length = length;
        }
    }

    private class TasksParamsGenerator
    {
        private Char[] alphabet;
        private int currPos;
        private int currLen;
        private int maxLen;
        private object lockObj;

        public TasksParamsGenerator(Char[] alphabet, int startLen, int maxLen)
        {
            this.alphabet = alphabet;
            this.currLen = startLen;
            this.currPos = 0;
            this.maxLen = maxLen;
            this.lockObj = new Object();
        }

        public IEnumerable<TaskParams> GetTaskParams()
        {
            while (this.currLen <= this.maxLen)
            {
                if (currPos >= this.alphabet.Length)
                {
                    currPos = 0;
                    currLen++;
                }
                var ch = this.alphabet[currPos];
                this.currPos++;
                yield return new TaskParams(ch, this.currLen);
            }
            yield break;
        }
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
