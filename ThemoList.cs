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

/// <summary>
/// Structure to hold information of species contain in the thermo data file
/// </summary>
public class ThermoListItem {
    public string? Name;
    public string? Comments;
    public int NumIntervals;            // Number of different temperature interval
    public string? Id;                  // Identification code
    public int[] Elem = new int[5];
    public int[] Coef = new int[5];
    public int State;
    public double Weight;               // Molecular weight
    public float Heat;                  // Heat of formation at 298.15 K  (J/mol)
    public double Dho;                  // HO(298.15) - HO(0)
    public float[][] Range = Utils.Make2DArray<float>(5, 2);    // Temperature range
    public int[] NumCoef = new int[5];    // Number of coefficient for Cp0/R
    public int[][] Ex = Utils.Make2DArray<int>(5, 8);           // Exponent in empirical equation

    public double[][] Param = Utils.Make2DArray<double>(5, 9);

    // For species with data at only one temperature especially condensed
    public float Temp;
    public float Enth;
}

public class ThermoList {

    private readonly List<ThermoListItem> _items = new();

    public ThermoList(int verbose) {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using var resourceStream = assembly.GetManifestResourceStream("ImpulseRocketry.LibPropellantEval.data.thermo.dat");
        Load(resourceStream, verbose);
    }

    public ThermoList(string fileName, int verbose) {
        using var fileStream = File.OpenRead(fileName);
        Load(fileStream, verbose);
    }
        
