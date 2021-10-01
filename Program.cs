using System;

namespace Automatas
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (Lenguaje l = new Lenguaje("C:\\Archivos\\suma.cpp"))
                {
                    l.Programa();
                }
            }
            catch (Error e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }
}
