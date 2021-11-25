using System.Collections.Generic;

namespace SatSolver
{
    // 
    /*
     *  This interface describes what a logical formula should be able to process.
     */
    public interface IFormule
    {
        void Verzamel(ISet<string> set);
        string ToString();
        bool MogelijkWaar(Valuatie v); 
        bool MogelijkOnwaar(Valuatie v);
        bool Waarde(Valuatie v);
    }

    public class Conjunctie: IFormule
    {
        IFormule links, rechts;
        
        public Conjunctie(IFormule l, IFormule r)
        {
            this.links = l;
            this.rechts = r;
        }

        public void Verzamel(ISet<string> set)
        {
            links.Verzamel(set);
            rechts.Verzamel(set);
        }

        public override string ToString()
        {
            string str_links, str_rechts;
            str_links = links.ToString();
            str_rechts = rechts.ToString();
            return "(" + str_links + "/\\" + str_rechts + ")"; 
        }

        public bool Waarde(Valuatie v)
        {
            return links.Waarde(v) && rechts.Waarde(v);
        }

        public bool MogelijkWaar(Valuatie v)
        {
            if (links.MogelijkOnwaar(v) && rechts.MogelijkOnwaar(v))
            {
                return true;
            }
            else return false;
        }

        public bool MogelijkOnwaar(Valuatie v)
        {
            if (!(links.MogelijkOnwaar(v) && rechts.MogelijkOnwaar(v)))
            {
                return true;
            }
            else return false;
        }
    }

    public class Disjunctie : IFormule
    {
        IFormule links, rechts;

        public Disjunctie(IFormule l, IFormule r)
        {
            this.links = l;
            this.rechts = r;
        }

        public void Verzamel(ISet<string> set)
        {
            links.Verzamel(set);
            rechts.Verzamel(set);
        }

        public override string ToString()
        {
            string str_links, str_rechts;
            str_links = links.ToString();
            str_rechts = rechts.ToString();
            return "(" + str_links + "\\/" + str_rechts + ")";
        }

        public bool Waarde(Valuatie v)
        {
            return links.Waarde(v) || rechts.Waarde(v);
        }

        public bool MogelijkWaar(Valuatie v)
        {
            if (links.MogelijkWaar(v) || rechts.MogelijkWaar(v))
            {
                return true;
            }
            else return false;
        }


        public bool MogelijkOnwaar(Valuatie v)
        {
            if (!(links.MogelijkOnwaar(v) || rechts.MogelijkOnwaar(v)))
            {
                return true;
            }
            else return false;
        }
    }

    public class Negatie : IFormule
    {
        IFormule onder;
        

        public Negatie(IFormule o)
        {
            this.onder = o;
        }

        public void Verzamel(ISet<string> set)
        {
            onder.Verzamel(set);
        }

        public override string ToString()
        {
            string str_negatie;
            str_negatie = onder.ToString();
            return "-" + str_negatie;
        }

        public bool Waarde(Valuatie v)
        {
            return !(v.GeefWaarde(onder.ToString()));
        }

        public bool MogelijkWaar(Valuatie v)
        {
            // Parse onder als propositie en maak deze onwaar om de negatie te vervullen
            if (onder.MogelijkOnwaar(v))
            {
                return true;
            }
            else return false;
        }

        public bool MogelijkOnwaar(Valuatie v)
        {
            if (onder.MogelijkWaar(v))
            {
                return true;
            }
            else return false;
        }

    }

    public class Propositie : IFormule
    {
        string prop_naam;

        public Propositie(string p)
        {
            this.prop_naam = p;
        }

        public void Verzamel(ISet<string> set)
        {
            set.Add(prop_naam);
        }

        public override string ToString()
        {
            return prop_naam;

        }

        public bool Waarde(Valuatie v)
        {
            return v.GeefWaarde(prop_naam);
        }

        public bool MogelijkWaar(Valuatie v)
        {
            if (v.BevatVariabele(prop_naam))
                {
                return v.GeefWaarde(prop_naam);
            }
            else return true; // voordeel van de twijfel
        }

        public bool MogelijkOnwaar(Valuatie v)
        {
            if (!(v.BevatVariabele(prop_naam)))
            {
                return true;
            }
            else return v.GeefWaarde(prop_naam); 
        }
    
    }
}