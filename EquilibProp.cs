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
/// Hold information on equilibrium properties once it has been computed
///
/// June 14, 2000
/// </summary>
public class EquilibriumProperties {
    public double P;    // Pressure (atm)
    public double T;    // Temperature (K)
    public double H;    // Enthalpy (kJ/kg)
    public double U;    // Internal energy (kJ/kg)
    public double G;    // Gibbs free energy (kJ/kg)
    public double S;    // Entropy (kJ/(kg)(K))
    public double M;    // Molar mass (g/mol)
    public double dV_P; // (d ln(V) / d ln(P))t
    public double dV_T; // (d ln(V) / d ln(T))p
    public double Cp;   // Specific heat (kJ/(kg)(K))
    public double Cv;   // Specific heat (kJ/(kg)(K))
    public double Isex; // Isentropic exponent (gamma)
    public double Vson; // Sound speed (m/s)

    public EquilibriumProperties Clone() {
        return (EquilibriumProperties)MemberwiseClone();
    }
}