    /***************************************************************************
    Initial format of thermo.dat:
    interval   variable   type	size	description
    -----------------------------------------------------------------------------
    (0, 18)    name	      string	18	compound name
    (18, 73)   comments   string	55	comment
    (73, 75)   nint	      int	2	the number of temperature intervals
    (75, 81)   id	      string	6	the material id
    81	   state      int	1	0 - GAS, else CONDENSED
    (82, 95)   weight     float	13	molecular weight
    (95, 108)  enth/heat  float	13	enthaply if nint == 0 
                                            else heat of formation
                ...
                rest of file
                ...
    ***************************************************************************/
    private void Load(Stream stream, int verbose) { 
        using var reader = new StreamReader(stream);

        if (verbose != 0) {
            Console.Write("Scanning thermo data file...");
        }

        var numThermo = 0;

        // Scan thermo.dat to find the number of positions in thermo_list to allocate
        var line = reader.ReadLine();
        while (line is not null) {
            // All that is required is to count the number of lines not
            // starting with ' ', '!' or '-'
            if (line[0] != ' ' && line[0] != '!' && line[0] != '-') {
                numThermo++;
            }

            line = reader.ReadLine();
        }

        // Reset the file pointer
        stream.Position = 0;

        if (verbose != 0) {
            Console.Write($"\nScan complete.  {numThermo} records found.  Allocating memory...");
        }

        _items.EnsureCapacity(numThermo);

        if (verbose != 0) {
            Console.Write("\nSuccessful.  Loading thermo data file...");
        }

        for (var i = 0; i < numThermo; i++) {
            // Read in the next line and check for EOF
            line = reader.ReadLine();
            if (line is null) {
                throw new IOException("Unexpected end of file");
            }

            // Skip commented lines
            while (line[0] == '!') {
                line = reader.ReadLine();

                if (line is null) {
                    throw new IOException("Unexpected end of file");
                }
            }

            // Read in the name and the comments
            var thermo = new ThermoListItem();
            thermo.Name = line[..18].Trim();
            thermo.Comments = line.Substring(18, Math.Min(line.Length - 18, 55)).Trim();

            // Read in the next line and check for EOF
            line = reader.ReadLine();
            if (line is null) {
                throw new IOException("Unexpected end of file");
            }

            thermo.NumIntervals = int.Parse(line[..3]);
            thermo.Id = line.Substring(3, 6).Trim();

            // get the chemical formula and coefficient
            // grep the elements (5 max)
            for (var k = 0; k < 5; k++) {
                var tmp2 = line.Substring(k * 8 + 10, 2);

                // Check for an empty place (no more atoms)
                if (tmp2 != "  ") {
                    // Atoms still to be processed

                    // find the atomic number of the element
                    thermo.Elem[k] = Constants.AtomicNumber(tmp2);

                    // And the number of atoms
                    // Should this be an int?  If so, why is it stored in x.2 format?
                    tmp2 = line.Substring(k * 8 + 13, 6);
                    var indx = tmp2.IndexOf('.');
                    if (indx != -1) {
                        thermo.Coef[k] = int.Parse(tmp2[..indx]);
                    } else {
                        thermo.Coef[k] = int.Parse(tmp2);
                    }
                } else {
                    // No atom here
                    thermo.Coef[k] = 0;
                }
            }

            // Grep the state
            if (line[51] == '0') {
                thermo.State = Constants.GAS;
            } else {
                thermo.State = Constants.CONDENSED;
            }

            // Grep the molecular weight
            thermo.Weight = double.Parse(line.Substring(52, 13));

            // grep the heat of formation (J/mol) or enthalpy if condensed
            // The values are assigned in the if block following
            var tmp = line.Substring(65, 15);

            // now get the data
            // there is 'thermo_list[i].nint' set of data 
            if (thermo.NumIntervals == 0) {
                // Set the enthalpy
                thermo.Enth = int.Parse(tmp);

                // condensed phase, different info
                // Read in the next line and check for EOF
                line = reader.ReadLine();
                if (line is null) {
                    throw new IOException("Unexpected end of file");
                }

                // treat the line
                // get the temperature of the assigned enthalpy
                thermo.Temp = (float)double.Parse(line.Substring(1, 10));
            } else {
                // Set the heat of formation
                thermo.Heat = (float)double.Parse(tmp);

                // I'm not quite sure this is necessary
                // if the value is 0 and this is the same substance as the previous one but in a different state ...
                if (thermo.Heat == 0 && i != 0) {
                    for (var j = 0; j < 5; j++) {
                        // set to the same value as the previous one if the same
                        if (thermo.Coef[j] == _items[i - 1].Coef[j] && thermo.Elem[j] == _items[i - 1].Elem[j]) {
                            thermo.Heat = _items[i - 1].Heat;
                        }
                    }
                }

                for (var j = 0; j < thermo.NumIntervals; j++) {
                    // Get the first line of three
                    // Read in the next line and check for EOF
                    line = reader.ReadLine();
                    if (line is null) {
                        throw new IOException("Unexpected end of file");
                    }

                    // low
                    thermo.Range[j][0] = (float)double.Parse(line.Substring(1, 10));

                    // high
                    thermo.Range[j][1] = (float)double.Parse(line.Substring(11, 10));

                    thermo.NumCoef[j] = int.Parse(line.Substring(22, 1));

                    // grep the exponent
                    for (var l = 0; l < 8; l++) {
                        thermo.Ex[j][l] = (int)double.Parse(line.Substring(l * 5 + 23, 5));
                    }

                    // HO(298.15) -HO(0)
                    thermo.Dho = double.Parse(line.Substring(65, 15));

                    // Get the second line of three
                    // Read in the line and check for EOF
                    line = reader.ReadLine();
                    if (line is null) {
                        throw new IOException("Unexpected end of file");
                    }

                    // grep the first data line
                    // there are 5 coefficients
                    for (var l = 0; l < 5; l++) {
                        thermo.Param[j][l] = double.Parse(line.Substring(l * 16, 16));
                    }

                    // Get the third line of three 
                    // Read in the line and check for EOF
                    line = reader.ReadLine();
                    if (line is null) {
                        throw new IOException("Unexpected end of file");
                    }

                    // grep the second data line
                    for (var l = 0; l < 2; l++) {
                        thermo.Param[j][l + 5] = double.Parse(line.Substring(l * 16, 16));
                    }

                    for (var l = 0; l < 2; l++) {
                        thermo.Param[j][l + 7] = double.Parse(line.Substring(l * 16 + 48, 16));
                    }
                }
            }

            _items.Add(thermo);
        }

        if (verbose != 0) {
            Console.WriteLine($"{_items.Count} species loaded.");
        }
    }

    public ThermoListItem this[int index] {
        get {
            return _items[index];
        }
    }

    public int Count {
        get {
            return _items.Count;
        }
    }

