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
/// Hold the composition of the combustion product. The molecule
/// are separate between their different possible state.
///
/// NOTE: This structure should be initialize with the function 
///       initialize_product.
/// 
/// DATE: February 13, 2000
/// </summary>
public class Product {
    public bool ElementListed;                 // true if element have been listed
    public bool ProductListed;                 // true if product have been listed
    public bool IsEquilibrium;                 // true if equilibrium is ok

    // coefficient matrix for the gases
    public int[][] A = Utils.Make2DArray<int>(Constants.MAX_ELEMENT, Constants.MAX_PRODUCT);

    public int NumElements;                                                     // Number of different element
    public int[] Elements = new int[Constants.MAX_ELEMENT];                     // Element list
    public int[] NumSpecies = new int[Constants.STATE_LAST];                    // Number of species for each state
    public int NumCondensed;                                                    // Number of total possible condensed
    public int[][] Species = Utils.Make2DArray<int>(Constants.STATE_LAST, Constants.MAX_PRODUCT);       // Possible species in each state
    public double[][] Coef = Utils.Make2DArray<double>(Constants.STATE_LAST, Constants.MAX_PRODUCT);    // Coefficients of each molecule

    public Product Clone() {
        return new Product {
            ElementListed = ElementListed,
            ProductListed = ProductListed,
            IsEquilibrium = IsEquilibrium,
            A = A.Select(a => a.ToArray()).ToArray(),
            NumElements = NumElements,
            Elements = (int[])Elements.Clone(),
            NumSpecies = (int[])NumSpecies.Clone(),
            NumCondensed = NumCondensed,
            Species = Species.Select(a => a.ToArray()).ToArray(),
            Coef = Coef.Select(a => a.ToArray()).ToArray()
        };
    }
}
