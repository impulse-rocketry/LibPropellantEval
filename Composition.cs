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
/// Hold the composition of a specific propellant
///   ncomp is the number of component
///   molecule[ ] hold the number in propellant_list corresponding
///               to the molecule
///   coef[ ] hold the stochiometric coefficient
///
/// NOTE: It should be great to allocate the memory of the array in 
///      function of the number of element
///
/// DATE: February 6, 2000
/// </summary>
public class Composition {
    /// <summary>
    /// Number of different components
    /// </summary>
    public int NumComponents;

    /// <summary>
    /// Molecule codes
    /// </summary
    public int[] Molecule = new int[Constants.MAX_COMP];

    /// <summary>
    /// Moles of molecule
    /// </summary
    public double[] Coef = new double[Constants.MAX_COMP];

    /// <summary>
    /// Density of propellant
    /// </summary
    public double Density;

    /// <summary>
    /// Returns a copy of this object
    /// </summary
    public Composition Clone() {
        return (Composition)MemberwiseClone();
    }
}