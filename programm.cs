using System;

class Program
{
    static void Main()
    {
        Console.Write("Введите значение F (беззнаковое целое число): ");
        string input = Console.ReadLine();

        uint F;
        while (!uint.TryParse(input, out F))
        {
            Console.WriteLine("Ошибка: введите только цифры (беззнаковое целое число)!");
            Console.Write("Попробуйте снова: ");
            input = Console.ReadLine();
        }

        uint plantCode = (F >> 8) & 0xFF;
        byte attributes = (byte)(F & 0xFF);

        bool hasFlowers = (attributes & (1 << 6)) != 0;
        bool isAnnual = (attributes & (1 << 2)) != 0;
        bool needsSpecialCare = (attributes & (1 << 1)) != 0;
        bool reproducesByShoots = (attributes & (1 << 3)) != 0;

        bool isPerennialFloweringWithSpecialCare = hasFlowers && !isAnnual && needsSpecialCare;
        bool isAnnualReproducingByShoots = isAnnual && reproducesByShoots;

        bool result = isPerennialFloweringWithSpecialCare || isAnnualReproducingByShoots;

        Console.WriteLine($"Код растения: {plantCode}");
        Console.WriteLine($"Атрибуты растения (биты 0-7): {Convert.ToString(attributes, 2).PadLeft(8, '0')}");
        Console.WriteLine("\nДетализация условий:");
        Console.WriteLine($"1. Многолетнее цветущее растение требует специального ухода: {isPerennialFloweringWithSpecialCare}");
        Console.WriteLine($"2. Однолетнее растение размножается побегами: {isAnnualReproducingByShoots}");
        Console.WriteLine($"\nИтоговый результат: {result}");
    }
}
