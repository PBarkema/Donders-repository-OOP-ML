using System;
using System.Collections.Generic;
using System.Linq;

namespace SatSolver
{
    public class Solver
    {
        /* 
         * This method returns the Valuation that fulfills the given Formula.
		 * And returns null when a Valuation does not exist.
		 * 
		 * This method calls a homonomously recursive method with three parameters and thef ollowing initial values:
		 * - formule       the given Formula,
		 * - variabelen    the Set of all variables from a formula, given by calling Verzamel,
		 * - valuatie      the empty valuation.
		 */
        public static Valuatie Vervulbaar(IFormule formule)
        {
            if (formule == null)
                return null;

            SortedSet<string> variabelen = new SortedSet<string>();
            formule.Verzamel(variabelen);

            Valuatie valuatie = new Valuatie();

            return Vervulbaar(formule, variabelen, valuatie);
        }

        /* 
         * This recursive method takes a Formula and a Set of remaining to-evaluate variables
         * and a Valuation for every subset of variables.
         * It returns a Valuation that satisfies a given formula, or else: null.
		 */
        private static Valuatie Vervulbaar(IFormule formule, SortedSet<string> variabelen, Valuatie valuatie)
        {
            // Valuation is not empty
            bool vervulbaar = false;

            if (variabelen.Count == 0)
            {
                if (vervulbaar)
                {
                    foreach (KeyValuePair<string,bool> sb in valuatie)
                    {
                        Console.WriteLine("Variable " + sb.Key + " has value " + sb.Value);
                    }
                    return valuatie;
                }
                else
                {
                    return null;
                }
            }

                string cur_var = GetElement(variabelen);
                string temp_var = "";

                if (formule.MogelijkWaar(valuatie))
                {
                Console.WriteLine(vervulbaar);
                vervulbaar = true;
                    valuatie.VoegToe(cur_var, true);
                    temp_var = cur_var;
                    if (formule.MogelijkWaar(valuatie))
                    {
                        variabelen.Remove(cur_var);
                    }
                Console.WriteLine(vervulbaar);
                return Vervulbaar(formule, variabelen, valuatie);
                Console.WriteLine(vervulbaar);
            }

                else {
                    vervulbaar = false;
                    if (valuatie.Counter() != 0)
                    {
                        valuatie.Remove(temp_var);
                    }
                    valuatie.VoegToe(cur_var, false);
                    variabelen.Remove(cur_var);
                    return Vervulbaar(formule, variabelen, valuatie);
                }
          
        }
        

        /* 
	* Return the first element of ISet.
	*/
        private static string GetElement(ISet<string> set)
        {
            foreach (string s in set)  // we preparere to go through all elements...
                return s;              // ...but only grab the first one!
            return null; // if empty.
        }
    }
}
