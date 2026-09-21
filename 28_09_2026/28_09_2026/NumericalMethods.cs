namespace _28_09_2026;

public static class NumericalMethods
{
    public const int DefaultMaxIterations = 100_000;
    public const double DefaultZeroTolerance = 1e-20;

    public static List<RootInterval> SeparateRoots(
        Func<double, double> function,
        double a,
        double b,
        int parts,
        double zeroTolerance = DefaultZeroTolerance)
    {
        var intervals = new List<RootInterval>();
        var step = (b - a) / parts;

        for (var i = 0; i < parts; i++)
        {
            var left = a + i * step;
            var right = i == parts - 1 ? b : a + (i + 1) * step;
            var fLeft = function(left);
            var fRight = function(right);

            if (!double.IsFinite(fLeft) || !double.IsFinite(fRight))
            {
                continue;
            }

            if (Math.Abs(fLeft) <= zeroTolerance)
            {
                AddPointRoot(intervals, left, zeroTolerance);
            }
            else if (fLeft * fRight < 0)
            {
                intervals.Add(new RootInterval(left, right));
            }

            if (i == parts - 1 && Math.Abs(fRight) <= zeroTolerance)
            {
                AddPointRoot(intervals, right, zeroTolerance);
            }
        }

        return intervals;
    }

    public static double ChooseNewtonInitial(
        Func<double, double> function,
        Func<double, double> secondDerivative,
        RootInterval interval)
    {
        if (function(interval.Left) * secondDerivative(interval.Left) > 0)
        {
            return interval.Left;
        }

        if (function(interval.Right) * secondDerivative(interval.Right) > 0)
        {
            return interval.Right;
        }

        return (interval.Left + interval.Right) / 2;
    }

    public static CalculationResult SolveBisection(
        Func<double, double> function,
        RootInterval interval,
        double epsilon,
        int maxIterations = DefaultMaxIterations,
        double zeroTolerance = DefaultZeroTolerance)
    {
        if (TryGetKnownRoot(function, interval, zeroTolerance, out var knownRoot))
        {
            return knownRoot;
        }

        var left = interval.Left;
        var right = interval.Right;
        var fLeft = function(left);

        if (fLeft * function(right) > 0)
        {
            return CalculationResult.Failed("на концах нет перемены знака");
        }

        var steps = 0;
        while (right - left > 2 * epsilon && steps < maxIterations)
        {
            var middle = (left + right) / 2;
            var fMiddle = function(middle);

            if (Math.Abs(fMiddle) <= zeroTolerance)
            {
                left = middle;
                right = middle;
                steps++;
                break;
            }

            if (fLeft * fMiddle <= 0)
            {
                right = middle;
            }
            else
            {
                left = middle;
                fLeft = fMiddle;
            }

            steps++;
        }

        if (steps >= maxIterations)
        {
            return CalculationResult.Failed("превышен предел итераций");
        }

        var root = (left + right) / 2;
        return CalculationResult.Completed(
            steps,
            root,
            (right - left) / 2,
            Math.Abs(function(root)));
    }

    public static CalculationResult SolveNewton(
        Func<double, double> function,
        Func<double, double> derivative,
        double initialApproximation,
        double epsilon,
        int maxIterations = DefaultMaxIterations,
        double zeroTolerance = DefaultZeroTolerance)
    {
        var x = initialApproximation;
        var initialValue = function(x);

        if (!double.IsFinite(initialValue))
        {
            return CalculationResult.Failed("функция вернула нечисловое значение");
        }

        if (Math.Abs(initialValue) <= zeroTolerance)
        {
            return CalculationResult.Completed(0, x, 0, Math.Abs(initialValue));
        }

        for (var step = 1; step <= maxIterations; step++)
        {
            var derivativeValue = derivative(x);
            if (!double.IsFinite(derivativeValue) ||
                Math.Abs(derivativeValue) <= zeroTolerance)
            {
                return CalculationResult.Failed("производная близка к нулю");
            }

            var next = x - function(x) / derivativeValue;
            if (!double.IsFinite(next))
            {
                return CalculationResult.Failed("получено нечисловое значение");
            }

            var delta = Math.Abs(next - x);
            if (delta <= epsilon)
            {
                return CalculationResult.Completed(
                    step,
                    next,
                    delta,
                    Math.Abs(function(next)));
            }

            x = next;
        }

        return CalculationResult.Failed("превышен предел итераций");
    }

