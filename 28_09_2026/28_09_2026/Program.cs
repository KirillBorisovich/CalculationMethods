using System.Text;
using _28_09_2026;

Console.OutputEncoding = Encoding.UTF8;

while (true)
{
    Console.WriteLine("\nВЫЧИСЛИТЕЛЬНЫЙ ПРАКТИКУМ");
    Console.WriteLine("1 - решение нелинейного уравнения");
    Console.WriteLine("2 - задача о погружении шара");
    Console.WriteLine("0 - выход");
    Console.Write("Ваш выбор: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            NonlinearEquationTask.Run();
            break;
        case "2":
            BallImmersion.Run();
            break;
        case "0":
        case null:
            return;
        default:
            Console.WriteLine("Введите 0, 1 или 2.");
            break;
    }
}
