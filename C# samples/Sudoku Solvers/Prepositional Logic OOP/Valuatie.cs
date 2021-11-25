using System.Collections.Generic;

namespace SatSolver
{
    // This class contains the valuations for the given function.
    public class Valuatie: SortedDictionary<string,bool>
    {
        private readonly SortedDictionary<string, bool> dictionary;

        // Constructor.
        public Valuatie()
        {
            dictionary = new SortedDictionary<string, bool>();
        }

        // Does this valuation contain the given variable?
        public bool BevatVariabele(string variabele)
        {
            return dictionary.ContainsKey(variabele);
        }

        // Return the value of the given variable.
        public bool GeefWaarde(string variabele)
        {
            return dictionary[variabele];
        }

        // Adds the given variable with the given value to a valuation.
        public void VoegToe(string variabele, bool waarde)
        {
            dictionary.Add(variabele, waarde);
        }

        // Remove the given variable from the valuation.
        public void Verwijder(string variabele)
        {
            dictionary.Remove(variabele);
        }

        // Empty the valuation.
        public void Clear()
        {
            dictionary.Clear();
        }

        // Remove the last KeyValuePair from the list.
        public int Counter()
        {
            return dictionary.Count;
        }


        // Return a string representation of this Valuation.
        public override string ToString()
        {
            string resultaat = "";
            foreach (KeyValuePair<string, bool> pair in dictionary)
            {
                if (resultaat != "")
                    resultaat += " ";
                resultaat += pair.Key+ "=" + pair.Value;
            }
            return resultaat;
        }

        // Return an alternate string representation of the valuation.
        // The valuation should contain x{y}{x}{n} and is True when the Sudoku the digit n is entered in place (y,x).
        public string ToSudokuString()
        {
            string result = "";
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int n = 1; n <= 9; n++)
                    {
                        if (this.GeefWaarde("x" + y + x + n))
                            result += n;
                    }
                    result += " ";
                }
                result += "\n";
            }
            return result;
        }
    }
}