    public static CalculationResult SolveModifiedNewton(
        Func<double, double> function,
        Func<double, double> derivative,
        double initialApproximation,
        double epsilon,
        int maxIterations = DefaultMaxIterations,
        double zeroTolerance = DefaultZeroTolerance)
    {
        var x = initialApproximation;
        var initialValue = function(x);

        if (!double.IsFinite(initialValue))
        {
            return CalculationResult.Failed("функция вернула нечисловое значение");
        }

        if (Math.Abs(initialValue) <= zeroTolerance)
        {
            return CalculationResult.Completed(0, x, 0, Math.Abs(initialValue));
        }

        var fixedDerivative = derivative(x);
        if (!double.IsFinite(fixedDerivative) ||
            Math.Abs(fixedDerivative) <= zeroTolerance)
        {
            return CalculationResult.Failed("производная в x0 близка к нулю");
        }

        for (var step = 1; step <= maxIterations; step++)
        {
            var next = x - function(x) / fixedDerivative;
            if (!double.IsFinite(next))
            {
                return CalculationResult.Failed("получено нечисловое значение");
            }

            var delta = Math.Abs(next - x);
            if (delta <= epsilon)
            {
                return CalculationResult.Completed(
                    step,
                    next,
                    delta,
                    Math.Abs(function(next)));
            }

            x = next;
        }

        return CalculationResult.Failed("превышен предел итераций");
    }

    public static CalculationResult SolveSecant(
        Func<double, double> function,
        RootInterval interval,
        double epsilon,
        int maxIterations = DefaultMaxIterations,
        double zeroTolerance = DefaultZeroTolerance)
    {
        if (TryGetKnownRoot(function, interval, zeroTolerance, out var knownRoot))
        {
            return knownRoot;
        }

        var previous = interval.Left;
        var current = interval.Right;
        var fPrevious = function(previous);
        var fCurrent = function(current);

        for (var step = 1; step <= maxIterations; step++)
        {
            var denominator = fCurrent - fPrevious;
            if (!double.IsFinite(denominator) ||
                Math.Abs(denominator) <= zeroTolerance)
            {
                return CalculationResult.Failed("знаменатель близок к нулю");
            }

            var next = current - fCurrent * (current - previous) / denominator;
            if (!double.IsFinite(next))
            {
                return CalculationResult.Failed("получено нечисловое значение");
            }

            var delta = Math.Abs(next - current);
            if (delta <= epsilon)
            {
                return CalculationResult.Completed(
                    step,
                    next,
                    delta,
                    Math.Abs(function(next)));
            }

            previous = current;
            fPrevious = fCurrent;
            current = next;
            fCurrent = function(current);
        }

        return CalculationResult.Failed("превышен предел итераций");
    }

    private static void AddPointRoot(
        List<RootInterval> intervals,
        double x,
        double zeroTolerance)
    {
        if (intervals.Count == 0 ||
            Math.Abs(intervals[^1].Left - x) > zeroTolerance)
        {
            intervals.Add(new RootInterval(x, x));
        }
    }

    private static bool TryGetKnownRoot(
        Func<double, double> function,
        RootInterval interval,
        double zeroTolerance,
        out CalculationResult result)
    {
        if (Math.Abs(interval.Right - interval.Left) <= zeroTolerance)
        {
            var root = interval.Left;
            result = CalculationResult.Completed(
                0,
                root,
                0,
                Math.Abs(function(root)));
            return true;
        }

        var leftValue = function(interval.Left);
        if (Math.Abs(leftValue) <= zeroTolerance)
        {
            result = CalculationResult.Completed(
                0,
                interval.Left,
                0,
                Math.Abs(leftValue));
            return true;
        }

        var rightValue = function(interval.Right);
        if (Math.Abs(rightValue) <= zeroTolerance)
        {
            result = CalculationResult.Completed(
                0,
                interval.Right,
                0,
                Math.Abs(rightValue));
            return true;
        }

        result = default;
        return false;
    }
}

public readonly record struct RootInterval(double Left, double Right);

public readonly record struct CalculationResult(
    bool Success,
    int Steps,
    double Root,
    double Delta,
    double Residual,
    string Error)
{
    public static CalculationResult Completed(
        int steps,
        double root,
        double delta,
        double residual) =>
        new(true, steps, root, delta, residual, string.Empty);

    public static CalculationResult Failed(string error) =>
        new(false, 0, double.NaN, double.NaN, double.NaN, error);
}
