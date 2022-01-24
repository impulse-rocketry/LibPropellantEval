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
/// Utility functions
/// </summary>
public static class Utils {
    /// <summary>
    /// Create an array of arrays of type T.
    /// </summary>
    public static T[][] Make2DArray<T>(int d1, int d2) {
        var result = new T[d1][];

        for (var i = 0; i < d1; i++) {
            result[i] = new T[d2];
        }

        return result;
    }

    internal static double Min(double a, double b, double c) => Math.Min(Math.Min(a, b), c);

    internal static double Max(double a, double b, double c) => Math.Max(Math.Max(a, b), c);
}

