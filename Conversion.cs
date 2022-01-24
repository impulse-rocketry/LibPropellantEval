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
/// Unit conversion
/// </summary>
public static class Conversion {
    /// <summary>
    /// Transform calories to joules
    /// </summary>
    public const double CAL_TO_JOULE = 4.1868;

    /// <summary>
    /// Transform pound/(cubic inch) to gram/(cubic centimeter)
    /// </summary>
    public const double LBS_IN3_TO_G_CM3 = 27.679905;

    /// <summary>
    /// Pressure Unit - Standard atmosphere to Pascal
    /// </summary>
    public const double ATM_TO_PA = 101325.0;

    /// <summary>
    /// Pressure Unit - Standard atmosphere to Pounds per square inch
    /// </summary>
    public const double ATM_TO_PSI = 14.695949;

    /// <summary>
    /// Pressure Unit - Standard atmosphere to Bar
    /// </summary>
    public const double ATM_TO_BAR = 1.01325;

    /// <summary>
    /// Pressure Unit - Bar to Pounds per square inch
    /// </summary>
    public const double BAR_TO_PSI = 14.503774;

    /// <summary>
    /// Pressure Unit - Bar to Standard atmosphere
    /// </summary>
    public const double BAR_TO_ATM = 0.98692327;

    /// <summary>
    /// Pressure Unit - Pounds per square inch to Standard atmosphere
    /// </summary>
    public const double PSI_TO_ATM = 0.068045964;

    /// <summary>
    /// Pressure Unit - Kilopascal to Standard atmosphere
    /// </summary>
    public const double KPA_TO_ATM = 0.0098692327;

    /// <summary>
    /// Length - Meters to Centimeters
    /// </summary>
    public const double M_TO_CM = 100.0;

    /// <summary>
    /// Length - Meters to Inches
    /// </summary>
    public const double M_TO_IN = 39.370079;

    /// <summary>
    /// Length - Inches to Meters
    /// </summary>
    public const double IN_TO_M = 0.0254;

    /// <summary>
    /// Surface - Square meters to Square centimeters
    /// </summary>
    public const double M2_TO_CM2 = 10000.0;

    /// <summary>
    /// Surface - Square meters to square inches
    /// </summary>
    public const double M2_TO_IN2 = 1550.0031;

    /// <summary>
    /// Volume - Cubic meters to cubic centimeters
    /// </summary>
    public const double M3_TO_CM3 = 1000000.0;

    /// <summary>
    /// Volume - Cubic meters to cubic inches
    /// </summary>
    public const double M3_TO_IN3 = 61023.744;

    /// <summary>
    /// Mass flow - Kilograms/second to Pounds/second
    /// </summary>
    public const double KG_S_TO_LB_S = 2.2046226;

    /// <summary>
    /// Force - Newtons to Pound-force
    /// </summary>
    public const double N_TO_LBF = 0.22480894;

    /// <summary>
    /// Force - Pound-force to Newtons
    /// </summary>
    public const double LBF_TO_N = 4.4482216;
}