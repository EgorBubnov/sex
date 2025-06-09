using System;

class Program
{
    static void Main()
    {
        Console.Write("Введите значение F (беззнаковое целое число): ");
        uint F = Convert.ToUInt32(Console.ReadLine());





        uint plantсode = (F >> 8) & 0xFF;

        
        byte attributes = (byte)(F & 0xFF);

        bool hasflowers = (attributes & (1 << 6)) != 0;       
        bool isannual = (attributes & (1 << 2)) != 0;         
        bool needsSpecialCare = (attributes & (1 << 1)) != 0;  
        bool reproducesByShoots = (attributes & (1 << 3)) != 0; 

     
        bool isPerennialFloweringWithSpecialCare = hasflowers && !isannual && needsSpecialCare;
        bool isAnnualReproducingByShoots = isannual && reproducesByShoots;

        bool result = isPerennialFloweringWithSpecialCare || isAnnualReproducingByShoots;

     
        Console.WriteLine($"Код растения: {plantсode}");
        Console.WriteLine($"Атрибуты растения (биты 0-7): {Convert.ToString(attributes, 2).PadLeft(8, '0')}");
        Console.WriteLine("\nДетализация условий:");
        Console.WriteLine($"1. Многолетнее цветущее растение требует специального ухода: {isPerennialFloweringWithSpecialCare}");
        Console.WriteLine($"2. Однолетнее растение размножается побегами: {isAnnualReproducingByShoots}");
        Console.WriteLine($"\nИтоговый результат: {result}");
    }
}
