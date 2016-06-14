using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception exc)
            {
                if (Console.CursorLeft > 0)
                    Console.WriteLine();

                Console.WriteLine(ViewModelCommon.ConvertExceptionToString(exc));
            }
            Console.WriteLine();
            Console.WriteLine("Stiskněte Enter pro ukončení aplikace.");
            Console.ReadLine();
        }
    }
}
