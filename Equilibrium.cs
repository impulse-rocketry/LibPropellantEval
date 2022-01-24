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
/// Equilibrium properties
/// </summary>
public class Equilibrium {

    /// <summary>
    /// true if the equilibrium have been computed
    /// </summary>
    public bool EquilibriumOk;  

    /// <summary>
    /// true if the properties have been computed
    /// </summary>
    public bool PropertiesOk;

    /// <summary>
    /// true if the performance have been computed
    /// </summary>
    public bool PerformanceOk;

    /// <summary>
    /// The entropy
    /// </summary>
    public double Entropy;

    /// <summary>
    /// Holds information during the iteration procedure
    /// </summary>
    public IterationInfo IterationInfo = new();

    /// <summary>
    /// The composition of the propellants
    /// </summary>
    public Composition Propellant = new();

    /// <summary>
    /// The composition of the combustion product.
    /// </summary>
    public Product Product = new();

    /// <summary>
    /// Holds information on equilibrium properties once it has been computed
    /// </summary>
    public EquilibriumProperties Properties = new();

    /// <summary>
    /// Holds the performance properties
    /// </summary>
    public PerformanceProperties Performance = new();

    /// <summary>
    /// Returns the products molar mass
    /// </summary>
    public double ProductMolarMass {
        get {
            return 1 / IterationInfo.N;
        }
    }

    /// <summary>
    /// Returns a copy of this object
    /// </summary>
    public void CopyTo(Equilibrium dest) {
        dest.Entropy = Entropy;
        dest.EquilibriumOk = EquilibriumOk;
        dest.IterationInfo = IterationInfo.Clone();
        dest.Performance = Performance.Clone();
        dest.PerformanceOk = PerformanceOk;
        dest.Product = Product.Clone();
        dest.Propellant = Propellant.Clone();
        dest.Properties = Properties.Clone();
        dest.PropertiesOk = PropertiesOk;
    }

    /// <summary>
    /// Resets the items in the list of elements
    /// </summary>
    public void ResetElementList() {
        for (var i = 0; i < Constants.MAX_ELEMENT; i++) {
            Product.Elements[i] = -1;
        }
    }

    /// <summary>
    /// Add a new molecule in the propellant
    /// 
    /// AUTHOR:    Antoine Lefebvre
    /// </summary>
    /// <param name="sp">The number of the molecule in the list</param>
    /// <param name="mol">Quantity in mol</param>
    public void AddInPropellant(int sp, double mol) {
        Propellant.Molecule[Propellant.NumComponents] = (short)sp;
        Propellant.Coef[Propellant.NumComponents] = mol;
        Propellant.NumComponents++;
    }
}
