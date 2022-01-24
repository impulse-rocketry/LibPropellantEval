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
/// Holds information during the iteration procedure
/// </summary>
public class IterationInfo {
    /// <summary>
    /// mol/g of the mixture
    /// </summary>
    public double N;

    /// <summary>
    /// ln(n)
    /// </summary>
    public double LnN;

    /// <summary>
    /// sum of all the nj
    /// </summary>
    public double SumN;

    /// <summary>
    /// delta ln(n) in the iteration process
    /// </summary>
    public double DeltaLnN;

    /// <summary>
    /// delta ln(T) in the iteration process
    /// </summary>
    public double DeltaLnT;

    /// <summary>
    /// delta ln(nj) in the iteration process
    /// </summary>
    public double[] DeltaLnNj = new double[Constants.MAX_PRODUCT];

    /// <summary>
    /// ln(nj) nj are the individual mol/g
    /// </summary>
    public double[] LnNj = new double[Constants.MAX_PRODUCT];

    /// <summary>
    /// Returns a copy of this object
    /// </summary>
    public IterationInfo Clone() {
        return new IterationInfo{
            N = N,
            LnN = LnN,
            SumN = SumN,
            DeltaLnN = DeltaLnN,
            DeltaLnT = DeltaLnT,
            DeltaLnNj = (double[])DeltaLnNj.Clone(),
            LnNj = (double[])LnNj.Clone()
        };
    }
}