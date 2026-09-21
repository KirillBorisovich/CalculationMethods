using System.Globalization;

namespace _28_09_2026;

public static class BallImmersion
{
    private const double Epsilon = 1e-12;
    private const int MaxIterations = 100;

    private static readonly (string Name, double Density)[] Materials =
    [
        ("Пробка", 0.25),
        ("Бамбук", 0.40),
        ("Сосна (белая)", 0.50),
        ("Кедр", 0.55),
        ("Дуб", 0.70),
        ("Бук", 0.75),
        ("Красное дерево", 0.80),
        ("Тиковое дерево", 0.85),
        ("Парафин", 0.90),
        ("Лёд/полиэтилен", 0.92),
        ("Пчелиный воск", 0.95)
    ];

    public static void Run()
    {
        Console.WriteLine("\nЗАДАЧА О ПОГРУЖЕНИИ ШАРА");
        Console.WriteLine("Глубина вычисляется методом Ньютона.");

        while (true)
        {
            var radius = ReadRadius();
            if (radius == 0)
            {
                return;
            }

            PrintTable(radius);
        }
    }

    private static void PrintTable(double radius)
    {
        Console.WriteLine($"\nРадиус шара: {radius:F4} м");
        Console.WriteLine(new string('-', 62));
        Console.WriteLine($"{"№",2}  {"Материал",-20} {"Плотность",10} {"Глубина d, м",18}");
        Console.WriteLine(new string('-', 62));

        for (var i = 0; i < Materials.Length; i++)
        {
            var material = Materials[i];
            var depth = CalculateDepth(radius, material.Density);
            Console.WriteLine(
                $"{i + 1,2}  {material.Name,-20} {material.Density,10:F2} {depth,18:F3}");
        }

        Console.WriteLine(new string('-', 62));
    }

    private static double CalculateDepth(double radius, double density)
    {
        var calculation = NumericalMethods.SolveNewton(
            t => t * t * t - 3 * t * t + 4 * density,
            t => 3 * t * t - 6 * t,
            initialApproximation: 1.0,
            epsilon: Epsilon,
            maxIterations: MaxIterations);

        if (!calculation.Success)
        {
            throw new InvalidOperationException(
                $"Метод Ньютона не сошёлся: {calculation.Error}.");
        }

        return calculation.Root * radius;
    }

    private static double ReadRadius()
    {
        while (true)
        {
            Console.Write("\nВведите радиус шара в метрах [0.62], 0 - назад: ");
            var input = Console.ReadLine();

            if (input is null)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                return 0.62;
            }

            var normalized = input.Trim().Replace(',', '.');
            if (double.TryParse(
                    normalized,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var radius) &&
                double.IsFinite(radius) &&
                radius >= 0)
            {
                return radius;
            }

            Console.WriteLine("Введите неотрицательное число.");
        }
    }
}
