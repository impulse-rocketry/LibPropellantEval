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
    /// <summary>
    /// true if element have been listed
    /// </summary>
    public bool ElementListed;

    /// <summary>
    /// true if product have been listed
    /// </summary>
    public bool ProductListed;

    /// <summary>
    /// true if equilibrium is ok
    /// </summary>
    public bool IsEquilibrium;

    /// <summary>
    /// coefficient matrix for the gases
    /// </summary>
    public int[][] A = Utils.Make2DArray<int>(Constants.MAX_ELEMENT, Constants.MAX_PRODUCT);

    /// <summary>
    /// Number of different elements
    /// </summary>
    public int NumElements;

    /// <summary>
    /// Element list
    /// </summary>
    public int[] Elements = new int[Constants.MAX_ELEMENT];

    /// <summary>
    /// Number of species for each state
    /// </summary>
    public int[] NumSpecies = new int[Constants.STATE_LAST];

    /// <summary>
    /// Number of total possible condensed
    /// </summary>
    public int NumCondensed;

    /// <summary>
    /// Possible species in each state
    /// </summary>
    public int[][] Species = Utils.Make2DArray<int>(Constants.STATE_LAST, Constants.MAX_PRODUCT);

    /// <summary>
    /// Coefficients of each molecule
    /// </summary>
    public double[][] Coef = Utils.Make2DArray<double>(Constants.STATE_LAST, Constants.MAX_PRODUCT);

    /// <summary>
    /// Creates a copy of this object
    /// </summary>
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
