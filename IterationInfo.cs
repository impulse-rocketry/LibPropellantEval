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

// Holds information during the iteration procedure
public class IterationInfo {
    public double N;                        // mol/g of the mixture
    public double LnN;                      // ln(n)
    public double SumN;                     // sum of all the nj
    public double DeltaLnN;                 // delta ln(n) in the iteration process
    public double DeltaLnT;                 // delta ln(T) in the iteration process
    public double[] DeltaLnNj = new double[Constants.MAX_PRODUCT];  // delta ln(nj) in the iteration process
    public double[] LnNj = new double[Constants.MAX_PRODUCT];       // ln(nj) nj are the individual mol/g

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