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
    /// <summary>
    /// Pressure (atm)
    /// </summary>
    public double P;

    /// <summary>
    /// Temperature (K)
    /// </summary>
    public double T;

    /// <summary>
    /// Enthalpy (kJ/kg)
    /// </summary>
    public double H;

    /// <summary>
    /// Internal energy (kJ/kg)
    /// </summary>
    public double U;

    /// <summary>
    /// Gibbs free energy (kJ/kg)
    /// </summary>
    public double G;

    /// <summary>
    /// Entropy (kJ/(kg)(K))
    /// </summary>
    public double S;

    /// <summary>
    /// Molar mass (g/mol)
    /// </summary>
    public double M;

    /// <summary>
    /// (d ln(V) / d ln(P))t
    /// </summary>
    public double dV_P;

    /// <summary>
    /// (d ln(V) / d ln(T))p
    /// </summary>
    public double dV_T;

    /// <summary>
    /// Specific heat (kJ/(kg)(K))
    /// </summary>
    public double Cp;

    /// <summary>
    /// Specific heat (kJ/(kg)(K))
    /// </summary>
    public double Cv;

    /// <summary>
    /// Isentropic exponent (gamma)
    /// </summary>
    public double Isex;

    /// <summary>
    /// Sound speed (m/s)
    /// </summary>
    public double Vson;

    /// <summary>
    /// Returns a copy of this object
    /// </summary>
    public EquilibriumProperties Clone() {
        return (EquilibriumProperties)MemberwiseClone();
    }
}