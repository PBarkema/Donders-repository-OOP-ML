using System;
using System.IO;
namespace SatSolver
{
    public class Program
    {
        /* 
         * The main file reads a formula line per line, and converts them to formula.
         * We look for a valuation that solves the formula.
         * If succesful, we print "SAT" on the first line of the output, and the second line contains the valuation.
         * If unsatisfiable, we print "UNSAT".
		 */
        public static void Main()
        {
            while (true)  
            {
                try
                {
                    String invoer = Console.ReadLine();
                    // String invoer = new StreamReader("..\\..\\testcases\\voorbeeld.txt").ReadToEnd();   // Read input from a file.
                    IFormule formule = Parser.ParseFormule(invoer); // Create a formula if possible.

                    DateTime start = DateTime.Now;
                    Valuatie valuatie = Solver.Vervulbaar(formule); // Solve and return valuation.
                    DateTime eind = DateTime.Now;
                    if (valuatie == null)
                        Console.WriteLine("UNSAT");
                    else Console.WriteLine("SAT\n" + valuatie);

                    // Console.WriteLine($"Solve time: {(eind - start).Ticks / 1E7} seconde");   // Time the process.

                }
                catch (Exception exc)
                {
                    Console.WriteLine("FOUT: "+ exc.Message);
                }

                //break;  // Enable to test multiple formulas.
            }
             Console.ReadLine();
        }
    }
}
