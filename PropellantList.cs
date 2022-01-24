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
/// Structure to hold information of species contain in the propellant data file
/// </summary>
public class PropellantListItem {
    /// <summary>
    /// Name of the propellant
    /// </summary>
    public string? Name;

    /// <summary>
    /// Element in the molecule (atomic number) max 6
    /// </summary>
    public int[] Elem = new int[6];

    /// <summary>
    /// Stochiometric coefficient of this element (0 for none)
    /// </summary>
    public int[] Coef = new int[6];

    /// <summary>
    /// Heat of formation in Joule/gram
    /// </summary>
    public float Heat;

    /// <summary>
    /// Density in g/cubic cm
    /// </summary>
    public float Density;
}

/// <summary>
/// List of propellant information
/// </summary>
public class PropellantList {
    private readonly List<PropellantListItem> _items = new();
    
    /// <summary>
    /// Initialises a new instance of the <see name="PropellantList"/> class with the built in propellant data.
    /// </summary>
    public PropellantList(int verbose) {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using var resourceStream = assembly.GetManifestResourceStream("ImpulseRocketry.LibPropellantEval.data.propellant.dat");

        if (resourceStream is null) {
            throw new ApplicationException("Unable to load propellant data from internal resources.");
        }

        Load(resourceStream, verbose);
    }

    /// <summary>
    /// Initialises a new instance of the <see name="PropellantList"/> class with propellant data from the specified file.
    /// </summary>
    public PropellantList(string fileName, int verbose) {
        using var fileStream = File.OpenRead(fileName);
        Load(fileStream, verbose);
    }

    private void Load(Stream stream, int verbose) { 
        using var reader = new StreamReader(stream);

        if (verbose != 0) {
            Console.Write("Scanning propellant data file...");
        }

        var numPropellant = 0;
        int nameStart, nameEnd, nameLen;

        // Scan propellant.dat to find the number of positions in propellant_list to allocate
        var line = reader.ReadLine();
        while (line is not null) {
            // All that is required is to count the number of lines not starting with '*' or '+'
            if (line[0] != '*' && line[0] != '+') {
                numPropellant++;
            }

            line = reader.ReadLine();
        }

        // Reset the file pointer
        stream.Position = 0;

        if (verbose != 0) {
            Console.Write($"\nScan complete.  {numPropellant} records found.  Allocating memory...");
        }

        _items.EnsureCapacity(numPropellant);

        if (verbose != 0) {
            Console.Write("\nSuccessful.  Loading propellant data file...");
        }

        line = reader.ReadLine();
        if (line is null) {
            throw new IOException("Unexpected end of file");
        }

        var propellant = new PropellantListItem();

        for (var i = 0; i < numPropellant; i++) {
            // Skip commented code
            do {
                line = reader.ReadLine();
                if (line is null) {
                    throw new IOException("Unexpected end of file");
                }
            } while (line[0] == '*');

            // Check for a continued name
            while (line[0] == '+') {
                // A continued name found
                var tmp = line.Substring(9, 70);

                // Find the end of the whitespaces.  name_start + 1 is used to leave one space.
                for (nameStart = 0; nameStart < 70; nameStart++) {
                    if (tmp[nameStart + 1] != ' ') {
                        break;
                    }
                }

                // Find the end of the name.  > 0 is used to be consistent with the one space left
                // when finding name_start
                for (nameEnd = 69; nameEnd > 0; nameEnd--) {
                    if (tmp[nameEnd] != ' ') {
                        break;
                    }
                }

                nameLen = nameEnd - nameStart + 1;
                // Concatenate the entire string
                propellant.Name += tmp.Substring(nameStart, nameLen);

                // Processing of this line is done, so get the next one
                line = reader.ReadLine();
                if (line is null) {
                    throw new IOException("Unexpected end of file");
                }
            }

            propellant = new PropellantListItem();

            // Grep the name
            propellant.Name = line.Substring(9, 30).TrimEnd();

            for (var j = 0; j < 6; j++) {
                propellant.Coef[j] = int.Parse(line.Substring(j * 5 + 39, 3));

                // Find the atomic number of the element
                propellant.Elem[j] = Constants.AtomicNumber(line.Substring(j * 5 + 42, 2));
            }

            propellant.Heat = (float)(double.Parse(line.Substring(69, 5)) * Conversion.CAL_TO_JOULE);
            propellant.Density = (float)(double.Parse(line.Substring(75, 5)) * Conversion.LBS_IN3_TO_G_CM3);

            _items.Add(propellant);
        }

        if (verbose != 0) {
            Console.WriteLine($"{_items.Count} species loaded.");
        }
    }

    /// <summary>
    /// Gets the <see cref="PropellantListItem"/> at the specified index.
    /// </summary>
    public PropellantListItem this[int index] {
        get {
            return _items[index];
        }
    }

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count {
        get {
            return _items.Count;
        }
    }

    /// <summary>
    /// Convert grams to moles.
    /// </summary>
    public double GramToMol(double g, int sp) => g / MolarMass(sp);

    internal double MolarMass(int molecule) {
        int i = 0;
        double ans = 0;

        var coef_list = _items[molecule].Coef;
        var elem_list = _items[molecule].Elem;

        var coef = coef_list[i];
        while (coef != 0) {
            ans += coef * Constants.MolarMass[elem_list[i]];
            i++;
            coef = coef_list[i];
        }

        return ans;
    }

