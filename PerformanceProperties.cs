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
/// Note: Specific impulse have unit of m/s
///   Ns/kg = (kg m / s^2) * (s / kg)
///         = (m / s)
///
/// It is habitual to found in literature specific impulse in units of second.
/// It is in reality Isp/g where g is the earth acceleration.
/// </summary>
public class PerformanceProperties {
    public double AeAt;   // Exit area / Throat area
    public double ADotm;  // Exit area / mass flow rate (m/s/atm)
    public double Cstar;   // Characteristic velocity
    public double Cf;      // Coefficient of thrust
    public double Ivac;    // Specific impulse (vacuum)
    public double Isp;     // Specific impulse

    public PerformanceProperties Clone() {
        return (PerformanceProperties)MemberwiseClone();
    }
}