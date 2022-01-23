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

public partial class Evaluator {
    private double PropellantEnthalpy(Equilibrium e) {
        double h = 0.0;
        for (var i = 0; i < e.Propellant.NumComponents; i++) {
            h += e.Propellant.Coef[i] * PropellantList.HeatOfFormation(e.Propellant.Molecule[i]) / PropellantMass(e);
        }
        return h;
    }

    private double ProductEnthalpy(Equilibrium e) {
        double h = 0.0;

        for (var i = 0; i < e.Product.NumSpecies[Constants.GAS]; i++) {
            h += e.Product.Coef[Constants.GAS][i] * ThermoList.Enthalpy0(e.Product.Species[Constants.GAS][i], (float)e.Properties.T);
        }

        for (var i = 0; i < e.Product.NumSpecies[Constants.CONDENSED]; i++) {
            h += e.Product.Coef[Constants.CONDENSED][i] * ThermoList.Enthalpy0(e.Product.Species[Constants.CONDENSED][i], (float)e.Properties.T);
        }

        return h;
    }

    private double ProductEntropy(Equilibrium e) {
        double ent = 0.0;

        for (var i = 0; i < e.Product.NumSpecies[Constants.GAS]; i++) {
            ent += e.Product.Coef[Constants.GAS][i] * ThermoList.Entropy(e.Product.Species[Constants.GAS][i], Constants.GAS,
                                            e.IterationInfo.LnNj[i] - e.IterationInfo.LnN,
                                            (float)e.Properties.T, (float)e.Properties.P);
        }

        for (var i = 0; i < e.Product.NumSpecies[Constants.CONDENSED]; i++) {
            ent += e.Product.Coef[Constants.CONDENSED][i] * ThermoList.Entropy(e.Product.Species[Constants.CONDENSED][i],
                                                Constants.CONDENSED, 0, (float)e.Properties.T, (float)e.Properties.P);
        }

        return ent;
    }

    // The specific heat of the mixture for frozen performance
    private double MixtureSpecificHeat0(Equilibrium e, double temp) {
        double cp = 0.0;

        // for gases
        for (var i = 0; i < e.Product.NumSpecies[Constants.GAS]; i++) {
            cp += e.Product.Coef[Constants.GAS][i] * ThermoList.SpecificHeat0(e.Product.Species[Constants.GAS][i], (float)temp);
        }

        // for condensed
        for (var i = 0; i < e.Product.NumSpecies[Constants.CONDENSED]; i++) {
            cp += e.Product.Coef[Constants.CONDENSED][i] *
            ThermoList.SpecificHeat0(e.Product.Species[Constants.CONDENSED][i], (float)temp);
        }

        return cp;
    }

    public int ComputeDensity(Composition c) {
        double mass = 0;

        c.Density = 0.0;

        for (var i = 0; i < c.NumComponents; i++) {
            mass += c.Coef[i] * PropellantList.MolarMass(c.Molecule[i]);
        }

        for (var i = 0; i < c.NumComponents; i++) {
            if (PropellantList[c.Molecule[i]].Density != 0.0) {
                c.Density += c.Coef[i] * PropellantList.MolarMass(c.Molecule[i])
                    / (mass * PropellantList[c.Molecule[i]].Density);
            }
        }

        if (c.Density != 0.0) {
            c.Density = 1 / c.Density;
        }

        return 0;
    }
}