    /// <summary>
    /// J/mol
    /// </summary>
    public double HeatOfFormation(int molecule) {
        return _items[molecule].Heat * MolarMass(molecule);
    }

    /// <summary>
    /// Search for the propellant with the specified name.
    /// </summary>
    public int Search(string str) {
        int last = -1;

        for (var i = 0; i < _items.Count; i++) {
            if (!string.Equals(str, _items[i].Name, StringComparison.OrdinalIgnoreCase)) {
                last = i;
                Console.WriteLine($"{i} {_items[i].Name}");
            }
        }

        return last;
    }

    /// <summary>
    /// Returns the offset of the molecule in the propellant list
    /// the argument is the chemical formula of the molecule
    /// </summary>    
    public int SearchByFormula(string str) {
        int i = 0, j;

        char[] tmp = new char[5];
        int ptr;

        int[] elem = new[] { 0, 0, 0, 0, 0, 1 };
        int[] coef = new[] { 0, 0, 0, 0, 0, 0 };

        int molecule = -1;

        ptr = 0; // Beginning of the string

        while ((i < 6) && (ptr < str.Length)) {
            if (Char.IsUpper(str[ptr]) && Char.IsLower(str[ptr + 1]) && (Char.IsUpper(str[ptr + 2]) || Char.IsControl(str[ptr + 2]))) {
                tmp[0] = str[ptr];
                tmp[1] = Char.ToUpper(str[ptr + 1]);
                tmp[2] = '\0';
                // Find the atomic number of the element
                elem[i] = Constants.AtomicNumber(new String(tmp));
                coef[i] = 1;
                i++;
                ptr += 2;
            } else if (Char.IsUpper(str[ptr]) && (Char.IsUpper(str[ptr + 1]) || Char.IsControl(str[ptr + 1]))) {
                tmp[0] = str[ptr];
                tmp[1] = ' ';
                tmp[2] = '\0';
                elem[i] = Constants.AtomicNumber(new String(tmp));
                coef[i] = 1;
                i++;
                ptr++;
            } else if (Char.IsUpper(str[ptr]) && Char.IsDigit(str[ptr + 1])) {
                tmp[0] = str[ptr];
                tmp[1] = ' ';
                tmp[2] = '\0';
                elem[i] = Constants.AtomicNumber(new String(tmp));

                j = 0;
                do {
                    tmp[j] = str[ptr + 1 + j];
                    j++;
                } while (Char.IsDigit(str[ptr + 1 + j]));

                tmp[j] = '\0';

                coef[i] = int.Parse(new String(tmp));
                i++;

                ptr = ptr + j + 1;
            } else if (Char.IsUpper(str[ptr]) && Char.IsLower(str[ptr + 1]) && Char.IsDigit(str[ptr + 2])) {
                tmp[0] = str[ptr];
                tmp[1] = Char.ToUpper(str[ptr + 1]);
                tmp[2] = '\0';
                elem[i] = Constants.AtomicNumber(new String(tmp));

                j = 0;
                while (Char.IsDigit(str[ptr + 2 + j])) {
                    tmp[j] = str[ptr + 1 + j];
                    j++;
                }
                tmp[j] = '\0';

                coef[i] = int.Parse(new String(tmp));
                i++;

                ptr = ptr + j + 2;
            }
        }

        for (i = 0; i < _items.Count; i++) {
            for (j = 0; j < 6; j++) {
                // Set to the same value as the previous one if the same
                if (!((_items[i].Coef[j] == coef[j]) && (_items[i].Elem[j] == elem[j]))) {
                    break;
                }
            }

            if (j == 5) {
                // We found the molecule, check if the inverse is true
                molecule = i;
                break;
            }
        }

        return molecule;
    }

    /// <summary>
    /// Print list of all propellant info.
    /// </summary>
    public void PrintList() {
        for (var i = 0; i < _items.Count; i++) {
            Console.WriteLine($"{i,-4} {_items[i].Name,-30} {_items[i].Heat,5:0.#######}");
        }
    }

    /// <summary>
    /// Print propellant info for the specifed species.
    /// </summary>
    public int PrintInfo(int sp) {
        if (sp >= _items.Count || sp < 0) {
            return -1;
        }

        Console.WriteLine($"Code {"Name",-35} Enthalpy  Density  Composition", "Name");
        Console.Write($"{sp}  {_items[sp].Name,-35} {_items[sp].Heat,-4:0.####}  {_items[sp].Density,-2:0.##}");
        Console.Write("  ");

        // Print the composition
        for (var j = 0; j < 6; j++) {
            if (_items[sp].Coef[j] != 0) {
                Console.Write($"{_items[sp].Coef[j]}{Constants.Symb[_items[sp].Elem[j]]} ");
            }
        }
        Console.WriteLine();

        return 0;
    }

    internal int PropellantElementCoef(int element, int molecule) {
        for (var i = 0; i < 6; i++) {
            if (_items[molecule].Elem[i] == element) {
                return _items[molecule].Coef[i];
            }
        }
        return 0;
    }
}