    public void PrintList() {
        for (var i = 0; i < _items.Count; i++) {
            Console.WriteLine($"{i,-4} {_items[i].Name,-15} {_items[i].Heat,2: 0.00;-0.00}");
        }
    }

    public int PrintInfo(int sp) {
        if (sp > _items.Count || sp < 0) {
            return -1;
        }

        var s = _items[sp];

        Console.WriteLine("---------------------------------------------");
        Console.WriteLine($"Name: \t\t\t{s.Name}");
        Console.WriteLine($"Comments: \t\t{s.Comments}");
        Console.WriteLine($"Id: \t\t\t{s.Id}");
        Console.Write($"Chemical formula:\t");

        for (var i = 0; i < 5; i++) {
            if (s.Coef[i] != 0) {
                Console.Write($"{s.Coef[i]}{Constants.Symb[s.Elem[i]]}");
            }
        }
        Console.WriteLine();
        Console.Write("State:\t\t\t");
        switch (s.State) {
            case Constants.GAS: {
                    Console.WriteLine("GAZ");
                    break;
                }

            case Constants.CONDENSED: {
                    Console.WriteLine("CONDENSED");
                    break;
                }

            default: {
                    Console.WriteLine("UNRECOGNIZE");
                    break;
                }
        }

        Console.WriteLine();
        Console.WriteLine($"Molecular weight: \t\t{s.Weight:0.000000} g/mol");
        Console.WriteLine($"Heat of formation at 298.15 K : {s.Heat:0.000000} J/mol");
        Console.WriteLine($"Assign enthalpy               : {s.Enth:0.000000} J/mol");
        Console.WriteLine($"HO(298.15) - HO(0): \t\t{s.Dho:0.000000} J/mol");
        Console.WriteLine($"Number of temperature range: {s.NumIntervals}\n");

        for (var i = 0; i < s.NumIntervals; i++) {
            Console.WriteLine($"Interval: {s.Range[i][0]:0.000000} - {s.Range[i][1]:0.000000}");
            for (var j = 0; j < 9; j++) {
                Console.Write($"{s.Param[i][j]: 0.000000000e+00;-0.000000000e+00} ");
            }
            Console.WriteLine("\n");
        }
        Console.WriteLine("---------------------------------------------");
        return 0;
    }

    internal int ThermoSearch(string str) {
        int last = -1;

        for (var i = 0; i < _items.Count; i++) {
            if (!string.Equals(str, _items[i].Name, StringComparison.OrdinalIgnoreCase)) {
                last = i;
                Console.WriteLine($"{i} {_items[i].Name}");
            }
        }

        return last;
    }

    // Enthalpy in the standard state (Dimensionless)
    internal double Enthalpy0(int sp, float T) {
        var s = _items[sp];

        double val;
        int pos = 0, i;

        if (T < s.Range[0][0]) {
            // Temperature below the lower range
            pos = 0;
        } else if (T >= s.Range[s.NumIntervals - 1][1]) {
            // Temperature above the higher range 
            pos = s.NumIntervals - 1;
        } else {
            for (i = 0; i < s.NumIntervals; i++) {
                // Find the range
                if ((T >= s.Range[i][0]) && (T < s.Range[i][1])) {
                    pos = i;
                }
            }
        }

        // parametric equation for dimentionless enthalpy
        val = -s.Param[pos][0] * Math.Pow(T, -2) + s.Param[pos][1] * Math.Pow(T, -1) * Math.Log(T)
            + s.Param[pos][2] + s.Param[pos][3] * T / 2 + s.Param[pos][4] * Math.Pow(T, 2) / 3
            + s.Param[pos][5] * Math.Pow(T, 3) / 4 + s.Param[pos][6] * Math.Pow(T, 4) / 5
            + s.Param[pos][7] / T;

        return val; // dimensionless enthalpy 
    }

