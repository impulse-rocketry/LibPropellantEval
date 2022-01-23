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

public class Equilibrium {
    public bool EquilibriumOk;  // true if the equilibrium have been compute
    public bool PropertiesOk;   // true if the properties have been compute
    public bool PerformanceOk;  // true if the performance have been compute

    //temporarily
    public double Entropy;

    public IterationInfo IterationInfo = new();
    public Composition Propellant = new();
    public Product Product = new();
    public EquilibriumProperties Properties = new();
    public PerformanceProperties Performance = new();

    public double ProductMolarMass {
        get {
            return 1 / IterationInfo.N;
        }
    }

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
    /// <param name="e">The equilibrium structure</param>
    /// <param name="sp">The number of the molecule in the list</param>
    /// <param name="mol">Quantity in mol</param>
    public void AddInPropellant(int sp, double mol) {
        Propellant.Molecule[Propellant.NumComponents] = (short)sp;
        Propellant.Coef[Propellant.NumComponents] = mol;
        Propellant.NumComponents++;
    }
}
