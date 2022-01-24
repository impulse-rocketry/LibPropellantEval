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
/// Constants
/// </summary>
public class Constants {

    internal const int MAX_PRODUCT = 400; // Maximum species in product
    internal const int MAX_ELEMENT = 15;  // Maximum different element
    internal const int MAX_COMP = 20;     // Maximum different ingredient in composition

    /// <summary>
    /// Gas substance state
    /// </summary>
    public const int GAS = 0;

    /// <summary>
    /// Condenses substance state
    /// </summary>
    public const int CONDENSED = 1;

    internal const int STATE_LAST = 2;

    // Molar gaz constant in J/(mol K)
    internal const double R = 8.31451;

    // Earth gravitational acceleration
    internal const double Ge = 9.80665;

    /// <summary>
    /// Contains the molar mass of element by atomic number molar_mass[0]
    /// contains hydrogen and so on.  Data come from Sargent-Welch 1996.
    /// </summary>
    internal readonly static float[] MolarMass = new float[]{
        1.00794f,   4.002602f, 6.941f,      9.012182f, 10.811f,    12.0107f,
        14.00674f,  15.9994f,  18.9984032f, 20.11797f, 22.989770f, 24.305f,
        26.981538f, 28.0855f,  30.973761f,  32.066f,   35.4527f,   39.948f,
        39.0983f,   40.078f,   44.95591f,   47.88f,    50.9415f,   51.996f,
        54.938f,    55.847f,   58.9332f,    58.6934f,  63.546f,    65.39f,
        69.723f,    72.61f,    74.9216f,    78.96f,    79.904f,    83.80f,
        85.4678f,   87.62f,    88.9059f,    91.224f,   92.9064f,   95.94f,
        98.0f,      101.07f,   102.9055f,   106.42f,   107.868f,   112.41f,
        114.82f,    118.71f,   121.757f,    127.60f,   126.9045f,  131.29f,
        132.9054f,  137.33f,   138.9055f,   140.12f,   140.9077f,  144.24f,
        145.0f,     150.36f,   151.965f,    157.25f,   158.9253f,  162.50f,
        164.9303f,  167.26f,   168.9342f,   173.04f,   174.967f,   178.49f,
        180.9479f,  183.85f,   186.207f,    190.2f,    192.22f,    195.08f,
        196.9665f,  200.59f,   204.383f,    207.2f,    208.9804f,  209.0f,
        210.0f,     222.0f,    223.0f,      226.0254f, 227.0f,     232.0381f,
        231.0359f,  238.029f,  237.0482f,   244.0f,    12.011f,    9.01218f,
        10.811f,    24.305f,   26.98154f,   257.0f,    0f,         2f};

    /// <summary>
    /// Contains the symbol of the element in the same way as for the molar mass.
    /// </summary>
    /// <remarks>
    /// It is use in the loading of the data file to recognize the chemical formula.
    /// </remarks>
    internal readonly static string[] Symb = new[] {
        "H ","HE","LI","BE","B ","C ","N ","O ",
        "F ","NE","NA","MG","AL","SI","P ","S ","CL","AR","K ","CA",
        "SC","TI","V ","CR","MN","FE","CO","NI","CU","ZN","GA","GE",
        "AS","SE","BR","KR","RB","SR","Y ","ZR","NB","MO","TC","RU",
        "RH","PD","AG","CD","IN","SN","SB","TE","I ","XE","CS","BA",
        "LA","CE","PR","ND","PM","SM","EU","GD","TB","DY","HO","ER",
        "TM","YB","LU","HF","TA","W ","RE","OS","IR","PT","AU","HG","TL",
        "PB","BI","PO","AT","RN","FR","RA","AC","TH","PA","U ","NP",
        "U6","U5","U1","U2","U3","U4","FM",
        "E ", "D " }; // the E stands for electron and D for deuterium

    /// <summary>
    /// Find the atomic number of the element
    /// </summary>
    public static int AtomicNumber(string symbol) {
        for (var i = 0; i < Symb.Length; i++) {
            if (symbol == Symb[i]) {
                return i;
            }
        }

        return -1;
    }
}