// SPDX-License-Identifier: GPL-2.0
// Compute thermodynamic properties of individual species and composition of species
// 
// Original C: Copyright (C) 2000
//    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>
//    Mark Pinese  <pinese@cyberwizards.com.au>
//
// C# Port: Copyright (C) 2022
//    Ben Voß
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

namespace ImpulseRocketry.LibPropellantEval;

public partial class Evaluator {

    private static readonly string[] header = new string[] {
        "CHAMBER",
        "THROAT",
        "EXIT" 
    };

    public void PrintCondensed(Product p) {
        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
            Console.Write($"{ThermoList[p.Species[Constants.CONDENSED][i]].Name} ");
        }

        Console.WriteLine();
    }

    public void PrintGazeous(Product p) {
        for (var i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            Console.Write($"{ThermoList[p.Species[Constants.GAS][i]].Name} ");
        }

        Console.WriteLine();
    }

    public void PrintProductComposition(params Equilibrium[] e) {
        var npt = e.Length;

        int i, j, k;

        double mol_g = e[0].IterationInfo.N;

        // we have to build a list of all condensed species present
        // in the three equilibrium
        var n = 0;
        var condensed_list = new int[Constants.MAX_PRODUCT];

        // ok become false if the species already exist in the list
        var ok = true;

        double qt;

        for (i = 0; i < e[0].Product.NumSpecies[Constants.CONDENSED]; i++) {
            mol_g += e[0].Product.Coef[Constants.CONDENSED][i];
        }

        Console.WriteLine();
        Console.WriteLine("Molar fractions");
        Console.WriteLine();

        for (i = 0; i < e[0].Product.NumSpecies[Constants.GAS]; i++) {
            if (e[0].Product.Coef[Constants.GAS][i] / e[0].IterationInfo.N > 0.0) {
                Console.Write($"{ThermoList[e[0].Product.Species[Constants.GAS][i]].Name,-20}");

                for (j = 0; j < npt; j++) {
                    Console.Write($" {e[j].Product.Coef[Constants.GAS][i] / mol_g,11:0.0000e+00}");
                }

                Console.WriteLine();
            }
        }

        // Build the list of condensed
        for (i = 0; i < npt; i++) {
            for (j = 0; j < e[i].Product.NumSpecies[Constants.CONDENSED]; j++) {
                for (k = 0; k < n; k++) {
                    // Check if the condensed are to be include in the list
                    if (condensed_list[k] == e[i].Product.Species[Constants.CONDENSED][j]) {
                        // We do not have to include the species
                        ok = false;
                        break;
                    }
                } // k

                if (ok) {
                    condensed_list[n] = e[i].Product.Species[Constants.CONDENSED][j];
                    n++;
                }

                // reset the flag
                ok = true;
            } // j
        } // i

        if (n > 0) {
            Console.WriteLine("Condensed species");
            for (i = 0; i < n; i++) {
                Console.Write($"{ThermoList[condensed_list[i]].Name,-20}s");

                for (j = 0; j < npt; j++) {
                    // search in the product of each equilibrium if the
                    // condensed is present
                    qt = 0.0;

                    for (k = 0; k < e[j].Product.NumSpecies[Constants.CONDENSED]; k++) {
                        if (condensed_list[i] == e[j].Product.Species[Constants.CONDENSED][k]) {
                            qt = e[j].Product.Coef[Constants.CONDENSED][k];
                            break;
                        }
                    }

                    Console.Write($" {qt / mol_g:11.4e}");
                }

                Console.WriteLine();
            }
        }

        Console.WriteLine();
    }

    public int PrintPropellantComposition(Equilibrium e) {
        int i, j;

        Console.WriteLine("Propellant composition");
        Console.WriteLine($"Code  {"Name",-35} mol    Mass (g)  Composition");
        for (i = 0; i < e.Propellant.NumComponents; i++) {
            Console.Write($"{e.Propellant.Molecule[i],-4}  {PropellantList[e.Propellant.Molecule[i]].Name,-35} {e.Propellant.Coef[i]:0.0000} {e.Propellant.Coef[i] * PropellantList.MolarMass(e.Propellant.Molecule[i]):0.0000} ");

            Console.Write("  ");
            // Print the composition
            for (j = 0; j < 6; j++) {
                if (PropellantList[e.Propellant.Molecule[i]].Coef[j] != 0) {
                    Console.Write($"{PropellantList[e.Propellant.Molecule[i]].Coef[j]}{Constants.Symb[PropellantList[e.Propellant.Molecule[i]].Elem[j]]} ");
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Density : {e.Propellant.Density: 0.000;-0.000} g/cm^3");

        if (e.Product.ElementListed) {
            Console.WriteLine($"{e.Product.NumElements} different elements");

            // Print those elements
            for (i = 0; i < e.Product.NumElements; i++) {
                Console.Write($"{Constants.Symb[e.Product.Elements[i]]} ");
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Total mass:  {PropellantMass(e):0.000000} g");

        Console.WriteLine($"Enthalpy  : {PropellantEnthalpy(e):0.00} kJ/kg");

        Console.WriteLine();

        if (e.Product.ProductListed) {
            Console.WriteLine($"{e.Product.NumSpecies[Constants.GAS]} possible gazeous species");

            if (_verbose > 1) {
                PrintGazeous(e.Product);
            }

            Console.WriteLine($"{e.Product.NumCondensed} possible condensed species");
            Console.WriteLine();

            if (_verbose > 1) {
                PrintCondensed(e.Product);
            }
        }

        return 0;
    }

    public static void PrintPerformanceInformation(Equilibrium[] e, short npt) {
        short i;

        Console.Write("Ae/At            :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.AeAt,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("A/dotm (m/s/atm) :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.ADotm,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("C* (m/s)         :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.Cstar,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("Cf               :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.Cf,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("Ivac (m/s)       :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.Ivac,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("Isp (m/s)        :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.Isp,11:0.00000}");
        }
        Console.WriteLine();

        Console.Write("Isp/g (s)        :            ");
        for (i = 1; i < npt; i++) {
            Console.Write($" {e[i].Performance.Isp / Constants.Ge,11:0.00000}");
        }
        Console.WriteLine();
    }

    public static void PrintProductProperties(params Equilibrium[] e) {
        var npt = e.Length;

        Console.Write($"                  ");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {header[i],11}");
        }
        Console.WriteLine();

        Console.Write($"Pressure (atm)   :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.P,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"Temperature (K)  :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.T,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"H (kJ/kg)        :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.H,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"U (kJ/kg)        :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.U,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"G (kJ/kg)        :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.G,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"S (kJ/(kg)(K)    :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.S,11:0.000}");
        }
        Console.WriteLine();
        Console.Write($"M (g/mol)        :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.M,11:0.000}");
        }
        Console.WriteLine();

        Console.Write($"(dLnV/dLnP)t     :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.dV_P,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"(dLnV/dLnT)p     :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.dV_T,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"Cp (kJ/(kg)(K))  :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.Cp,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"Cv (kJ/(kg)(K))  :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.Cv,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"Cp/Cv            :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.Cp / e[i].Properties.Cv,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"Gamma            :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.Isex,11:0.00000}");
        }
        Console.WriteLine();
        Console.Write($"Vson (m/s)       :");
        for (var i = 0; i < npt; i++) {
            Console.Write($" {e[i].Properties.Vson,11:0.00000}");
        }
        Console.WriteLine();
        Console.WriteLine();
    }
}