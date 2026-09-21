using System.Globalization;

namespace _28_09_2026;

public static class NonlinearEquationTask
{
    private const double DefaultA = -5.0;
    private const double DefaultB = 10.0;
    private const double DefaultEpsilon = 1e-6;
    private const int DefaultN = 1000;
    private static readonly double Ln2 = Math.Log(2.0);

    public static void Run()
    {
        Console.WriteLine("ЧИСЛЕННЫЕ МЕТОДЫ РЕШЕНИЯ НЕЛИНЕЙНЫХ УРАВНЕНИЙ");
        Console.WriteLine("Тестовая задача № 2");
        Console.WriteLine("f(x) = 2^(-x) - sin(x)");

        while (true)
        {
            var a = ReadDouble("Введите A", DefaultA);
            var b = ReadDouble("Введите B", DefaultB);

            if (a >= b)
            {
                Console.WriteLine("Ошибка: должно выполняться A < B.\n");
                continue;
            }

            var changeBounds = false;
            var n = DefaultN;

            while (!changeBounds)
            {
                n = ReadInt("Введите N - число разбиений", 2, 10_000_000, n);
                var intervals = NumericalMethods.SeparateRoots(
                    Function,
                    a,
                    b,
                    n);
                PrintSeparatedRoots(a, b, n, intervals);

                while (true)
                {
                    Console.WriteLine();
                    if (intervals.Count > 0)
                    {
                        Console.WriteLine("1 - уточнить корень на выбранном отрезке");
                    }

                    Console.WriteLine("2 - ввести новое N и повторить отделение корней");
                    Console.WriteLine("3 - изменить A и B");
                    Console.WriteLine("0 - вернуться в главное меню");

                    var action = ReadInt("Ваш выбор", 0, 3);

                    if (action == 0)
                    {
                        return;
                    }

                    if (action == 2)
                    {
                        break;
                    }

                    if (action == 3)
                    {
                        changeBounds = true;
                        break;
                    }

                    if (action != 1 || intervals.Count == 0)
                    {
                        Console.WriteLine("Этот пункт сейчас недоступен.");
                        continue;
                    }

                    var intervalNumber = ReadInt(
                        "Введите номер отрезка",
                        1,
                        intervals.Count);
                    var epsilon = ReadPositiveDouble("Введите точность ε", DefaultEpsilon);
                    PrintRefinementResults(intervals[intervalNumber - 1], epsilon);
                }
            }
        }
    }

    private static void PrintSeparatedRoots(
        double a,
        double b,
        int n,
        IReadOnlyList<RootInterval> intervals)
    {
        var h = (b - a) / n;

        Console.WriteLine();
        Console.WriteLine($"A = {FormatNumber(a)}, B = {FormatNumber(b)}");
        Console.WriteLine($"N = {n}, h = {h:E6}");
        Console.WriteLine($"Найдено отрезков (или точек) локализации: {intervals.Count}");

        for (var i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            Console.WriteLine(
                $"{i + 1,3}) [{FormatNumber(interval.Left),20}; {FormatNumber(interval.Right),20}]");
        }
    }

    private static void PrintRefinementResults(RootInterval interval, double epsilon)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Уточнение корня на {FormatInterval(interval)}, ε = {epsilon:E3}");

        var newtonInitial = NumericalMethods.ChooseNewtonInitial(
            Function,
            SecondDerivative,
            interval);

        var results = new (string Name, string InitialApproximation, CalculationResult Result)[]
        {
            (
                "Метод бисекции",
                FormatInterval(interval),
                NumericalMethods.SolveBisection(
                    Function,
                    interval,
                    epsilon)),
            (
                "Метод Ньютона",
                $"x0={FormatNumber(newtonInitial)}",
                NumericalMethods.SolveNewton(
                    Function,
                    FirstDerivative,
                    newtonInitial,
                    epsilon)),
            (
                "Модифиц. метод Ньютона",
                $"x0={FormatNumber(newtonInitial)}",
                NumericalMethods.SolveModifiedNewton(
                    Function,
                    FirstDerivative,
                    newtonInitial,
                    epsilon)),
            (
                "Метод секущих",
                $"x0={FormatNumber(interval.Left)}; x1={FormatNumber(interval.Right)}",
                NumericalMethods.SolveSecant(
                    Function,
                    interval,
                    epsilon))
        };

        Console.WriteLine(new string('-', 129));
        Console.WriteLine(
            $"{"Метод",-29} {"Начальное приближение",-37} {"Шаги",7} {"Приближенный корень",22} {"Δ/длина",14} {"|f(x)|",14}");
        Console.WriteLine(new string('-', 129));

        foreach (var method in results)
        {
            var result = method.Result;
            if (!result.Success)
            {
                Console.WriteLine(
                    $"{method.Name,-29} {method.InitialApproximation,-37} " +
                    $"{"ошибка:",-7} {result.Error}");
                continue;
            }

            Console.WriteLine(
                $"{method.Name,-29} {method.InitialApproximation,-37} {result.Steps,7} " +
                $"{result.Root,22:F16} {result.Delta,14:E3} {result.Residual,14:E3}");
        }

        Console.WriteLine(new string('-', 129));
        Console.WriteLine("Для бисекции в столбце Δ/длина указана длина последнего отрезка.");
    }

    private static double Function(double x) => PowerOfTwoMinusX(x) - Math.Sin(x);

    private static double FirstDerivative(double x) =>
        -Ln2 * PowerOfTwoMinusX(x) - Math.Cos(x);

    private static double SecondDerivative(double x) =>
        Ln2 * Ln2 * PowerOfTwoMinusX(x) + Math.Sin(x);

    private static double PowerOfTwoMinusX(double x) => Math.Pow(2.0, -x);

    private static string FormatInterval(RootInterval interval) =>
        $"[{FormatNumber(interval.Left)}; {FormatNumber(interval.Right)}]";

    private static string FormatNumber(double value) =>
        value.ToString("G15", CultureInfo.InvariantCulture);

    private static double ReadDouble(string prompt, double defaultValue)
    {
        while (true)
        {
            Console.Write($"{prompt} [{FormatNumber(defaultValue)}]: ");
            var input = Console.ReadLine();

            if (input is null || string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (TryParseDouble(input, out var value) && double.IsFinite(value))
            {
                return value;
            }

            Console.WriteLine("Введите корректное вещественное число.");
        }
    }

    private static double ReadPositiveDouble(string prompt, double defaultValue)
    {
        while (true)
        {
            var value = ReadDouble(prompt, defaultValue);
            if (value > 0)
            {
                return value;
            }

            Console.WriteLine("Значение должно быть больше нуля.");
        }
    }

    private static int ReadInt(string prompt, int minimum, int maximum, int? defaultValue = null)
    {
        while (true)
        {
            var suffix = defaultValue.HasValue ? $" [{defaultValue.Value}]" : string.Empty;
            Console.Write($"{prompt}{suffix}: ");
            var input = Console.ReadLine();

            if ((input is null || string.IsNullOrWhiteSpace(input)) && defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            if (input is null)
            {
                return minimum;
            }

            if (int.TryParse(input.Trim(), out var value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }

    private static bool TryParseDouble(string input, out double value)
    {
        var normalized = input.Trim().Replace(',', '.');
        return double.TryParse(
            normalized,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value);
    }

}
