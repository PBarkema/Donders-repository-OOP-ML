using System;

namespace SatSolver
{
    /*
     * A Parser object can dissect a formula from preposition logic.
     * The input is a string with a text-representation of the formula, 
     * where the "and"-sign is "/\", the "or"-sign is "\/" and the "not"-sign is "-"
     * the preposition-variables exist of letters and numbers, such as "x", "hoi", "x23", "1", "45b1"
     * Possible formulas are:
     *      x
     *      (x/\-y)
     *      ((a1/\a2)\/a3)
     * For readability, spaces may be added to the string:
     *      ( (p\/q) /\ -p )
     * De signs can also be indicated by the C# notation C#-notatie ||, && and ! instead of \/, /\, and -:
     *      ( (p||q) && !p )
     * You can remove the parentheses. In that case: 
     *      x/\y/\z         is recognized as    (x /\ (y/\z))
     * Priorities: /\ before \/, as multipliciation as addition:
     *      x/\y\/a/\b      is recognized as   ( (x/\y) \/ (a/\b) )
     * Parentheses can control order of priority:
     *      x/\(y\/a)/\b    is recognized as    (x /\ ( (y\/a) /\ b))
     */


    class Parser
	{
		private string inhoud;
		private int    cursor;
        private int    lengte;

        /*
         * Converts a string to a prepositional formula.
         * Returns an error if the string contains syntax errors.
         */
        public static IFormule ParseFormule(string s)
        {
            Parser parser = new Parser(s);
            return parser.ParseFormule();
        }

        /*
	 * De constructor bewaart de string die ontleed moet worden, 
         * en initialiseert een "cursor" waarmee we kunnen aanwijzen tot hoe ver het ontleedproces gevorderd is.
	 */
        private Parser(string s)
		{
            inhoud = s;
            cursor = 0;
            lengte = s.Length;
		}
        
        /*
         * Deze hulpmethode zorgt ervoor dat de cursor eventuele extra spaties/tabs/newlines in de string passeert.
         */
        private void SkipSpaces()
        {
            while (cursor < lengte && char.IsWhiteSpace(inhoud[cursor]))
                cursor++;
        }
        
        /*
         * Deze methode start het recursieve ontleedproces op, 
         * en controleert na afloop of inderdaad de hele invoer is geconsumeerd.
         */
        private IFormule ParseFormule()
        {
            IFormule e = ParseExpressie();
            SkipSpaces();
            if (cursor < lengte)
                throw new Exception("Extra input op positie" + cursor + "(" + inhoud[cursor] +")");
            return e;
        }

        /* 
         * The real work is done by three recursive methods:
         * - ParseExpressie, dissectes complete formulas.
         * - ParseTerm, parses lose terms (without 'OR').
         * - ParseFactor, parses loose variables.
         * De methodes leveren de herkende deelformule op, 
         * en verplaatsen de cursor naar de positie in de invoer daar net voorbij.
         */
        private IFormule ParseFactor()
        {
            SkipSpaces();
            if (cursor<lengte && inhoud[cursor] == '(')
            {
                cursor++; // passeer het openingshaakje
                IFormule resultaat = ParseExpressie(); // tussen de haakjes mag een complete propositie staan
                SkipSpaces();
                if (inhoud[cursor] != ')') throw new Exception("sluithaakje ontbreekt op positie " + cursor);
                cursor++; // passeer het sluithaakje
                return resultaat;
            }
            else if (cursor < lengte && (inhoud[cursor] == '-' || inhoud[cursor] == '!' || inhoud[cursor]=='~'))
            {
                cursor++;
                IFormule resultaat = ParseFactor();
                IFormule Not_resultaat = MaakNegatie(resultaat);
                return Not_resultaat;
                // DONE: zorg dat de parser ook een negatie kan herkennen
             
            }
            else
            {                
                // geen haakje, geen not-teken, dus dan moeten we een variabele herkennen
                string naam = "";
                while (cursor < lengte && char.IsLetterOrDigit(inhoud[cursor]))
                {
                    naam = naam + inhoud[cursor];
                    cursor++;
                }
                IFormule Prop_resultaat = MaakPropositie(naam);
                return Prop_resultaat;

                // DONE: zorg dat de parser ook een variabele kan herkennen
            }
        }

        private IFormule ParseTerm()
        {
            IFormule f = ParseFactor();
            SkipSpaces();
            if (cursor<lengte-1 && (inhoud[cursor]=='/' && inhoud[cursor+1]=='\\' || inhoud[cursor] == '&' && inhoud[cursor + 1] == '&'))
            {
                cursor+=2; // passeer het voegteken
                IFormule t = ParseTerm();
                return MaakConjunctie(f, t);
            }
            return f;
        }

        private IFormule ParseExpressie()
        {
            IFormule t = ParseTerm();
            SkipSpaces();
            // Staat er een of-voegteken?
            if (cursor < lengte - 1 && (inhoud[cursor] == '\\' && inhoud[cursor + 1] == '/' || inhoud[cursor] == '|' && inhoud[cursor + 1] == '|'))
            {
                cursor +=2; // passeer het voegteken
                IFormule e = ParseExpressie();
                return MaakDisjunctie(t, e);
            }
            return t;
        }

        /*
         * Deze vier hulpmethoden maken een object aan voor de vier verschillende formule-vormen.
         */
		static IFormule MaakPropositie(string variabele)
		{
            Propositie propositie = new Propositie(variabele);
            return propositie;
		}

		static IFormule MaakNegatie(IFormule formule)
		{
            Negatie negatie = new Negatie(formule);
            return negatie;
        }

        static IFormule MaakConjunctie(IFormule links, IFormule rechts)
		{
            Conjunctie conjunctie = new Conjunctie(links, rechts);
            return conjunctie;
        }

        static IFormule MaakDisjunctie(IFormule links, IFormule rechts)
		{
            Disjunctie disjunctie = new Disjunctie(links, rechts);
            return disjunctie;
        }
    }
}