    // Entropy in the standard state (Dimensionless)
    internal double Entropy0(int sp, float T) {
        var s = _items[sp];
        double val;
        int pos = 0, i;

        if (T < s.Range[0][0]) {
            pos = 0;
        } else if (T >= s.Range[s.NumIntervals - 1][1]) {
            pos = s.NumIntervals - 1;
        } else {
            for (i = 0; i < s.NumIntervals; i++) {
                if ((T >= s.Range[i][0]) && (T < s.Range[i][1])) {
                    pos = i;
                }
            }
        }

        // parametric equation for dimentionless entropy
        val = -s.Param[pos][0] * Math.Pow(T, -2) / 2 - s.Param[pos][1] * Math.Pow(T, -1)
            + s.Param[pos][2] * Math.Log(T) + s.Param[pos][3] * T
            + s.Param[pos][4] * Math.Pow(T, 2) / 2
            + s.Param[pos][5] * Math.Pow(T, 3) / 3 + s.Param[pos][6] * Math.Pow(T, 4) / 4
            + s.Param[pos][8];

        return val;
    }

    // Specific heat in the standard state (Dimensionless)
    internal double SpecificHeat0(int sp, float T) {
        var s = _items[sp];
        double val;
        int pos = 0, i;

        if (T < s.Range[0][0]) {
            pos = 0;
        } else if (T >= s.Range[s.NumIntervals - 1][1]) {
            pos = s.NumIntervals - 1;
        } else {
            for (i = 0; i < s.NumIntervals; i++) {
                if ((T >= s.Range[i][0]) && (T < s.Range[i][1])) {
                    pos = i;
                }
            }
        }

        // parametric equation for dimentionless specific_heat
        val = s.Param[pos][0] * Math.Pow(T, -2) + s.Param[pos][1] * Math.Pow(T, -1)
            + s.Param[pos][2] + s.Param[pos][3] * T + s.Param[pos][4] * Math.Pow(T, 2)
            + s.Param[pos][5] * Math.Pow(T, 3) + s.Param[pos][6] * Math.Pow(T, 4);

        return val;
    }

    // Dimensionless Gibbs free energy in the standard state
    internal double Gibbs0(int sp, float T) {
        return Enthalpy0(sp, T) - Entropy0(sp, T); // dimensionless
    }

    // Check if the species is in its range of definition
    // false if out of range, true if ok
    internal bool TemperatureCheck(int sp, float T) {
        var s = _items[sp];

        if ((T > s.Range[s.NumIntervals - 1][1]) || (T < s.Range[0][0])) {
            return false;
        }

        return true;
    }

    // Returns the transition temperature of the species
    // considered which is nearest of the temperature T
    internal double TransitionTemperature(int sp, float T) {
        var s = _items[sp];

        // first assume that the lowest temperature is the good one
        var transition_T = s.Range[0][0];

        // verify if we did the good bet
        if (Math.Abs(transition_T - T) > Math.Abs(s.Range[s.NumIntervals - 1][1] - T)) {
            transition_T = s.Range[s.NumIntervals - 1][1];
        }

        return transition_T;
    }

    internal double Entropy(int sp, int st, double ln_nj_n, float T, float P) {
        switch (st) {
            case Constants.GAS: {
                // The thermodynamic data are based on a standard state pressure of 1 bar (10^5 Pa)
                return Entropy0(sp, T) - ln_nj_n - Math.Log(P * Conversion.ATM_TO_BAR);
            }

            case Constants.CONDENSED: {
                return Entropy0(sp, T);
            }
        }

        return 0;
    }

    // J/mol T is in K, P is in atm
    internal double Gibbs(int sp, int st, double ln_nj_n, float T, float P) {
        switch (st) {
            case Constants.GAS: {
                return Gibbs0(sp, T) + ln_nj_n + Math.Log(P * Conversion.ATM_TO_BAR);
            }

            case Constants.CONDENSED: {
                return Gibbs0(sp, T);
            }
        }

        return 0;
    }


    /// <summary>
    /// Return the stochiometric coefficient of an element in a molecule. If the element isn't present, it return 0.
    /// 
    /// AUTHOR:   Antoine Lefebvre
    /// </summary>
    /// <remarks>
    /// There is a different function for the product and for the propellant.
    /// </remarks>
    internal int ProductElementCoef(int element, int molecule) {
        for (var i = 0; i < 5; i++) {
            if (_items[molecule].Elem[i] == element) {
                return _items[molecule].Coef[i];
            }
        }

        return 0;
    }
}