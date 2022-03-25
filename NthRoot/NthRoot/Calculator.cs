namespace NthRoot;
public static class Calculator
{
    public static double GetNthRoot(double number, int n)
    {
        if (number < 1 || n < 1)
        {
            throw new ArgumentException($"Expected number >= 1.0 and n >= 1, got number: {number} and n: {n}");
        }
        var rootGuess = guessRoot(number, n);
        return newtonNthRoot(number, n, rootGuess);
    }

    private static double newtonNthRoot(double number, int n, double approximation)
    {
        double prevApprox;
        do
        {
            prevApprox = approximation;
            approximation = ((double)(1.0 / n)) * ((n - 1) * approximation + number / nthPower(approximation, n - 1));
        } while (!areDoublesEqual(prevApprox, approximation));
        return approximation;
    }
    private static double guessRoot(double number, int n)
    {
        if (number == 0)
        {
            return 0;
        }
        var interval = findRootInterval(number, n);
        return findRootApproximation(interval, number, n);
    }

    private static double findRootApproximation((int, int) interval, double number, int n)
    {
        var (lower, upper) = interval;
        while (upper - lower > 1)
        {
            var mid = (upper + lower) / 2;
            var midPow = nthPower(mid, n);
            if (areDoublesEqual(midPow, number))
            {
                return midPow;
            }
            if (midPow < number)
            {
                lower = mid;
            }
            else
            {
                upper = mid;
            }
        }
        var lowerDiff = number - nthPower(lower, n);
        var upperDiff = nthPower(upper, n) - number;
        if (lowerDiff - upperDiff > 0)
        {
            return upper;
        }
        return lower;
    }

    private static (int, int) findRootInterval(double number, int n)
    {
        const int hop = 10;
        int prev = 0;
        int curr = prev + hop;
        while (nthPower(curr, n) < number)
        {
            prev = curr;
            curr += hop;
        }
        return (prev, curr);
    }

    private static double nthPower(double number, int n)
    {
        double res = number;
        for (var i = 0; i < n - 1; i++)
        {
            res *= number;
        }
        return res;
    }

    private const double calculatorEqualDoublesDiff = 0.000001;

    private static bool areDoublesEqual(double first, double second)
    {
        double diff = first - second;
        if (diff < 0)
        {
            diff = -diff;
        }
        return diff < calculatorEqualDoublesDiff;
    }
}
