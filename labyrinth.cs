using System;
using System.Collections.Generic;

namespace labyrinth.cs
{
    internal class Program
    {
        static void Main(string[] args)
        {
		s
        }
    }
    public class Film
    {
        public Film(string nazev, string jmeno, string prijmeni, int rok)
        {
            Nazev = nazev;
            JmenoRezisera = jmeno;
            PrijmeniRezisera = prijmeni;
            RokVzniku = rok;
        }
        public string Nazev { get; }
        public string JmenoRezisera { get; }
        public string PrijmeniRezisera { get; }
        public int RokVzniku { get; }


        public double Hodnoceni { get; private set; }

        List<double> VsechnaHodnoceni = new List<double>();

        public void PridaniHodnoceni(double vlastniHodnoceni)
        {
            VsechnaHodnoceni.Add(vlastniHodnoceni);
            foreach (var h in VsechnaHodnoceni) {
							Hodnoceni += h;
						}
            Hodnoceni = Hodnoceni / VsechnaHodnoceni.Count;
        }
        public void VypisHodnoceni()
        {
            foreach (double hodnoc in VsechnaHodnoceni) {
							Console.WriteLine(hodnoc);
						}
        }
        public override string ToString()
        {
            return $"{{{Nazev}}} ({{{RokVzniku}}}; {{{PrijmeniRezisera}}}, {{{JmenoRezisera[0]}}}): {{{Hodnoceni}}}⭐";

        }
    }
}
