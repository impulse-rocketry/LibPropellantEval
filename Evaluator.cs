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

/* thermo.c  -  Compute thermodynamic properties of individual
                species and composition of species           */
/* $Id: thermo.c,v 1.2 2000/08/06 00:19:14 antoine Exp $ */
/* Copyright (C) 2000                                                  */
/*    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>                   */
/*    Mark Pinese <pinese@cyberwizards.com.au>                         */
/*                                                                     */
/* Licensed under the GPLv2                                            */

namespace ImpulseRocketry.LibPropellantEval;

/// <summary>
/// The evaluator
/// </summary>
public partial class Evaluator {
    // 1 for verbose, 0 for non-verbose
    private readonly int _verbose = 10;

    /// <summary>
    /// Gets the propellant list
    /// </summary>
    public PropellantList PropellantList { get; }

    /// <summary>
    /// Gets the thermo list
    /// </summary>
    public ThermoList ThermoList { get; }

    /// <summary>
    /// Initialises a new instance of the <see ref="Evaluator"/> class with internal propellant and thermo list data
    /// </summary>
    public Evaluator(int verbose) {
        _verbose = verbose;
        PropellantList = new PropellantList(verbose);
        ThermoList = new ThermoList(verbose);
    }

    /// <summary>
    /// Initialises a new instance of the <see ref="Evaluator"/> class with the specified propellant and thermo lists
    /// </summary>
    public Evaluator(PropellantList propellantList, ThermoList thermoList, int verbose) {
        PropellantList = propellantList;
        ThermoList = thermoList;
        _verbose = verbose;
    }
}